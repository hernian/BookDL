using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Windows.Controls;
using System.IO;

namespace BookDL.Test.TestUtilities
{
    public class HtmlLoader
    {
        private readonly string _resourcePrefix;
        private readonly Dictionary<string, string> _urlDict = new();

        public Dictionary<string, string> HtmlMap => _urlDict;

        public HtmlLoader(string resorcePrefix)
        {
            _resourcePrefix = resorcePrefix;
        }

        public string LoadHtmlForUrl(string url)
        {
            var resourceName = ConvertUrlToResourceName(url);
            return LoadEmbeddedResource(resourceName);
        }

        public void LoadHtmlMap(IEnumerable<string> urls)
        {
            foreach (var url in urls)
            {
                var html = LoadHtmlForUrl(url);
                _urlDict.Add(url, html);
            }
        }

        public string ConvertUrlToResourceName(string url)
        {
            var uri = new Uri(url);
            var name = uri.AbsolutePath;
            var segments = name.Split("/", StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length > 0) {
                var lastSegment = segments[^1];
                var ext = Path.GetExtension(lastSegment);
                if (string.IsNullOrEmpty(ext))
                {
                    ext = ".html";
                }
                segments[^1] = Path.GetFileNameWithoutExtension(lastSegment);
                name = string.Join(".", segments) + ext;
            }

            var fullName = $"{_resourcePrefix}.{uri.Host}.{name}";
            return fullName;
        }

        private string LoadEmbeddedResource(string fullName)
        {
            var asm = Assembly.GetExecutingAssembly();
            using var stream = asm.GetManifestResourceStream(fullName)
                ?? throw new FileNotFoundException($"Resource not found: {fullName}");

            var enc = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
            using var reader = new StreamReader(stream, enc);
            return reader.ReadToEnd();
        }
    }
}
