using AngleSharp.Html.Parser;
using AngleSharp.Dom;
using HernianLib.AngleSharp;
using HernianLib.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace BookDL.Parser.BerrysCafe
{
    /// <summary>
    /// Berry's Cafe サイトから書籍データを取得します。
    /// </summary>
    internal class BerrysCafeBookSource : IBookSource
    {
        // タイトルページ内のセレクター
        private const string TITLE_SELECTOR = "#container > main > section.section.bookDetails > div.title-wrap > div.title > h2";
        private const string AUTHOR_SELECTOR = "#container > main > section.section.bookDetails > div.group-wrap > div.group-01 > div > div.subDetails-02 > div > a";
        private const string FIRST_SECTION_LINK_SELECTOR = "link[rel~=\"next\"]";
        // 各本文ページ内のセレクター
        private const string CHAPTER_TITLE_SELECTOR = "#container > main > section > div > div.bookHead > div.chapterTit";
        private const string CONTENT_SELECTOR = "#container > main > section > div > div.bookBody";
        private const string NEXT_PAGE_LINK_SELECTOR = "link[rel~=\"next\"]";

        static BerrysCafeBookSource()
        {
            var factory = BookSourceFactory.TheInstance;
            factory.RegisterBookSource(".berrys-cafe.jp", (webViewControl, bookUrl) => new BerrysCafeBookSource(webViewControl, bookUrl));
        }

        /// <summary>
        /// 書籍のURL。
        /// </summary>
        public string BookUrl { get; init; }
        /// <summary>
        /// 書籍のタイトル。
        /// </summary>
        public string Title { get; private set; } = string.Empty;
        /// <summary>
        /// 著者名。
        /// </summary>
        public string Author { get; private set; } = string.Empty;

        /// <summary>
        /// 最初のセクションへのリンク。
        /// </summary>
        public string FirstSectionLink { get; private set; } = string.Empty;

        private readonly WebView2Control _webViewControl;
        private readonly HtmlParser _parser = new ();

        public BerrysCafeBookSource(WebView2Control webViewControl, string bookUrl)
        {
            _webViewControl = webViewControl;
            this.BookUrl = bookUrl;
        }

        public async Task GetInfoAsync(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var html = await _webViewControl.NavigateAsync(this.BookUrl);
            ct.ThrowIfCancellationRequested();
            var doc = _parser.ParseDocument(html);
            this.Title = doc.QuerySelectorText(TITLE_SELECTOR);
            this.Author = doc.QuerySelectorText(AUTHOR_SELECTOR);
            var firstSectionLinkElement = doc.QuerySelector(FIRST_SECTION_LINK_SELECTOR);
            this.FirstSectionLink = firstSectionLinkElement?.GetAttribute("href") ?? string.Empty;
        }

        public async IAsyncEnumerable<ISectionSource> AllSectionsAsync([EnumeratorCancellation] CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            int index = 1;
            var chapterTitle = string.Empty;  // チャプタータイトルがない場合は空文字列
            var contents = new List<IElement>();
            var pageLink = this.FirstSectionLink;
            var visitedUris = new HashSet<Uri>();
            var baseUri = new Uri(this.BookUrl);
            while (pageLink != string.Empty)
            {
                ct.ThrowIfCancellationRequested();
                var uri = new Uri(baseUri, pageLink);
                // すでに訪れたURIの場合はループを抜ける（安全策）
                if (!visitedUris.Add(uri))
                {
                    break;
                }
                var html = await _webViewControl.NavigateAsync(uri.ToString());
                ct.ThrowIfCancellationRequested();
                var doc = _parser.ParseDocument(html);
                var chapterTitleElement = doc.QuerySelector(CHAPTER_TITLE_SELECTOR);
                var contentElement = doc.QuerySelector(CONTENT_SELECTOR) ?? throw new InvalidDataException($"Content element not found at URL: {uri}");
                var nextPageLinkElement = doc.QuerySelector(NEXT_PAGE_LINK_SELECTOR);
                var nextPageLink = nextPageLinkElement?.GetAttribute("href");

                // chapterTitleElementが存在する = 新しいチャプターの開始
                if (chapterTitleElement != null)
                {
                    // 前のチャプターがあれば出力（タイトルが空でも可）
                    if (contents.Count > 0)
                    {
                        var sectionSource = new BerrysCafeSectionSource(index, chapterTitle, contents.AsReadOnly());
                        yield return sectionSource;
                        index++;
                    }
                    // 新しいチャプタータイトルに更新
                    chapterTitle = chapterTitleElement.TextContent.Trim();
                    contents = new List<IElement>();
                }

                contents.Add(contentElement);
                pageLink = nextPageLink ?? string.Empty;
            }

            // 最後のセクションを出力（タイトルが空でも可）
            if (contents.Count > 0)
            {
                var sectionSource = new BerrysCafeSectionSource(index, chapterTitle, contents: contents.AsReadOnly());
                yield return sectionSource;
            }
        }
    }
}
