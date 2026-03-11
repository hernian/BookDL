using AngleSharp.Dom;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookDL
{
    public interface ISectionSource
    {
        int Index { get; }
        string Title { get; }
        IReadOnlyCollection<IElement> Paragraphs { get; }
    }
}
