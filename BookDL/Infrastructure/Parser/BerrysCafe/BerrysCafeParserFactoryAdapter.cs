using AngleSharp.Html.Dom;

namespace BookDL.Infrastructure.Parser.BerrysCafe
{
    public class BerrysCafeParserFactoryAdapter : IBookParserFactoryAdapter
    {
        public string Name { get; } = "Bessy's Cafe";
        private IBrowserService _browserService;

        public BerrysCafeParserFactoryAdapter(IBrowserService browserService)
        {
            _browserService = browserService;
        }

        public Task<IBookParser?> CreateParserAsync(IHtmlDocument doc, string bookUrl, CancellationToken ct)
        {
            return Task.FromResult(BerrysCafeParser.CreateParser(doc, bookUrl, _browserService));
        }
    }
}
