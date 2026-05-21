using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using BookDL.Domain;
using BookDL.Infrastructure.Html;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Media.Animation;

namespace BookDL.Infrastructure.Generator.SingleHtml
{
    public class SingleHtmlGenerator : IGenerator
    {
        private const int SPLIT_SIZE = 2 * 1024 * 1024; // 2MB
        private static readonly TagLog<SingleHtmlGenerator> LOG = new();

        private static bool IsChapterOnly(Book book)
        {
            foreach (var chapter in book.Chapters)
            {
                if (chapter.Episodes.Count != 1)
                {
                    return false;
                }
                if (!string.IsNullOrWhiteSpace(chapter.Episodes[0].Title))
                {
                    return false;
                }
            }
            return true;
        }

        private readonly Book _book;
        private readonly string _outputDirectory;
        private readonly IResourceService _resourceService;
        private readonly ITextWriterFactory _storageService;
        private readonly bool _chapterOnly;
        public SingleHtmlGenerator(
            Book book,
            string outputDirectory,
            IResourceService resourceService,
            ITextWriterFactory storageService)
        {
            LOG.Debug($"ctor. book.Title: {book.Info.Title}, outputDirectory: {outputDirectory}");
            _book = book;
            _outputDirectory = outputDirectory;
            _resourceService = resourceService;
            _storageService = storageService;
            _chapterOnly = IsChapterOnly(book);
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
                // bookPart毎に異なるDOMツリーを構築するためコピーを作る
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
            LOG.Debug($"GenerateBookPart. EpisodeRange: {bookPart.EpisodeRange}, Size: {bookPart.Size}");
            var book = bookPart.Source;
            var baseName = PathHelper.SanitizeForWindowsPathSegment(book.Info.Title);
            var fileName = $"{baseName}({bookPart.EpisodeRange}).html";
            var outputPath = Path.Combine(_outputDirectory, fileName);

            var titleElem = doc.QuerySelector("title") ?? throw new InvalidOperationException("Missing title element.");
            titleElem.TextContent = $"{book.Info.Title}({bookPart.EpisodeRange})";
            // Kindle Paperwhiteで著者名とは認識されないけど、
            // 著者名と、カタカナのタイトル・著者名もheadに追加しておく。
            titleElem
                .AddAfterSelf(doc.CreateMeta([("property", "og:title"), ("content", book.Info.Title)]))
                .AddAfterSelf(doc.CreateMeta([("property", "og:url"), ("content", book.Info.BookUrl)]))
                .AddAfterSelf(doc.CreateMeta([("name", "creator"), ("content", book.Info.Author)]))
                .AddAfterSelf(doc.CreateMeta([("name", "title-katakana"), ("content", book.Info.TitleKatakana)]))
                .AddAfterSelf(doc.CreateMeta([("name", "creator-katakana"), ("content", book.Info.AuthorKatakana)]));

            var body = doc.QuerySelector("body") as IHtmlElement ?? throw new InvalidOperationException("Missing body element.");
            GenerateCover(body, bookPart);
            GenerateBookPartToc(body, bookPart);
            foreach (var chapter in bookPart.Chapters)
            {
                var chapterTitle = chapter.Source.Title;
                foreach (var episode in chapter.Episodes.Cast<HtmlEpisode>())
                {
                    GenerateEpisode(body, episode, chapterTitle);
                    chapterTitle = string.Empty;
                }
            }

            using var writer = _storageService.OpenTextStream(outputPath);
            doc.Save(writer);
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
            if (_chapterOnly)
            {
                // Berry's Cafeではチャプタータイトルがエピソードタイトルとして扱われることがある
                // その場合、各チャプターは1つのエピソードのみを持ち、エピソードにタイトルはなく、チャプターにはタイトルがある。
                // ただし、全体が1チャプター1エピソードで、チャプタータイトル・エピソードタイトルの両方を持たない、もとのサイトで目次がない場合もある。
                foreach (var chapter in bookPart.Chapters)
                {
                    var episode = (HtmlEpisode)chapter.Episodes[0];
                    var p = section.AppendElement("p", attr: ("class", "indent"));
                    var a = p.AppendElement("a", attr: ("href", $"#{episode.Id}"));
                    var title = !string.IsNullOrWhiteSpace(chapter.Source.Title) ? chapter.Source.Title : "(無題)";
                    a.AppendTateChuYokoText(title);
                }
            }
            else
            {
                foreach (var chapter in bookPart.Chapters)
                {
                    var hasTitle = !string.IsNullOrEmpty(chapter.Source.Title);
                    if (bookPart.Chapters.Count > 1 || hasTitle)
                    {
                        var h2 = section.AppendElement("h2");
                        h2.AppendTateChuYokoText(chapter.Source.Title);
                    }
                    foreach (var episode in chapter.Episodes.Cast<HtmlEpisode>())
                    {
                        var p = section.AppendElement("p", attr: ("class", "indent"));
                        var a = p.AppendElement("a", attr: ("href", $"#{episode.Id}"));
                        var title = !string.IsNullOrWhiteSpace(episode.Source.Title) ? episode.Source.Title : "(無題)";
                        a.AppendTateChuYokoText(title);
                    }
                }
            }
        }

        private void GenerateEpisode(IElement body, HtmlEpisode episode, string chapterTitle)
        {
            var doc = body.GetOwnerSafe();
            // エピソードDOMを作ったときと異なるdocへ追加するため、インポートする必要がある。
            var section = doc.Import(episode.SectionElement, deep: true);
            // チャプターオンリーのときはepisode生成時にエピソードタイトルが分からないので、
            // ここでエピソードタイトルとしてチャプタータイトルを用いる
            var hasChapterTitle = !string.IsNullOrWhiteSpace(chapterTitle);
            if (_chapterOnly || hasChapterTitle)
            {
                var tag = _chapterOnly ? "h2" : "h1";
                var h2 = doc.CreateElement(tag);
                h2.AppendTateChuYokoText(chapterTitle);
                if (section.FirstChild is null)
                {
                    section.AppendChild(h2);
                }
                else
                {
                    section.InsertBefore(h2, section.FirstChild);
                }
            }
            body.AppendChild(section);
        }
    }
}
