using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using BookDL.Domain;
using BookDL.Infrastructure.Html;
using System.Text;

namespace BookDL.Infrastructure.Generator.SingleHtml
{
    public class SingleHtmlGBookNodeFactory : GBookNodeFactory
    {
        private readonly IHtmlDocument _doc;
        public SingleHtmlGBookNodeFactory(IHtmlDocument doc)
        {
            _doc = doc;
        }

        public override GEpisode CreateGEpisode(Episode source)
        {
            var id = $"p-{source.Index}";
            var section = (IHtmlElement)_doc.CreateElement("section");
            section.SetAttribute("id", id);

            if (!string.IsNullOrWhiteSpace(source.Title))
            {
                var h2 = section.AppendElement("h2");
                h2.AppendTateChuYokoText(source.Title);
            }

            foreach (var para in source.Paragraphs)
            {
                section.AppendParagraph(para);
            }

            var size = section.GetBytes().Length;
            return new HtmlEpisode(
                Source: source,
                Size: size,
                Id: id,
                SectionElement: section);
        }
    }
}
