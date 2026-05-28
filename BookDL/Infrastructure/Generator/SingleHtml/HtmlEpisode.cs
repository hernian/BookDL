using AngleSharp.Html.Dom;
using BookDL.Domain;

namespace BookDL.Infrastructure.Generator.SingleHtml
{
    public record HtmlEpisode(
        Episode Source,
        int Size,
        string Id,
        IHtmlElement SectionElement)
        : GEpisode(Source, Size);
}
