using BookDL.Domain;
using BookDL.Infrastructure;
using BookDL.Infrastructure.Parser;
using BookDL.Infrastructure.Generator;
using System.IO;

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
                var bookParser = await _bookParserFactory.CreateBookParserAsync(bookUrl, ct);
                if (bookParser == null)
                {
                    throw new NotSupportedSiteException(bookUrl);
                }
                return await bookParser.GetBookInfoAsync(ct);
            }, ct);
        }

        public Task DownloadAsync(BookInfo bookInfo, string outputDirectory, IProgress<DownloadReport> progress, CancellationToken ct)
        {
            return Task.Run(async () =>
            {
                var bookParser = await _bookParserFactory.CreateBookParserAsync(bookInfo.BookUrl, ct);
                if (bookParser == null)
                {
                    throw new NotSupportedSiteException(bookInfo.BookUrl);
                }
                var book = await bookParser.DownloadBookAsync(bookInfo, progress, ct);
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
