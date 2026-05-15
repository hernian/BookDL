using BookDL.Domain;

namespace BookDL.Infrastructure.Generator
{
    public interface IGeneratorDefinition
    {
        OutputDataKind Kind { get; }
        IGenerator Create(Book book, string outputDirectory);
    }
}
