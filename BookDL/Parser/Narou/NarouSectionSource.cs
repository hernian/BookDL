using AngleSharp.Dom;
using System;
using System.Collections.Generic;
using System.Text;
using HernianLib.AngleSharp;

namespace BookDL.Parser.Narou
{
    internal class NarouSectionSource : ISectionSource
    {
        private const string TITLE_SELECTOR = "body > div.l-container > main > article > h1";
        private const string PARAGRAPHS_SELECTOR = "body > div.l-container > main > article > div.p-novel__body > div > p";
        private const string NEXT_SECTION_LINK_SELECTOR = "body > div.l-container > main > div.c-pager.c-pager--center > a.c-pager__item.c-pager__item--next";

        public NarouSectionSource(int index, IDocument doc)
        {
            this.Index = index;

            this.Title = doc.QuerySelectorText(TITLE_SELECTOR);

            var listParagraph = new List<(IElement Element, string TextContents)>();
            var paragraphElements = doc.QuerySelectorAll(PARAGRAPHS_SELECTOR);
            foreach (var p in paragraphElements)
            {
                var html = p.OuterHtml;
                var text = p.TextContent?.Trim() ?? string.Empty;
                listParagraph.Add((Element: p, TextContents: text));
            }

            for (var i = paragraphElements.Count - 1; i >= 0; i--)
            {
                var (html, text) = listParagraph[i];
                if (text.Length == 0)
                {
                    listParagraph.RemoveAt(i);
                }
            }
            this.Paragraphs = listParagraph.Select(x => x.Element).ToList().AsReadOnly();

            var nextLinkElement = doc.QuerySelector(NEXT_SECTION_LINK_SELECTOR);
            this.NextSectionLink = nextLinkElement?.GetAttribute("href") ?? string.Empty;
        }

        public int Index { get; init; }

        public string Title { get; init; }

        public IReadOnlyCollection<IElement> Paragraphs { get; init; }

        public string NextSectionLink { get; init; }
    }
}
