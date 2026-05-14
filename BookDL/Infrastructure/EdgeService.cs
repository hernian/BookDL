using BookDL.Domain;
using OpenQA.Selenium.Edge;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace BookDL.Infrastructure
{
    public interface IBrowserService : IDisposable
    {
        void SetWindowOwner(IntPtr hWndTarget);
        void SetBrowserForeground();
        void Navigate(string url);
        void RunJavaScript(string script);
        string CallJavaScript(string script);
        string GetCurrentUrl();
        string GetDom();
    }

    public class EdgeService : IBrowserService
    {
        private static readonly TagLog<EdgeService> Log = new();

        private const string SCRIPT_GET_DOM = "return document.documentElement.outerHTML;";
        private const string SCRIPT_GET_URL = "return window.location.href;";
        private const int GET_BROWSER_WINDOW_RETRY_COUNT = 50;
        private const int GET_BROWSER_WINDOW_RETRY_INTERVAL = 100; // ms

        private static IntPtr FindBrowserWindow(EdgeDriver driver, out string uuidTitle)
        {
            uuidTitle = $"BookDL-{Guid.NewGuid()}";
            driver.ExecuteScript($"document.title = '{uuidTitle}'");
            Thread.Sleep(1000);
            var hWnd = IntPtr.Zero;
            for (var i = 0; i < GET_BROWSER_WINDOW_RETRY_COUNT; ++i)
            {
                hWnd = WinApi.FindWindowByTitleContains(uuidTitle);
                if (hWnd != IntPtr.Zero)
                {
                    break;
                }
                Thread.Sleep(GET_BROWSER_WINDOW_RETRY_INTERVAL);
            }
            return hWnd;
        }

        public event EventHandler? BrowserClosed;

        private EdgeDriver _driver;
        private IntPtr _hWnd;
        private Process _process;
        private bool _disposed;

        public EdgeService()
        {
            var service = EdgeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;
            var options = new EdgeOptions();
            options.AddArgument("--remote-allow-origins=*");
            // options.AddArgument("--window-size=1000,800");
            _driver = new EdgeDriver(service, options);
            if (_driver == null)
            {
                var msg = "EdgeDriver initialization failed.";
                Log.Error(msg);
                throw new InvalidOperationException(msg);
            }
            _hWnd = FindBrowserWindow(_driver, out string uuidTitle);
            if (_hWnd == IntPtr.Zero)
            {
                var msg = $"Missing browser window. Title: {uuidTitle}";
                Log.Error(msg);
                throw new InvalidOperationException(msg);
            }
            _process = WinApi.GetWindowProcess(_hWnd);
            if (_process == null)
            {
                var msg = $"Missing browser process. hWnd: 0x{_hWnd:x8}";
                Log.Error(msg);
                throw new InvalidOperationException(msg);
            }
            _process.EnableRaisingEvents = true;
            _process.Exited += process_Exited;
            _driver.ExecuteScript($"document.title = 'BookDL'");
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }
            if (disposing)
            {
                // Dispose managed state (managed objects).
                _process?.Close();
                _driver?.Dispose();
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public void SetWindowOwner(IntPtr hWndTarget)
        {
            WinApi.SetWindowOwner(hWndTarget, _hWnd);
        }

        public void SetBrowserForeground()
        {
            WinApi.SetForegroundWindow(_hWnd);
        }

        public void Navigate(string url)
        {
            Log.Debug($"Navigate. url: {url}");
            _driver.Navigate().GoToUrl(url);
        }
        public void RunJavaScript(string script)
        {
            _driver.ExecuteScript(script);
        }

        public string CallJavaScript(string script)
        {
            var res = (string?)_driver.ExecuteScript(script);
            if (res == null)
            {
                var msg = $"RunJavaScript returned null. script: {script}";
                Log.Debug(msg);
                throw new InvalidOperationException(msg);
            }
            return res;
        }

        public string GetCurrentUrl()
        {
            var currentUrl = CallJavaScript(SCRIPT_GET_URL);
            return currentUrl;
        }

        public string GetDom()
        {
            var html = CallJavaScript(SCRIPT_GET_DOM);
            return html;
        }

        private void process_Exited(object? sender, EventArgs e)
        {
            this.BrowserClosed?.Invoke(this, e);
        }

    }
}
