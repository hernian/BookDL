using AngleSharp.Html.Dom;
using AngleSharp;
using System;
using System.Collections.Generic;
using System.Text;
using BookDL.Domain;
using AngleSharp.Dom;

namespace BookDL.Infrastructure.Html
{
    public static class AngleSharpHelper
    {
        public static IHtmlDocument ParseDocument(string html, string url)
        {
            var config = Configuration.Default;
            var context = new BrowsingContext(config);
            var doc = context.OpenAsync(resp =>
            {
                resp.Address(url);
                resp.Content(html);
            }).GetAwaiter().GetResult() as IHtmlDocument;
            if (doc == null)
            {
                throw new InvalidOperationException("Html parse error");
            }
            return doc;
        }

        public static ParagraphNode ConvertParagraph(IHtmlElement srcElement)
        {
            var nodeList = new List<IBookNode>();
            var sb = new StringBuilder();
            foreach (var node in srcElement.ChildNodes)
            {
                if (node is IText text)
                {
                    sb.Append(text.TextContent);
                    continue;
                }
                if (node is IHtmlBreakRowElement)
                {
                    if (sb.Length > 0)
                    {
                        nodeList.Add(new TextNode(sb.ToString()));
                        sb.Clear();
                    }
                    nodeList.Add(new BreakRowNode());
                    continue;
                }
                if (node is IHtmlElement childElement)
                {
                    var tag = childElement.TagName.ToLower();
                    if (tag == "ruby")
                    {
                        if (sb.Length > 0)
                        {
                            nodeList.Add(new TextNode(sb.ToString()));
                            sb.Clear();
                        }
                        nodeList.Add(ConvertRuby(childElement));
                        continue;
                    }
                    sb.Append(childElement.TextContent);
                    continue;
                }
            }
            var lastText = sb.ToString();
            if (!string.IsNullOrWhiteSpace(lastText))
            {
                nodeList.Add(new TextNode(lastText));
            }
            return new ParagraphNode(nodeList);
        }

        private static RubyNode ConvertRuby(IHtmlElement srcElement)
        {
            var rubyItemList = new List<RubyItem>();
            var textContent = default(string);
            foreach (var node in srcElement.ChildNodes)
            {
                if (node is IText text)
                {
                    textContent = text.TextContent;
                    continue;
                }
                if (node is IHtmlElement childElement)
                {
                    var tag = childElement.TagName.ToLower();
                    if (tag == "rt")
                    {
                        if (textContent != default)
                        {
                            rubyItemList.Add(new RubyItem(textContent, childElement.TextContent));
                            textContent = default;
                        }
                    }
                    continue;
                }
            }

            return new RubyNode(rubyItemList);
        }
    }
}
