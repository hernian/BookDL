using BookDL.Domain;

namespace BookDL.Infrastructure.Generator
{

    public interface IGeneratorFactory
    {
        IGenerator CreateGenerator(OutputDataKind kind, Book book, string outputDirectory);
    }

    public class GeneratorFactory : IGeneratorFactory
    {
        private readonly Dictionary<OutputDataKind, CreateGeneratorDelegate> _dict = new();
        public void AddGenerator(OutputDataKind kind, CreateGeneratorDelegate create)
        {
            _dict.Add(kind, create);
        }

        public IGenerator CreateGenerator(OutputDataKind kind, Book book, string outputDirectory)
        {
            var cerate = _dict[kind];
            return cerate(book, outputDirectory);
        }
    }
}
