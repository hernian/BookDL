using System;
using System.Collections.Generic;
using System.Text;
using BookDL.Domain;

namespace BookDL.Infrastructure.Generator.SingleHtml
{
    public class SingleHtmlGeneratorDefinition : IGeneratorDefinition
    {
        public OutputDataKind Kind => OutputDataKind.SingleHtml;

        private readonly IResourceService _resourceService;
        public SingleHtmlGeneratorDefinition(IResourceService resourceService)
        {
            _resourceService = resourceService;
        }
        public IGenerator Create(Book book, string outputDirectory)
        {
            return new SingleHtmlGenerator(book, outputDirectory, _resourceService);
        }
    }
}
