using BookDL.Domain;
using BookDL.Infrastructure;
using BookDL.Infrastructure.Generator;
using BookDL.Infrastructure.Parser;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace BookDL.Services
{
    public class NotSupportedSiteException(string bookUrl) : Exception("Not supported site.")
    {
        public string BookUrl { get; } = bookUrl;
    }

    public interface IBookDownloadService
    {
        Task<BookInfo> AnalyzeAsync(string bookUrl, CancellationToken ct);
        Task DownloadAsync(BookInfo bookInfo, string outputDirectory, IProgress<DownloadReport> progress, CancellationToken ct);
        string ConstructOutputDirectory(BookInfo bookInfo);
    }

    public class BookDownloadService : IBookDownloadService
    {
        private static readonly TagLog<BookDownloadService> _log = new();
        private readonly ISettingsService _settingsService;
        private readonly IBookParserFactory _bookParserFactory;
        private readonly IGeneratorFactory _generatorFactory;
        private IBookParser? _bookParser;

        public BookDownloadService(
            ISettingsService settingsService,
            IBookParserFactory bookParserFactory,
            IGeneratorFactory generatorFactory
            )
        {
            _settingsService = settingsService;
            _bookParserFactory = bookParserFactory;
            _generatorFactory = generatorFactory;
        }

        public Task<BookInfo> AnalyzeAsync(string bookUrl, CancellationToken ct)
        {
            return Task.Run<BookInfo>(async () =>
            {
                _bookParser = await _bookParserFactory.CreateBookParserAsync(bookUrl, ct);
                if (_bookParser == null)
                {
                    throw new NotSupportedSiteException(bookUrl);
                }
                // InitialyzeAsyncの後はBookInfoに値が設定される
                return _bookParser.BookInfo!;
            }, ct);
        }

        public Task DownloadAsync(BookInfo bookInfo, string outputDirectory, IProgress<DownloadReport> progress, CancellationToken ct)
        {
            return Task.Run(async () =>
            {
                if (_bookParser?.BookInfo?.BookUrl != bookInfo.BookUrl)
                {
                    _bookParser = await _bookParserFactory.CreateBookParserAsync(bookInfo.BookUrl, ct);
                    if (_bookParser == null)
                    {
                        throw new NotSupportedSiteException(bookInfo.BookUrl);
                    }
                }
                var bookParser = await _bookParserFactory.CreateBookParserAsync(bookInfo.BookUrl, ct);
                if (bookParser == null)
                {
                    throw new NotSupportedSiteException(bookInfo.BookUrl);
                }
                var book = await bookParser.DownloadBookAsync(bookInfo, progress, ct);

                var options = new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    WriteIndented = true,
                };
                var text = System.Text.Json.JsonSerializer.Serialize<Book>(book, options);
                File.WriteAllText(@"d:\temp\BookDL\book.json", text, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

                var generator = _generatorFactory.CreateGenerator(_settingsService.OutputDataKind, book, outputDirectory);
                await generator.GenerateOutputAsync(ct);
            }, ct);
        }

        public string ConstructOutputDirectory(BookInfo bookInfo)
        {
            var title = PathHelper.SanitizeForWindowsPathSegment(bookInfo.Title);
            var author = PathHelper.SanitizeForWindowsPathSegment(bookInfo.Author);
            var authorKatakana = PathHelper.SanitizeForWindowsPathSegment(bookInfo.AuthorKatakana);
            var katakanaDir = authorKatakana.Length > 0 ? authorKatakana[0].ToString() : "不明";
            var baseDir = _settingsService.OutputDirectory;
            var outputDirectory = string.Join(Path.DirectorySeparatorChar, [baseDir, katakanaDir, author, title]);
            return outputDirectory;
        }
    }
}
