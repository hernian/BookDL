using BookDL.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Navigation;

namespace BookDL.Test.FakeServices
{
    public class FakeBrowserService : IBrowserService
    {
        private readonly Dictionary<string, string> _htmlMap;
        private string _currentUrl = string.Empty;

        public FakeBrowserService(Dictionary<string, string> htmlMap)
        {
            _htmlMap = htmlMap;
        }

        public void Navigate(string url)
        {
            _currentUrl = url;
        }

        public string GetCurrentUrl()
        {
            return _currentUrl;
        }

        public string GetDom()
        {
            if (!_htmlMap.TryGetValue(_currentUrl, out string? html))
            {
                throw new InvalidOperationException($"GetDom. Invalid url. {_currentUrl}");
            }
            return html!;
        }

        public void SetBrowserForeground()
        {
            // nothing to do.
        }

        public void RunJavaScript(string script) => throw new NotImplementedException();
        public string CallJavaScript(string script) => throw new NotImplementedException();
    }
}
