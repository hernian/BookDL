using AngleSharp;
using AngleSharp.Dom;
using HernianLib.AngleSharp;
using HernianLib.Controls;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using AngleSharp.Html.Parser;

namespace BookDL.Parser.Narou
{
    public class NarouBookSource : IBookSource
    {
        private const string TITLE_SELECTOR = "body > div.l-container > main > article > h1";
        private const string AUTHOR_SELECTOR = "body > div.l-container > main > article > div.p-novel__author > a";
        private const string FIRST_SECTION_LINK_SELECTOR = "a.p-eplist__subtitle";

        public string BookUrl { get; init; }
        public string Title { get; private set; } = string.Empty;
        public string Author { get; private set; } = string.Empty;

        public string FirstSectionLink { get; private set; } = string.Empty;

        private readonly WebView2Control _webViewControl;
        private readonly HtmlParser _parser = new HtmlParser();

        public NarouBookSource(WebView2Control webViewControl, string bookUrl)
        {
            _webViewControl = webViewControl;
            this.BookUrl = bookUrl;
        }

        public async Task GetInfoAsync(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var html = await _webViewControl.NavigateAsync(this.BookUrl);
            ct.ThrowIfCancellationRequested();
            var (title, author, firstSectionLink) = await Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();
                var doc = _parser.ParseDocument(html);
                var title = doc.QuerySelectorText(TITLE_SELECTOR);
                var author = doc.QuerySelectorText(AUTHOR_SELECTOR);
                var firstPageLinkElement = doc.QuerySelector(FIRST_SECTION_LINK_SELECTOR);
                var firstSectionLink = firstPageLinkElement?.GetAttribute("href") ?? string.Empty;
                return (title, author, firstSectionLink);
            }, ct);
            this.Title = title;
            this.Author = author;
            this.FirstSectionLink = firstSectionLink;
        }

        public async IAsyncEnumerable<ISectionSource> AllSectionsAsync([EnumeratorCancellation] CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var baseUri = _webViewControl.GetSource();
            if (baseUri == null)
            {
                throw new InvalidOperationException("WebView source is not available");
            }

            string sectionLink = this.FirstSectionLink;
            int index = 1;
            var visitedLinks = new HashSet<string>();
            while (sectionLink != string.Empty)
            {
                if (!visitedLinks.Add(sectionLink))
                {
                    throw new InvalidOperationException($"Circular reference detected: {sectionLink}");
                }

                ct.ThrowIfCancellationRequested();
                var url = new Uri(baseUri, sectionLink).ToString();
                var html = await _webViewControl.NavigateAsync(url);
                ct.ThrowIfCancellationRequested();
                var sectionSource = await Task.Run(() =>
                {
                    var doc = _parser.ParseDocument(html);
                    return new NarouSectionSource(index, doc);
                }, ct);
                yield return sectionSource;
                index++;
                sectionLink = sectionSource.NextSectionLink;
            }
        }
    }
}
