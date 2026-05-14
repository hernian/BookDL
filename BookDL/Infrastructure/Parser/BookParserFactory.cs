using System;
using System.Collections.Generic;
using System.Text;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using BookDL.Infrastructure.Html;

namespace BookDL.Infrastructure.Parser
{
    public interface IBookParserFactory
    {
        Task<IBookParser?> CreateBookParserAsync(IBrowserService browserService, string bookUrl, CancellationToken ct);
    }

    public class BookParserFactory : IBookParserFactory
    {
        private static readonly TagLog<BookParserFactory> Log = new();

        private readonly List<(string Name, CreateBookParserAsyncDelegate Creator)> _creatorList = new();
        public void AddParser(string name, CreateBookParserAsyncDelegate creator)
        {
            _creatorList.Add((name, creator));
        }

        public async Task<IBookParser?> CreateBookParserAsync(IBrowserService browserService, string bookUrl, CancellationToken ct)
        {
            browserService.Navigate(bookUrl);
            var html = browserService.GetDom();
            var currentUrl = browserService.GetCurrentUrl();
            var doc = await AngleSharpHelper.ParseDocumentAsync(html, currentUrl);
            foreach (var (name, createAsync) in _creatorList)
            {
                ct.ThrowIfCancellationRequested();
                try
                {
                    var bookParser = await createAsync(browserService, doc, bookUrl, ct);
                    if (bookParser != null)
                    {
                        return bookParser;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"Exception occered in CreateBookParser. Name: {name}");
                }
            }
            return null;
        }
    }
}
