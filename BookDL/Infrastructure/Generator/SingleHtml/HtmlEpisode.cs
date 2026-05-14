using AngleSharp.Dom;
using BookDL.Domain;

namespace BookDL.Infrastructure.Generator.SingleHtml
{
    public record HtmlEpisode(
        Episode Source,
        int Size,
        string Id,
        IDocumentFragment HtmlFragment)
        : GEpisode(Source, Size);
}
