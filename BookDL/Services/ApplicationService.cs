using BookDL.Infrastructure;
using BookDL.Domain;
using System.Diagnostics;

namespace BookDL.Services
{
    public record DownloadReport(int StartEpisode, int CurrentEpisode, int TotalEpisode);

    public interface IApplicationService
    {
        Task<BookInfo> AnalyzeAsync(string bookUrl, CancellationToken ct);
        Task DownloadAsync(BookInfo bookInfo, IProgress<DownloadReport> progress, CancellationToken ct);
    }


    public class ApplicationService : IApplicationService
    {
        private readonly ISettingsService _settingsService;
        public ApplicationService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public async Task<BookInfo> AnalyzeAsync(string bookUrl, CancellationToken ct)
        {
            for (int i = 0; i < 5; i++)
            {
                ct.ThrowIfCancellationRequested();
                await Task.Delay(1000);
            }
            return new BookInfo(bookUrl, "t", "tk", "a", "ak");
        }
        public Task DownloadAsync(BookInfo bookInfo, IProgress<DownloadReport> progress, CancellationToken ct)
        {
            return Task.Run(() =>
            {
                var end = 5;
                for (int i = 0; i < end; i++)
                {
                    ct.ThrowIfCancellationRequested();
                    Debug.WriteLine($"ApplicationService.DownloadAsync. Report: {i}");
                    progress.Report(new DownloadReport(0, i, end));
                    Task.Delay(3000).Wait();
                }
            }, ct);
        }
    }
}
