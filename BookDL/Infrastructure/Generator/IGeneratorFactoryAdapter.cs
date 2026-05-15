using BookDL.Domain;

namespace BookDL.Infrastructure.Generator
{
    public interface IGeneratorFactoryAdapter
    {
        OutputDataKind Kind { get; }
        IGenerator Create(Book book, string outputDirectory);
    }
}
