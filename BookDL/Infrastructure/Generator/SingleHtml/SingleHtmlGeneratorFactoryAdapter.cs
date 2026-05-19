using BookDL.Domain;

namespace BookDL.Infrastructure.Generator.SingleHtml
{
    public class SingleHtmlGeneratorFactoryAdapter : IGeneratorFactoryAdapter
    {
        public OutputDataKind Kind => OutputDataKind.SingleHtml;

        private readonly IResourceService _resourceService;
        private readonly ITextWriterFactory _storageService;
        public SingleHtmlGeneratorFactoryAdapter(IResourceService resourceService, ITextWriterFactory storateService)
        {
            _resourceService = resourceService;
            _storageService = storateService;
        }
        public IGenerator Create(Book book, string outputDirectory)
        {
            return new SingleHtmlGenerator(book, outputDirectory, _resourceService, _storageService);
        }
    }
}
