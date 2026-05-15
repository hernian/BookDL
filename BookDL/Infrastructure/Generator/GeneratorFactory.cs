using BookDL.Domain;

namespace BookDL.Infrastructure.Generator
{

    public interface IGeneratorFactory
    {
        IGenerator CreateGenerator(OutputDataKind kind, Book book, string outputDirectory);
    }

    public class GeneratorFactory : IGeneratorFactory
    {
        private readonly Dictionary<OutputDataKind, IGeneratorDefinition> _dict = new();
        public void AddGenerator(IGeneratorDefinition genDef)
        {
            _dict.Add(genDef.Kind, genDef);
        }
        public void AddAllGenerator(IEnumerable<IGeneratorDefinition> genDefs)
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
