using AngleSharp.Html.Dom;
using BookDL.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using BookDL.Infrastructure.Html;
using OpenQA.Selenium.DevTools.V146.CSS;
using AngleSharp.Dom;

namespace BookDL.Infrastructure.Parser.Narou
{
    public class NarouBookParser : IBookParser
    {
        private record EpirodeInfo(int TotalEpisode, string ChapterTitle, Episode Episode);

        // タイトルページのセレクタ
        private const string TITLE_SELECTOR = "body > div.l-container > main > article > h1";
        private const string NOVEL_AUTHOR_SELECTOR = "body > div.l-container > main > article > div.p-novel__author";
        private const string FIRST_SECTION_LINK_SELECTOR = "a.p-eplist__subtitle";
        // 本文ページのセレクタ
        private const string TITLE_AUTHOR_SELECTOR = "body > div.l-container > main > div.c-announce-box > div > a";
        // 作者ページのセレクタ
        private const string KATAKANA_AUTHOR_SELECTOR = "body > div.l-header > div.p-userheader > div.p-userheader__body > div.p-userheader__username-info";
        private static readonly Regex AUTHOR_PATTERN = new Regex(@"^\s*作者：\s*(.+)\s*$");
        // タイトルページに本文が書いてある場合
        private const string EPISODE_TITLE_SELECTOR = "body > div.l-container > main > article > h1";
        // タイトルページとは別の本文ページの場合
        private const string CHAPTER_TITLE_SELECTOR = "body > div.l-container > main > div.c-announce-box > div > span";
        private const string SECTION_TITLE_SELECTOR = "body > div.l-container > main > article > h1";
        private const string SECTION_NUMBER_SELECTOR = "body > div.l-container > main > article > div.p-novel__number";
        private const string PARAGRAPHS_SELECTOR = "body > div.l-container > main > article > div.p-novel__body > div > p";
        private const string NEXT_CHAPTER_LINK_SELECTOR = "body > div.l-container > main > div.c-pager.c-pager--center > a.c-pager__item.c-pager__item--next";
        private static readonly Regex LINE_ID_PATTERN = new(@"^L\d+$");

        /******************************************************************************************************/
        // ここからスタティックメソッド群

        public static async Task<IBookParser?> CreateAsync(IBrowserService browserService, IHtmlDocument doc, string bookUrl, CancellationToken ct)
        {
            await Task.Yield();
            ct.ThrowIfCancellationRequested();

            var author = string.Empty;
            var authorKatakana = string.Empty;

            var titleElement = doc.QuerySelector(TITLE_SELECTOR);
            if (titleElement == null)
            {
                return null;
            }
            var novelAuthorElement = doc.QuerySelector(NOVEL_AUTHOR_SELECTOR);
            if (novelAuthorElement == null)
            {
                return null;
            }
            var authorAnchorElement = novelAuthorElement.QuerySelector("a") as IHtmlAnchorElement;
            if (authorAnchorElement == null)
            {
                var tempAuthor = novelAuthorElement.TextContent.Trim();
                var match = AUTHOR_PATTERN.Match(tempAuthor);
                if (match.Success)
                {
                    author = match.Groups[1].Value;
                }
            }
            else
            {
                author = authorAnchorElement.TextContent.Trim();
                authorKatakana = await GetAuthorKatakanaAsync(browserService, authorAnchorElement.Href);
            }
            var firstEpisodeUrl = GetFirstEpisodeUrl(doc);
            if (string.IsNullOrWhiteSpace(firstEpisodeUrl))
            {
                return null;
            }
            var bookInfo = new BookInfo(
                BookUrl: bookUrl, // リダイレクトされてdocが示すURLとは違うかもしれないが、ユーザーが指定したURLを保持する
                Title: titleElement.TextContent.Trim(),
                TitleKatakana: string.Empty,
                Author: author,
                AuthorKatakana: authorKatakana);
            return new NarouBookParser(browserService, bookInfo, firstEpisodeUrl);
        }

        private static async Task<string> GetAuthorKatakanaAsync(IBrowserService browserService, string authorPageUrl)
        {
            browserService.Navigate(authorPageUrl);
            var currentUrl = browserService.GetCurrentUrl();
            var html = browserService.GetDom();
            var doc = await AngleSharpHelper.ParseDocumentAsync(html, currentUrl);
            var katakanaAuthor = doc.QuerySelector(KATAKANA_AUTHOR_SELECTOR)?.TextContent.Trim() ?? string.Empty;
            return katakanaAuthor;
        }

        private static string? GetFirstEpisodeUrl(IHtmlDocument doc)
        {
            var mainContents = doc.QuerySelectorAll(PARAGRAPHS_SELECTOR);
            if (mainContents.Length > 0)
            {
                // タイトルと本文が同一ページ、または本文ページ
                return doc.Url;
            }
            // タイトルページ
            var firstPageLinkElement = doc.QuerySelector(FIRST_SECTION_LINK_SELECTOR) as IHtmlAnchorElement;
            if (firstPageLinkElement == null)
            {
                return null;
            }
            return firstPageLinkElement.Href;
        }

        /******************************************************************************************************/
        // ここからインスタンスメンバー
        public BookInfo? BookInfo => _bookInfo;
        private readonly IBrowserService _browserService;
        private readonly BookInfo _bookInfo;
        private readonly string _firstEpisodeUrl;

