using BookDL.Domain;

namespace BookDL.Infrastructure.Generator
{
    public interface IGenerator
    {
        Task GenerateOutputAsync(CancellationToken ct);
    }
}
