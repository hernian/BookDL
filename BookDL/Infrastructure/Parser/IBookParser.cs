using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using BookDL.Domain;
using BookDL.Infrastructure;

namespace BookDL.Infrastructure.Parser
{
    public record DownloadReport(int Start, int Current, int Total);

    public interface IBookParser
    {
        Task<BookInfo> GetBookInfoAsync(CancellationToken ct);
        Task<Book> DownloadBookAsync(BookInfo bookInfo, IProgress<DownloadReport> progress, CancellationToken ct);
    }
}