        public NarouBookParser(IBrowserService browserService, BookInfo bookInfo, string firstEposodeUrl)
        {
            _browserService = browserService;
            _bookInfo = bookInfo;
            _firstEpisodeUrl = firstEposodeUrl;
        }

        public Task InitializeAsync(CancellationToken ct)
        {
            return Task.CompletedTask;
        }

        public async Task<Book> DownloadBookAsync(BookInfo bookInfo, IProgress<DownloadReport> progress, CancellationToken ct)
        {
            var visitedUrls = new HashSet<string>();
            var firstEpisode = default(Episode);
            var chapterList = new List<Chapter>();
            var episodeList = new List<Episode>();
            var chapterTitle = string.Empty;
            var episodeUrl = _firstEpisodeUrl;
            while (!string.IsNullOrWhiteSpace(episodeUrl))
            {
                ct.ThrowIfCancellationRequested();
                // 循環リンクを検出する
                if (!visitedUrls.Add(episodeUrl))
                {
                    break;
                }
                _browserService.Navigate(episodeUrl);
                var currentUrl = _browserService.GetCurrentUrl();
                var html = _browserService.GetDom();
                var doc = await AngleSharpHelper.ParseDocumentAsync(html, currentUrl);
                var episodeInfo = ParseEpisode(doc);
                if (episodeInfo.ChapterTitle != chapterTitle && !string.IsNullOrWhiteSpace(episodeInfo.ChapterTitle))
                {
                    if (episodeList.Count > 0)
                    {
                        var start = episodeList[0].Index;
                        var end = episodeList[^1].Index;
                        var range = new EpisodeRange(start, end);
                        var chapter = new Chapter(chapterTitle, range, episodeList);
                        chapterList.Add(chapter);
                        episodeList = new List<Episode>();
                    }
                    chapterTitle = episodeInfo.ChapterTitle;
                }
                if (firstEpisode == default)
                {
                    firstEpisode = episodeInfo.Episode;
                }
                episodeList.Add(episodeInfo.Episode);
                var nextLinkElement = doc.QuerySelector(NEXT_CHAPTER_LINK_SELECTOR) as IHtmlAnchorElement;
                episodeUrl = nextLinkElement?.Href ?? string.Empty;
                progress.Report(new DownloadReport(firstEpisode.Index, episodeInfo.Episode.Index, episodeInfo.TotalEpisode));
            }
            if (episodeList.Count > 0)
            {
                var start = episodeList[0].Index;
                var end = episodeList[^1].Index;
                var range = new EpisodeRange(start, end);
                var chapter = new Chapter(chapterTitle, range, episodeList);
                chapterList.Add(chapter);
            }

            return new Book(bookInfo, chapterList);
        }

        private EpirodeInfo ParseEpisode(IHtmlDocument doc)
        {
            var chapterTitle = string.Empty;
            var currentEpisodeNumber = 0;
            var totalEpisodeNumber = 0;
            var episodeTitle = string.Empty;

            var sectionNumberText = doc.QuerySelector(SECTION_NUMBER_SELECTOR)?.TextContent.Trim() ?? string.Empty;
            if (sectionNumberText == string.Empty)
            {
                // タイトルと本文が同一ページの場合、セクション番号はないので1とする。
                currentEpisodeNumber = 1;
                totalEpisodeNumber = 1;
                episodeTitle = doc.QuerySelector(TITLE_SELECTOR)?.TextContent.Trim() ?? string.Empty;
            }
            else
            {
                // 本文ページの場合
                var pageNumbers = sectionNumberText.Split('/');
                currentEpisodeNumber = int.Parse(pageNumbers[0].Trim());
                totalEpisodeNumber = int.Parse(pageNumbers[1].Trim());
                // 編(part)のタイトルは要素が存在しないこともある。
                // そのときは空文字列とする。
                var partTitleElement = doc.QuerySelector(CHAPTER_TITLE_SELECTOR) as IHtmlElement;
                chapterTitle = partTitleElement?.TextContent.Trim() ?? string.Empty;
                // 章(chapter)のタイトルは必ず存在するはず。
                episodeTitle = doc.QuerySelector(SECTION_TITLE_SELECTOR)?.TextContent.Trim() ?? string.Empty;
            }

            var paragraphList = new List<ParagraphNode>();
            var srcParas = doc.QuerySelectorAll(PARAGRAPHS_SELECTOR);
            var nodeList = new List<IBookNode>();
            foreach (IHtmlElement srcPara in srcParas)
            {
                var id = srcPara.Id;
                if (string.IsNullOrEmpty(id) || !LINE_ID_PATTERN.IsMatch(id))
                {
                    continue;
                }
                if (AngleSharpHelper.IsSeparator(srcPara))
                {
                    if (nodeList.Count > 0)
                    {
                        var para = new ParagraphNode(nodeList);
                        paragraphList.Add(para);
                        nodeList = new List<IBookNode>();
                    }
                    continue;
                }
                if (nodeList.Count > 0)
                {
                    nodeList.Add(new BreakRowNode());
                }
                nodeList.AddRange(AngleSharpHelper.ConvertParagraph(srcPara));
            }
            if (nodeList.Count > 0)
            {
                var para = new ParagraphNode(nodeList);
                paragraphList.Add(para);
            }

            var episode = new Episode(
                Title: episodeTitle,
                Index: currentEpisodeNumber,
                Paragraphs: paragraphList);
            return new EpirodeInfo(totalEpisodeNumber, chapterTitle, episode);
        }
    }
}
