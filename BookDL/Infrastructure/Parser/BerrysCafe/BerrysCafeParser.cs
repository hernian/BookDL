using AngleSharp.Html.Dom;
using AngleSharp.Dom;
using BookDL.Domain;
using BookDL.Infrastructure.Html;
using OpenQA.Selenium.DevTools.V146.DOM;
using System.Text.RegularExpressions;
using System.Text;
using System.Diagnostics;

namespace BookDL.Infrastructure.Parser.BerrysCafe
{
    public class BerrysCafeParser : IBookParser
    {
        public static class TitlePageSelector
        {
            public const string TITLE_SELECTOR = "#container > main > section.section.bookDetails > div.title-wrap > div.title > h2";
            public const string AUTHOR_SELECTOR = "#container > main > section.section.bookDetails > div.group-wrap > div.group-01 > div > div.subDetails-02 > div > a";
            public const string FIRST_SECTION_LINK_SELECTOR = "link[rel=\"next\"]";
        }
        public static class EpisodePageSelector
        {
            public const string PAGE_NUNBER_SELECTOR = "#container > main > div > div > div.bookPager > div.current";
            public const string CHAPTER_TITLE_SELECTOR = "#container > main > section > div > div.bookHead > div.chapterTit";
            public const string EPISODE_TITLE_SELECTOR = "#container > main > section > div > div.bookHead > p";
            public const string MAIN_CONTENT_SELECTOR = "#container > main > section > div > div.bookBody";
            public const string NEXT_PAGE_LINK_SELECTOR = "link[rel=\"next\"]";
            public const string TITLE_SELECTOR = "#container > main > section > div > div.bookHead > div.title";
            public const string KEYWORDS_SELECTOR = "head > meta[name=\"keywords\"]";
        }
        private record Page(
            string ChapterTitle,
            string EpisodeTitle,
            int PageNumber,
            int TotalPageNumber,
            string NextPageLink,
            IReadOnlyList<ParagraphNode> Paragraphs);

        private static readonly Regex PAGE_NUMBER_PATTERN = new Regex(@"<\s*(\d+)\s*/\s*(\d+)\s*>");

        public static IBookParser? CreateParser(IHtmlDocument doc, string bookUrl, IBrowserService browseService)
        {
            var mainContent = doc.QuerySelector(EpisodePageSelector.MAIN_CONTENT_SELECTOR) as IHtmlElement;
            var bookParser = default(IBookParser);
            if (mainContent == null)
            {
                bookParser = CreateParserFromTitlePage(doc, bookUrl, browseService);
            }
            else
            {
                bookParser = CreateParserFromEpisodePage(doc, bookUrl, browseService);

            }
            return bookParser;
        }

        private static IBookParser? CreateParserFromTitlePage(IHtmlDocument doc, string bookUrl, IBrowserService browserService)
        {
            var titleElement = doc.QuerySelector(TitlePageSelector.TITLE_SELECTOR);
            var authorElement = doc.QuerySelector(TitlePageSelector.AUTHOR_SELECTOR);
            var firstSectionLinkElement = doc.QuerySelector(TitlePageSelector.FIRST_SECTION_LINK_SELECTOR);
            var firstSectionLink = firstSectionLinkElement?.GetAttribute("href") ?? string.Empty;
            if (titleElement == null || authorElement == null || firstSectionLink == string.Empty)
            {
                return null;
            }

            var title = titleElement.TextContent.Trim() ?? string.Empty;
            var author = authorElement.TextContent.Trim() ?? string.Empty;
            var bookInfo = new BookInfo(
                BookUrl: bookUrl,
                Title: title,
                TitleKatakana: string.Empty,
                Author: author,
                AuthorKatakana: string.Empty);
            return new BerrysCafeParser(bookInfo, firstSectionLink, browserService);
        }

