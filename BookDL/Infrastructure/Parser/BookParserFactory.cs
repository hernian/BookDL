using AngleSharp.Html.Dom;
using BookDL.Infrastructure.Html;

namespace BookDL.Infrastructure.Parser
{
    public interface IBookParserFactory
    {
        Task<IBookParser?> CreateBookParserAsync(string bookUrl, CancellationToken ct);
    }

    public class BookParserFactory : IBookParserFactory
    {
        private static readonly TagLog<BookParserFactory> Log = new();

        private readonly IBrowserService _browserService;
        private readonly List<IBookParserFactoryAdapter> _descriptorList = new();

        public BookParserFactory(IBrowserService browserService)
        {
            _browserService = browserService;
        }

        public void AddParserFactoryAdapter(IBookParserFactoryAdapter parserDefinition)
        {
            _descriptorList.Add(parserDefinition);
        }

        public void AddAllFactoryAdapters(IEnumerable<IBookParserFactoryAdapter> parserDefinitions)
        {
            _descriptorList.AddRange(parserDefinitions);
        }

        public async Task<IBookParser?> CreateBookParserAsync(string bookUrl, CancellationToken ct)
        {
            _browserService.Navigate(bookUrl);
            var html = _browserService.GetDom();
            var currentUrl = _browserService.GetCurrentUrl();
            var doc = await AngleSharpHelper.ParseDocumentAsync(html, currentUrl);
            foreach (var parserDesc in _descriptorList)
            {
                ct.ThrowIfCancellationRequested();
                try
                {
                    var bookParser = await parserDesc.CreateParserAsync(doc, bookUrl, ct);
                    if (bookParser != null)
                    {
                        return bookParser;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"Exception occered in CreateBookParser. Name: {parserDesc.Name}");
                }
            }
            return null;
        }
    }
}
