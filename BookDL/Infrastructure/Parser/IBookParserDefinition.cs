using AngleSharp.Html.Dom;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookDL.Infrastructure.Parser
{
    public interface IBookParserDefinition
    {
        string Name { get; }
        Task<IBookParser?> CreateParserAsync(IHtmlDocument doc, string bookUrl, CancellationToken ct);
    }
}
