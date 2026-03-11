using AngleSharp.Dom;
using System;
using System.Collections.Generic;
using System.Text;

namespace HernianLib.AngleSharp
{
    public static class AngleSharpHelper
    {
        public static string QuerySelectorText(this IDocument? doc, string selector)
        {
            var selectedElement = doc?.QuerySelector(selector);
            return selectedElement?.TextContent ?? string.Empty;
        }

        public static string QuerySelectorText(this IElement? element, string selector)
        {
            var selectedElement = element?.QuerySelector(selector);
            return selectedElement?.TextContent ?? string.Empty;
        }
    }
}
