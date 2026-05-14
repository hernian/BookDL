using AngleSharp.Html.Dom;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookDL.Infrastructure.Parser
{
    public delegate Task<IBookParser?> CreateBookParserAsyncDelegate(
        IBrowserService browserService,
        IHtmlDocument doc,
        string bookUrl,
        CancellationToken ct);

}
