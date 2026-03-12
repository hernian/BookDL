using AngleSharp.Dom;
using HernianLib.AngleSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace BookDL.Parser.BerrysCafe
{
    public class BerrysCafeSectionSource : ISectionSource
    {
        // 定数
        private const string TITLE_SELECTOR = "#container > main > section > div > div.bookHead > div.chapterTit";
        private const string CONTENT_SELECTOR = "#container > main > section > div > div.bookBody";
        // プロパティ
        public int Index { get; init; }
        public string Title { get; init; }
        public IReadOnlyCollection<IElement> Paragraphs { get; init; }

        private static IEnumerable<IElement> GetParagraphs(IElement contentElement)
        {
            var doc = contentElement.Owner;
            if (doc == null)
            {
                yield break;
            }

            var currentParagraph = doc.CreateElement("p");
            var blankParagraphs = new List<IElement>();

            foreach (var child in contentElement.ChildNodes)
            {
                if (child is IElement element && element.TagName.ToLowerInvariant() == "br")
                {
                    // 現在の段落が空白のみの場合、一時的に保持
                    if (currentParagraph.TextContent.Trim() == string.Empty)
                    {
                        blankParagraphs.Add(currentParagraph);
                        currentParagraph = doc.CreateElement("p");
                        continue;
                    }

                    // 非空白段落の前にあった空白段落を出力
                    foreach (var blankParagraph in blankParagraphs)
                    {
                        yield return blankParagraph;
                    }
                    blankParagraphs.Clear();

                    // 非空白段落を出力
                    yield return currentParagraph;
                    currentParagraph = doc.CreateElement("p");
                }
                else if (child is IText textNode)
                {
                    // テキストノードの場合、改行文字を除去
                    var textContent = textNode.TextContent.Replace("\n", "");
                    if (textContent != string.Empty)
                    {
                        var newTextNode = doc.CreateTextNode(textContent);
                        currentParagraph.AppendChild(newTextNode);
                    }
                }
                else
                {
                    var clonedNode = child.Clone(deep: true);
                    currentParagraph.AppendChild(clonedNode);
                }
            }

            // 最後の段落が非空白の場合のみ出力（末尾の空白段落は出力しない）
            if (currentParagraph.TextContent.Trim() != string.Empty)
            {
                // 最後の非空白段落の前にあった空白段落を出力
                foreach (var blankParagraph in blankParagraphs)
                {
                    yield return blankParagraph;
                }
                yield return currentParagraph;
            }
            // blankParagraphs に残っている段落は末尾の空白段落なので出力しない
        }
        public BerrysCafeSectionSource(int index, string title, ReadOnlyCollection<IElement> contents)
        {
            this.Index = index;
            this.Title = title;

            var paragraphsInSection = new List<IElement>();
            foreach (var content in contents)
            {
                var paragraphsInPage = GetParagraphs(content);
                paragraphsInSection.AddRange(paragraphsInPage);
            }
            this.Paragraphs = paragraphsInSection.AsReadOnly();
        }
    }
}