        private static IBookParser? CreateParserFromEpisodePage(IHtmlDocument doc, string bookUrl, IBrowserService browserService)
        {
            var titleElement = doc.QuerySelector(EpisodePageSelector.TITLE_SELECTOR) as IHtmlElement;
            var keywordsElement = doc.QuerySelector(EpisodePageSelector.KEYWORDS_SELECTOR) as IHtmlElement;
            var pageNumberElement = doc.QuerySelector(EpisodePageSelector.PAGE_NUNBER_SELECTOR) as IHtmlElement;

            if (titleElement == null || keywordsElement == null || pageNumberElement == null)
            {
                return null;
            }
            var title = titleElement.TextContent.Trim();
            var author = GetAuthorFromKeywords(keywordsElement);
            if (title == string.Empty || author == string.Empty)
            {
                return null;
            }
            var bookInfo = new BookInfo(
                BookUrl: bookUrl,
                Title: title,
                TitleKatakana: string.Empty,
                Author: author,
                AuthorKatakana: string.Empty);
            return new BerrysCafeParser(bookInfo, bookUrl, browserService);
        }
        private static string GetAuthorFromKeywords(IHtmlElement keywordsElement)
        {
            var keywords = keywordsElement.GetAttribute("content");
            if (keywords == null)
            {
                return string.Empty;
            }
            var keywordsArray = keywords.Trim().Split(',');
            if (keywordsArray.Length != 3)
            {
                return string.Empty;
            }
            return keywordsArray[1].Trim();
        }

        public BookInfo BookInfo { get; init; }
        private readonly string _firstEpisodeUrl;
        private readonly IBrowserService _browserService;

        public BerrysCafeParser(BookInfo bookInfo, string firstEpisodeUrl, IBrowserService browserService)
        {
            this.BookInfo = bookInfo;
            _browserService = browserService;
            _firstEpisodeUrl = firstEpisodeUrl;
        }

        public async Task<Book> DownloadBookAsync(BookInfo bookInfo, IProgress<DownloadReport> progress, CancellationToken ct)
        {
            var visitedUrls = new HashSet<string>();
            var chapterList = new List<Chapter>();
            var episodeList = new List<Episode>();
            var paragraphList = new List<ParagraphNode>();
            var firstChapterPage = default(Page);
            var firstEpisodePage = default(Page);
            var lastPage = default(Page);
            var episodeUrl = _firstEpisodeUrl;
            while (!string.IsNullOrWhiteSpace(episodeUrl))
            {
                ct.ThrowIfCancellationRequested();
                if (!visitedUrls.Add(episodeUrl))
                {
                    break;
                }
                _browserService.Navigate(episodeUrl);
                var currentUrl = _browserService.GetCurrentUrl();
                var html = _browserService.GetDom();
                var doc = await AngleSharpHelper.ParseDocumentAsync(html, currentUrl);
                var page = ParsePage(doc);
                Debug.WriteLine($"page. Title: {page.EpisodeTitle}, PageNumber: {page.PageNumber}, NextPageLink: {page.NextPageLink}");
                var isEpisodeChanged = IsEpisodeChanged(firstEpisodePage, page);
                var isChapterChanged = IsChapterChanged(firstChapterPage, page);
                if (lastPage != null && (isEpisodeChanged || isChapterChanged))
                {
                    var episode = new Episode(firstEpisodePage!.EpisodeTitle, firstEpisodePage.PageNumber, paragraphList);
                    episodeList.Add(episode);
                    paragraphList = new List<ParagraphNode>();
                    firstEpisodePage = null;
                    if (isChapterChanged)
                    {
                        var range = new EpisodeRange(firstChapterPage!.PageNumber, lastPage.PageNumber);
                        var chapter = new Chapter(firstChapterPage.ChapterTitle, range, episodeList);
                        chapterList.Add(chapter);
                        episodeList = new List<Episode>();
                        firstChapterPage = null;
                    }
                }
                if (firstChapterPage == null)
                {
                    firstChapterPage = page;
                }
                if (firstEpisodePage == null)
                {
                    firstEpisodePage = page;
                }
                lastPage = page;
                paragraphList.AddRange(page.Paragraphs);
                episodeUrl = page.NextPageLink;
            }
            if (lastPage != null 
                && firstEpisodePage != null
                && paragraphList.Count > 0)
            {
                var episode = new Episode(firstEpisodePage.EpisodeTitle, firstEpisodePage.PageNumber, paragraphList);
                episodeList.Add(episode);
            }
            if (lastPage != null
                && firstChapterPage != null)
            {
                var range = new EpisodeRange(firstChapterPage.PageNumber, lastPage.PageNumber);
                var chapter = new Chapter(firstChapterPage.ChapterTitle, range, episodeList);
                chapterList.Add(chapter);
            }
            return new Book(bookInfo, chapterList);
        }

