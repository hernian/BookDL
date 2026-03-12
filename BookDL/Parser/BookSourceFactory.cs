using BookDL.Parser.Narou;
using HernianLib.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace BookDL.Parser
{
    public class BookSourceFactory
    {
        public delegate IBookSource CreateBookSourceDelegate(WebView2Control webViewControl, string bookUrl);

        static BookSourceFactory()
        {
            TheInstance = new BookSourceFactory();
            TheInstance.RegisterAllBookSources();
        }

        public static BookSourceFactory TheInstance { get; }

        private readonly Dictionary<string, CreateBookSourceDelegate> _bookSourceCreators = new();

        private void RegisterAllBookSources()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var bookSourceTypes = assembly.GetTypes()
                        .Where(t => typeof(IBookSource).IsAssignableFrom(t)
                                 && t.IsClass
                                 && !t.IsAbstract);
            foreach (var bst in bookSourceTypes)
            {
                RuntimeHelpers.RunClassConstructor(bst.TypeHandle);
            }
        }

        public void RegisterBookSource(string key, CreateBookSourceDelegate createBookSource)
        {
            _bookSourceCreators[key] = createBookSource;
        }

        public IBookSource CreateBookSource(WebView2Control webViewControl, string bookUrl)
        {
            var uri = new Uri(bookUrl);
            var host = uri.Host.ToLowerInvariant();

            foreach (var (key, creator) in _bookSourceCreators)
            {
                if (host.EndsWith(key))
                {
                    var bookSource = creator(webViewControl, bookUrl);
                    return bookSource;
                }
            }
            throw new NotSupportedException($"The host '{uri.Host}' is not supported.");
        }
    }
}
