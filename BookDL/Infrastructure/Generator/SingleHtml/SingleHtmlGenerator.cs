using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using BookDL.Domain;
using BookDL.Infrastructure.Html;
using System.Diagnostics;
using System.IO;

namespace BookDL.Infrastructure.Generator.SingleHtml
{
    public class SingleHtmlGenerator : IGenerator
    {
        private const int SPLIT_SIZE = 1024 * 1024; // 1MB
        private readonly Book _book;
        private readonly string _outputDirectory;
        private readonly IResourceService _resourceService;
        public SingleHtmlGenerator(Book book, string outputDirectory, IResourceService resourceService)
        {
            _book = book;
            _outputDirectory = outputDirectory;
            _resourceService = resourceService;
        }

        public async Task GenerateOutputAsync(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            Directory.CreateDirectory(_outputDirectory);

            var docTemplate = await CreateDocumentAsync();
            var gBookNodeFactory = new SingleHtmlGBookNodeFactory(docTemplate);
            var splitter = new BookSplitter(gBookNodeFactory, SPLIT_SIZE);
            var gBook = splitter.SplitBook(_book);
            foreach (var bookPart in gBook.OutputBookParts)
            {
                ct.ThrowIfCancellationRequested();
                var doc = (IHtmlDocument)docTemplate.Clone(deep: true);
                GenerateBookPart(doc, bookPart);
            }
        }

        private async Task<IHtmlDocument> CreateDocumentAsync()
        {
            using var stream = _resourceService.OpenEmbeddedItem("BookDL.Resources.SingleHtml.single.html");
            var doc = await AngleSharpHelper.ParseDocumentAsync(stream, string.Empty);
            return doc;
        }

        private void GenerateBookPart(IHtmlDocument doc, GBookPart bookPart)
        {
            var book = bookPart.Source;
            var baseName = PathHelper.SanitizeForWindowsPathSegment(book.Info.Title);
            var fileName = $"{baseName}({bookPart.EpisodeRange}).html";
            var outputPath = Path.Combine(_outputDirectory, fileName);

            var titleElem = doc.QuerySelector("title") ?? throw new InvalidOperationException("Missing title element.");
            titleElem.TextContent = $"{book.Info.Title}({bookPart.EpisodeRange})";
            titleElem
                .AddAfterSelf(doc.CreateMeta([("name", "og:title"), ("content", book.Info.Title)]))
                .AddAfterSelf(doc.CreateMeta([("name", "og:url"), ("content", book.Info.BookUrl)]))
                .AddAfterSelf(doc.CreateMeta([("name", "creator"), ("content", book.Info.Author)]))
                .AddAfterSelf(doc.CreateMeta([("name", "title-katakana"), ("content", book.Info.TitleKatakana)]))
                .AddAfterSelf(doc.CreateMeta([("name", "creator-katakana"), ("content", book.Info.AuthorKatakana)]));

            var body = doc.QuerySelector("body") as IHtmlElement ?? throw new InvalidOperationException("Missing body element.");
            GenerateCover(body, bookPart);
            GenerateBookPartToc(body, bookPart);
            foreach (var chapter in bookPart.Chapters)
            {
                foreach (var episode in chapter.Episodes.Cast<HtmlEpisode>())
                {
                    GenerateEpisode(body, episode);
                }
            }

            doc.Save(outputPath);
        }

        private void GenerateCover(IElement body, GBookPart bookPart)
        {
            var book = bookPart.Source;
            var section = body.AppendElement("section", ("id", "p-cover"));
            var h1 = section.AppendElement("h1");
            var title = !string.IsNullOrWhiteSpace(book.Info.Title) ? book.Info.Title : "(無題)";
            h1.AppendTateChuYokoText(title);
            var h2 = section.AppendElement("h2");
            h2.AppendTateChuYokoText(bookPart.EpisodeRange.ToString());
            var h3 = section.AppendElement("h3");
            h3.AppendTateChuYokoText(book.Info.Author);
        }

        private void GenerateBookPartToc(IElement body, GBookPart bookPart)
        {
            var section = body.AppendElement("section", attr: ("id", "p-toc"));

            section.AppendElementWithText("h1", "目次");
            foreach (var chapter in bookPart.Chapters)
            {
                var hasTitle = !string.IsNullOrEmpty(chapter.Source.Title);
                if (bookPart.Chapters.Count > 1 || hasTitle)
                {
                    var h2 = section.AppendElement("h2");
                    h2.AppendTateChuYokoText(hasTitle ? chapter.Source.Title : "(無題)");
                }
                foreach (var episode in chapter.Episodes.Cast<HtmlEpisode>())
                {
                    var p = section.AppendElement("p", attr: ("class", "indent"));
                    var a = p.AppendElement("a", attr: ("href", $"#{episode.Id}"));
                    a.AppendTateChuYokoText(episode.Source.Title);
                }
            }
        }

        private void GenerateEpisode(IElement body, HtmlEpisode episode)
        {
            var doc = body.GetOwnerSafe();
            // エピソードDOMを作ったときと異なるdocへ追加するため、インポートする必要がある。
            var fragment = doc.Import(episode.HtmlFragment, deep: true);
            body.AppendChild(fragment);
        }
    }
}
