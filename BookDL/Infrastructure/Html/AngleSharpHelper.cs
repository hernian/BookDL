using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using BookDL.Domain;
using System.IO;
using System.Text;

namespace BookDL.Infrastructure.Html
{
    public static class AngleSharpHelper
    {
        public static async Task<IHtmlDocument> ParseDocumentAsync(string html, string url)
        {
            var config = Configuration.Default;
            var context = new BrowsingContext(config);
            var unkdownDoc = await context.OpenAsync(resp =>
            {
                resp.Address(url);
                resp.Content(html);
            });
            if (unkdownDoc == null || unkdownDoc is not IHtmlDocument htmlDoc)
            {
                throw new InvalidOperationException("Html parse error");
            }
            return htmlDoc;
        }

        public static async Task<IHtmlDocument> ParseDocumentAsync(Stream stream, string url)
        {
            var config = Configuration.Default;
            var context = new BrowsingContext(config);
            var unkdownDoc = await context.OpenAsync(resp =>
            {
                resp.Address(url);
                resp.Content(stream);
            });
            if (unkdownDoc == null || unkdownDoc is not IHtmlDocument htmlDoc)
            {
                throw new InvalidOperationException("Html parse error");
            }
            return htmlDoc;
        }

        public static bool IsSeparator(IHtmlElement srcElement)
        {
            return string.IsNullOrWhiteSpace(srcElement.TextContent)
                && srcElement.Children.Count == 1
                && srcElement.Children[0].TagName.ToLower() == "br";
        }

        public static IEnumerable<IBookNode> ConvertParagraph(IHtmlElement srcElement)
        {
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
                        yield return new TextNode(sb.ToString());
                        sb.Clear();
                    }
                    yield return new BreakRowNode();
                    continue;
                }
                if (node is IHtmlElement childElement)
                {
                    var tag = childElement.TagName.ToLower();
                    if (tag == "ruby")
                    {
                        if (sb.Length > 0)
                        {
                            yield return new TextNode(sb.ToString());
                            sb.Clear();
                        }
                        yield return ConvertRuby(childElement);
                        continue;
                    }
                    sb.Append(childElement.TextContent);
                    continue;
                }
            }
            var lastText = sb.ToString();
            if (!string.IsNullOrWhiteSpace(lastText))
            {
                yield return new TextNode(lastText);
            }
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
