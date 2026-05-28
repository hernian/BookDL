using AngleSharp.Html.Dom;

namespace BookDL.Infrastructure.Parser
{
    public interface IBookParserFactoryAdapter
    {
        string Name { get; }
        Task<IBookParser?> CreateParserAsync(IHtmlDocument doc, string bookUrl, CancellationToken ct);
    }
}
