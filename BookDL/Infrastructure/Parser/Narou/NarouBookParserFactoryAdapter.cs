using AngleSharp.Html.Dom;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookDL.Infrastructure.Parser.Narou
{
    public class NarouBookParserFactoryAdapter : IBookParserFactoryAdapter
    {
        public string Name { get; } = "なろう";

        private readonly IBrowserService _browserService;
        public NarouBookParserFactoryAdapter(IBrowserService browserService)
        {
            _browserService = browserService;
        }

        public Task<IBookParser?> CreateParserAsync(IHtmlDocument doc, string bookUrl, CancellationToken ct)
        {
            return NarouBookParser.CreateAsync(_browserService, doc, bookUrl, ct);
        }
    }
}
