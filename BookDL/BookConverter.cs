using AngleSharp.Dom;
using AngleSharp.Text;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BookDL
{
    public static class BookConverter
    {
        public class BookConvertError : Exception
        {
            public BookConvertError(string message) : base(message)
            {
            }
        }

        private static HashSet<Char> CreateInvalidFileNameChars()
        {
            var hash = new HashSet<Char>(Path.GetInvalidFileNameChars());
            hash.Add('.');
            return hash;
        }
        private static readonly HashSet<Char> INVALID_FILENAME_CHARS = CreateInvalidFileNameChars();
        private const int MAX_FILENAME_LENGTH = 64;

        public static string GetFileNameFromTitle(string title)
        {
            var sb = new StringBuilder();
            foreach (var c in title)
            {
                if (sb.Length >= MAX_FILENAME_LENGTH)
                {
                    sb.Append("…");
                    break;
                }
                if (INVALID_FILENAME_CHARS.Contains(c))
                {
                    sb.Append('_');
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        private static readonly FrozenSet<string> ALLOW_TAGS = new[] { "ruby", "rp", "rt" }.ToFrozenSet();

        public static IElement NormalizeParagraph(IElement sourceElement)
        {
            IElement pElement = CreateTargetElement(sourceElement);
            foreach (var child in sourceElement.ChildNodes)
            {
                var filteredNode = FilterNode(child);
                pElement.AppendChild(filteredNode);
            }

            return pElement;
        }

        // p タグならクローン、それ以外なら新規作成
        private static IElement CreateTargetElement(IElement sourceElement)
        {
            var tagName = sourceElement.TagName.ToLowerInvariant();
            if (tagName != "p")
            {
                var created = sourceElement.Owner?.CreateElement("p");
                if (created == null)
                {
                    throw new BookConvertError("sourceElement.Owner?.CreateElement() returns null");
                }
                return created;
            }
            var cloned = sourceElement.Clone(false) as IElement;
            if (cloned == null)
            {
                throw new BookConvertError("sourceElement.Clone() returns null");
            }
            return cloned;
        }

        private static INode FilterNode(INode node)
        {
            if (node.NodeType != NodeType.Element)
            {
                return node.Clone();
            }

            var element = (IElement)node;
            var tagName = element.TagName.ToLowerInvariant();

            if (ALLOW_TAGS.Contains(tagName))
            {
                var clonedElement = element.Clone(false) as IElement;
                if (clonedElement == null)
                {
                    throw new BookConvertError("element.Clone() returns null");
                }

                foreach (var child in element.ChildNodes)
                {
                    var filteredChild = FilterNode(child);
                    clonedElement.AppendChild(filteredChild);
                }
                return clonedElement;
            }
            else
            {
                var fragment = element.Owner?.CreateDocumentFragment();
                if (fragment == null)
                {
                    throw new BookConvertError("element.Owner?.CreateDocumentFragment() returns null");
                }

                foreach (var child in element.ChildNodes)
                {
                    var filteredChild = FilterNode(child);
                    fragment.AppendChild(filteredChild);
                }
                return fragment;
            }
        }

    }
}
