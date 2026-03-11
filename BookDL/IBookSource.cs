using System;
using System.Collections.Generic;
using System.Text;

namespace BookDL
{
    public interface IBookSource
    {
        string BookUrl { get; }
        string Title { get; }
        string Author { get; }

        Task GetInfoAsync(CancellationToken ct);
        IAsyncEnumerable<ISectionSource> AllSectionsAsync(CancellationToken ct);
    }
}
