using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Windows.Controls;
using System.IO;
using System.Text.Json;
using System.Web;

namespace BookDL.Test.TestUtilities
{
    public class HtmlLoader
    {
        private record UrlName(string Url, string Name);

        private readonly Assembly _asm = Assembly.GetExecutingAssembly();
        private readonly string _resourcePrefix;
        private readonly Dictionary<string, string> _htmlMap = new();

        public Dictionary<string, string> HtmlMap => _htmlMap;

        public HtmlLoader(string resorcePrefix)
        {
            _resourcePrefix = resorcePrefix;
        }

        public string LoadHtmlForUrl(string url)
        {
            var resourceName = ConvertUrlToResourceName(url);
            return LoadEmbeddedResource(resourceName);
        }

        private UrlName[] LoadManifest(string fullName)
        {
            using var stream = _asm.GetManifestResourceStream(fullName)
                ?? throw new InvalidOperationException($"Missing resource. Name: {fullName}");
            var res = JsonSerializer.Deserialize<UrlName[]>(stream)
                ?? throw new InvalidOperationException($"Invalid manifest. Name: {fullName}");
            return res;
        }

        public void LoadHtmlMap(IEnumerable<string> urls)
        {
            foreach (var url in urls)
            {
                var html = LoadHtmlForUrl(url);
                _htmlMap.Add(url, html);
            }
        }

        public void Test()
        {
            var urlNameList = new List<UrlName>();
            urlNameList.Add(new UrlName("https://www.berrys-cafe.jp/book/n1779829", "www.berrys-cafe.jp.book.n1779829"));
            urlNameList.Add(new UrlName("https://www.berrys-cafe.jp/book/n1779829/1", "www.berrys-cafe.jp.book.n1779829.1"));
            urlNameList.Add(new UrlName("https://www.berrys-cafe.jp/book/n1779829/2", "www.berrys-cafe.jp.book.n1779829.2"));
            using var stream = new FileStream(@"D:\MyPrograms\BookDL\BookDL.Test\TestHtml\BerrysCafe2\manifest.json", FileMode.Create, FileAccess.Write);
            JsonSerializer.Serialize(stream, urlNameList);
        }

        public void LoadByManifest(string manifest)
        {
            var urlNames = LoadManifest($"{_resourcePrefix}.{manifest}");
            foreach (var urlName in urlNames)
            {
                var fullName = $"{_resourcePrefix}.{urlName.Name}";
                var html = LoadEmbeddedResource(fullName);
                _htmlMap[urlName.Url] = html;
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
