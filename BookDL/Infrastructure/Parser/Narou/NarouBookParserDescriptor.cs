using AngleSharp.Html.Dom;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookDL.Infrastructure.Parser.Narou
{
    public class NarouBookParserDescriptor : IBookParserDefinition
    {
        public string Name { get; } = "なろう";

        private readonly IBrowserService _browserService;
        public NarouBookParserDescriptor(IBrowserService browserService)
        {
            _browserService = browserService;
        }

        public Task<IBookParser?> CreateParserAsync(IHtmlDocument doc, string bookUrl, CancellationToken ct)
        {
            return NarouBookParser.CreateAsync(_browserService, doc, bookUrl, ct);
        }
    }
}
