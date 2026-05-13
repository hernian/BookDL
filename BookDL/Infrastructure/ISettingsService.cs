using BookDL.Domain;

namespace BookDL.Infrastructure
{
    public interface ISettingsService
    {
        string OutputDirectory { get; set; }
        OutputDataKind OutputDataKind { get; set; }
    }
}
