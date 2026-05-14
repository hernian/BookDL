using BookDL.Domain;
using BookDL.Infrastructure;
using BookDL.Infrastructure.Parser;

namespace BookDL.Services
{
    public class NotSupportedSiteException(string bookUrl) : Exception("Not supported site.")
    {
        public string BookUrl { get; } = bookUrl;
    }

    public interface IBookDownloadService : IDisposable
    {
        Task InitializeAsync();
        Task<BookInfo> AnalyzeAsync(string bookUrl, CancellationToken ct);
        Task DownloadAsync(BookInfo bookInfo, IProgress<DownloadReport> progress, CancellationToken ct);
    }


    public class BookDownloadService : IBookDownloadService
    {
        private static readonly TagLog<BookDownloadService> _log = new();
        private readonly ISettingsService _settingsService;
        private readonly IBookParserFactory _bookParserFactory;
        private readonly Lazy<IBrowserService> _browserService;
        public BookDownloadService(
            ISettingsService settingsService,
            IBookParserFactory bookParserFactory,
            Lazy<IBrowserService> browserService
            )
        {
            _settingsService = settingsService;
            _bookParserFactory = bookParserFactory;
            _browserService = browserService;
        }

        public void Dispose()
        {
            if (_browserService.IsValueCreated)
            {
                // TODO: これは時間がかかる可能性があるので IAsyncDisposable化を検討せよ
                _browserService.Value.Dispose();
            }
        }

        public Task InitializeAsync()
        {
            return Task.Run(() =>
            {
                // Valueを参照することでブラウザサービスのコンストラクタが走る
                _ = _browserService.Value;
            });
        }

        public Task<BookInfo> AnalyzeAsync(string bookUrl, CancellationToken ct)
        {
            return Task.Run<BookInfo>(async () =>
            {
                var bookParser = await _bookParserFactory.CreateBookParserAsync(_browserService.Value, bookUrl, ct);
                if (bookParser == null)
                {
                    throw new NotSupportedSiteException(bookUrl);
                }
                return await bookParser.GetBookInfoAsync(ct);
            }, ct);
        }

        public Task DownloadAsync(BookInfo bookInfo, IProgress<DownloadReport> progress, CancellationToken ct)
        {
            return Task.Run(async () =>
            {
                var bookParser = await _bookParserFactory.CreateBookParserAsync(_browserService.Value, bookInfo.BookUrl, ct);
                if (bookParser == null)
                {
                    throw new NotSupportedSiteException(bookInfo.BookUrl);
                }
                var book = await bookParser.DownloadBookAsync(bookInfo, progress, ct);
            }, ct);
        }
    }
}