        private bool IsEpisodeChanged(Page? firstEpisodePage, Page currentPage)
        {
            if (firstEpisodePage == null)
            {
                return false;
            }
            return currentPage.EpisodeTitle != string.Empty;
        }

        private bool IsChapterChanged(Page? firstChapterPage, Page currentPage)
        {
            if (firstChapterPage == null)
            {
                return false;
            }
            return currentPage.ChapterTitle != string.Empty;
        }


        private Page ParsePage(IHtmlDocument doc)
        {
            if (doc.Url.EndsWith("192"))
            {
                Debug.WriteLine("192");
            }
            var chapterTitle = doc.QuerySelector(EpisodePageSelector.CHAPTER_TITLE_SELECTOR)?.TextContent.Trim() ?? string.Empty;
            var episodeTitle = doc.QuerySelector(EpisodePageSelector.EPISODE_TITLE_SELECTOR)?.TextContent.Trim() ?? string.Empty;
            var (pageNumber, totalPageNumber) = GetPageNumber(doc);
            var nextPageLink = (doc.QuerySelector(EpisodePageSelector.NEXT_PAGE_LINK_SELECTOR) as IHtmlLinkElement)?.Href ?? string.Empty;
            var mainContent = doc.QuerySelector(EpisodePageSelector.MAIN_CONTENT_SELECTOR)
                ?? throw new InvalidOperationException("Missing episode body.");
            var paragraphList = new List<ParagraphNode>();
            var nodeList = new List<IBookNode>();
            var isRowBreaked = false;
            foreach (var child in mainContent.ChildNodes)
            {
                if (child is IHtmlElement elem)
                {
                    var tag = elem.TagName.ToLower();
                    if (tag == "br")
                    {
                        if (isRowBreaked)
                        {
                            paragraphList.Add(new ParagraphNode(nodeList));
                            nodeList = new List<IBookNode>();
                            isRowBreaked = false;
                        }
                        else if (nodeList.Count > 0)
                        {
                            isRowBreaked = true;
                        }
                    }
                    else if (tag == "ruby")
                    {
                        if (isRowBreaked)
                        {
                            nodeList.Add(new BreakRowNode());
                            isRowBreaked = false;
                        }
                        nodeList.Add(AngleSharpHelper.ConvertRuby(elem));
                    }
                    else
                    {
                        var textContent = elem.TextContent.Replace("\n", string.Empty);
                        if (textContent != string.Empty)
                        {
                            if (isRowBreaked)
                            {
                                nodeList.Add(new BreakRowNode());
                                isRowBreaked = false;
                            }
                            nodeList.Add(new TextNode(textContent));
                        }
                    }
                }
                else if (child is IText text)
                {
                    var textContent = text.TextContent.Replace("\n", string.Empty);
                    if (textContent != string.Empty)
                    {
                        if (isRowBreaked)
                        {
                            nodeList.Add(new BreakRowNode());
                            isRowBreaked = false;
                        }
                        nodeList.Add(new TextNode(textContent));
                    }
                }
            }
            if (nodeList.Count > 0)
            {
                paragraphList.Add(new ParagraphNode(nodeList));
            }
            return new Page(
                ChapterTitle: chapterTitle,
                EpisodeTitle: episodeTitle,
                PageNumber: pageNumber,
                TotalPageNumber: totalPageNumber,
                NextPageLink: nextPageLink,
                Paragraphs: paragraphList);
        }

        private (int, int) GetPageNumber(IHtmlDocument doc)
        {
            var pageNumberText = doc.QuerySelector(EpisodePageSelector.PAGE_NUNBER_SELECTOR)?.TextContent ?? string.Empty;
            var match = PAGE_NUMBER_PATTERN.Match(pageNumberText);
            if (!match.Success
                || !int.TryParse(match.Groups[1].Value, out int currentNumber)
                || !int.TryParse(match.Groups[2].Value, out int totalNumber))
            {
                throw new InvalidOperationException($"Missing page number in {doc.Url}");
            }
            return (currentNumber, totalNumber);
        }
    }
}
