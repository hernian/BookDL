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
            var id = $"p-{source.Index + 1}";
            var fragment = _doc.CreateDocumentFragment();
            var section = fragment.AppendElement("section", attr: ("id", id));

            var title = !string.IsNullOrWhiteSpace(source.Title) ? source.Title : "(無題)";
            var h2 = section.AppendElement("h2");
            h2.AppendTateChuYokoText(title);

            foreach (var para in source.Paragraphs)
            {
                section.AppendParagraph(para);
            }

            var size = fragment.GetBytes().Length;
            return new HtmlEpisode(
                Source: source,
                Size: size,
                Id: id,
                HtmlFragment: fragment);
        }
    }
}
