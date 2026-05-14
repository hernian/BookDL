using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BookDL.Infrastructure
{
    public interface IResourceService
    {
        Stream OpenEmbeddedItem(string resourceName);
    }

    public class ResourceService : IResourceService
    {
        public Stream OpenEmbeddedItem(string resourceName)
        {
            var assembly = typeof(ResourceService).Assembly;
            var stream = assembly.GetManifestResourceStream(resourceName)
                ?? throw new FileNotFoundException(resourceName);
            return stream;
        }
    }
}
