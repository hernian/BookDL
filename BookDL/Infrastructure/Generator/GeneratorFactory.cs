using BookDL.Domain;

namespace BookDL.Infrastructure.Generator
{

    public interface IGeneratorFactory
    {
        IGenerator CreateGenerator(OutputDataKind kind, Book book, string outputDirectory);
    }

    public class GeneratorFactory : IGeneratorFactory
    {
        private readonly Dictionary<OutputDataKind, IGeneratorFactoryAdapter> _dict = new();
        public GeneratorFactory()
        {
        }

        public void AddGeneratorAdapter(IGeneratorFactoryAdapter genDef)
        {
            _dict.Add(genDef.Kind, genDef);
        }

        public void AddAllGeneratorAdapters(IEnumerable<IGeneratorFactoryAdapter> genDefs)
        {
            foreach (var genDef in genDefs)
            {
                _dict.Add(genDef.Kind, genDef);
            }
        }

        public IGenerator CreateGenerator(OutputDataKind kind, Book book, string outputDirectory)
        {
            var cerator = _dict[kind];
            return cerator.Create(book, outputDirectory);
        }
    }
}
