using AngleSharp.Dom;
using AngleSharp.Html;
using AngleSharp.Html.Dom;
using BookDL.Domain;
using System.IO;
using System.Text;

namespace BookDL.Infrastructure.Html
{
    public static class AngleSharpExtensions
    {
        private static readonly Encoding UTF8_WO_BOM = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
        private static readonly PrettyMarkupFormatter FORMATTER = new()
        {
            Indentation = "  ",
            NewLine = "\n",
        };

        public static IHtmlDocument GetOwnerSafe(this INode node)
        {
            var owner = node.Owner;
            if (owner == null || owner is not IHtmlDocument doc)
            {
                throw new InvalidOperationException("Node has no owner html document.");
            }
            return doc;
        }


        public static byte[] GetBytes(this IDocumentFragment frag)
        {
            using var ms = new MemoryStream();
            using (var writer = new StreamWriter(ms, UTF8_WO_BOM))
            {
                frag.ToHtml(writer, FORMATTER);
            }
            return ms.ToArray();
        }

        public static void Save(this IDocument doc, string path)
        {
            using var writer = new StreamWriter(path, false, UTF8_WO_BOM);
            doc.ToHtml(writer, FORMATTER);
        }

        public static T AddAfterSelf<T>(this IElement element, T newNode) where T : INode
        {
            var parent = element.Parent ?? throw new InvalidOperationException("Missing parent.");
            var next = element.NextSibling;
            parent.InsertBefore(newNode, next);
            return newNode;
        }

        public static IElement CreateMeta(this IDocument doc, (string Name, string Content)[] attrs)
        {
            var metaElem = doc.CreateElement("meta");
            foreach (var item in attrs)
            {
                metaElem.SetAttribute(item.Name, item.Content);
            }
            return metaElem;
        }

        // 以下の拡張メソッド群は、必ず親を指定してDOMを操作する。
        // 複数のDocumentが混じる環境にて、必ず親を指定することで、親のオーナーから要素を生成することを保証する。

        public static void AppendText(this INode parent, string text)
        {
            var doc = parent.GetOwnerSafe();
            var textNode = doc.CreateTextNode(text);
            parent.AppendChild(textNode);
        }

        public static IElement AppendElement(
            this INode parent,
            string tag,
            (string Name, string Value)? attr = null)
        {
            var doc = parent.GetOwnerSafe();
            var newElement = doc.CreateElement(tag);
            if (attr is { } a)
            {
                newElement.SetAttribute(a.Name, a.Value);
            }
            parent.AppendChild(newElement);
            return newElement;
        }

        public static IElement AppendElementWithText(
            this INode parent,
            string tag,
            string text,
            (string Name, string Value)? attr = null)
        {
            var newElement = parent.AppendElement(tag);
            newElement.TextContent = text;
            if (attr is { } a)
            {
                newElement.SetAttribute(a.Name, a.Value);
            }
            return newElement;
        }
        public static IElement AppendParagraph(this IElement parent, ParagraphNode para)
        {
            var p = parent.AppendElement("p");
            foreach (var node in para.Nodes)
            {
                p.AppendBookNode(node);
            }
            return parent;
        }

        public static IElement AppendTateChuYokoText(this IElement parent, string text)
        {
            var doc = parent.GetOwnerSafe();
            foreach (var node in TateChuYokoHelper.ConvertForText(text))
            {
                parent.AppendBookNode(node);
            }
            return parent;
        }

        public static IElement AppendBookNode(this IElement parent, IBookNode node)
        {
            if (node is TextNode text)
            {
                parent.AppendText(text.TextContent);
            }
            else if (node is BreakRowNode)
            {
                parent.AppendElement("br");
            }
            else if (node is TateChuYokoNode tateChuYoko)
            {
                parent.AppendElementWithText("span", tateChuYoko.TextContent, attr: ("class", "tcy"));
            }
            else if (node is RubyNode ruby)
            {
                var rubyElem = parent.AppendElement("ruby");
                foreach (var item in ruby.Items)
                {
                    rubyElem.AppendText(item.TextContent);
                    rubyElem.AppendElementWithText("rp", "(");
                    rubyElem.AppendElementWithText("rt", item.RubyText);
                    rubyElem.AppendElementWithText("rp", ")");
                }
            }
            else
            {
                throw new InvalidDataException($"SingleHtmlGenerator.ConvertNodeToHtml. Unknown IBookNode type. node: {node}");
            }
            return parent;
        }

    }
}
