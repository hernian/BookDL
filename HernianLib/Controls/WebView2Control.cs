using AngleSharp.Dom;
using Microsoft.Web.WebView2.Core;     // CoreWebView2, 設定、イベント引数など
using Microsoft.Web.WebView2.Wpf;      // WebView2 コントロール本体
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace HernianLib.Controls
{
    public class WebView2Control
    {
        public class WebView2ControlException : Exception
        {
            public WebView2ControlException(string message) : base(message) { }
        }

        public class WebView2ControlError : WebView2ControlException
        {
            public WebView2ControlError(string message) : base(message) { }
        }

        private const string GET_ALL_SCRIPT = "new XMLSerializer().serializeToString(document)";

        public class WebView2ControlNavigationError : WebView2ControlError
        {
            public int HttpStatusCode { get; }
            public CoreWebView2WebErrorStatus WebErrorStatus { get; }
            public WebView2ControlNavigationError(string message, int httpStatusCode, CoreWebView2WebErrorStatus webErrorStatus)
                : base(message)
            {
                HttpStatusCode = httpStatusCode;
                WebErrorStatus = webErrorStatus;
            }
        }

        public record NavigationResult(bool IsSuccess, int HttpStatusCode, CoreWebView2WebErrorStatus WebErrorStatus);

        private readonly WebView2 _webView;
        private TaskCompletionSource<NavigationResult>? _navigationTcs;

        public WebView2Control(WebView2 webView)
        {
            _webView = webView;
            _webView.NavigationCompleted += WebView_NavigationCompleted;
        }

        public async Task InitializeWebViewAsync(string dataFolderPath)
        {
            _webView.IsEnabled = false;
            var env = await CoreWebView2Environment.CreateAsync(null, dataFolderPath);
            await _webView.EnsureCoreWebView2Async(env);
            _webView.IsEnabled = true;
        }

        private void WebView_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (_navigationTcs != null)
            {
                _navigationTcs.TrySetResult(new NavigationResult(e.IsSuccess, e.HttpStatusCode, e.WebErrorStatus));
                _navigationTcs = null;
            }
        }

        public async Task<string> NavigateAsync(string url)
        {
            if (_navigationTcs != null)
            {
                throw new InvalidOperationException("前のナビゲーションが完了していません。");
            }
            _navigationTcs = new TaskCompletionSource<NavigationResult>();
            _webView.CoreWebView2.Navigate(url);
            var naviRes = await _navigationTcs.Task;
            if (!naviRes.IsSuccess)
            {
                throw new WebView2ControlNavigationError(
                    $"Navigation Error. URL: {url}",
                    naviRes.HttpStatusCode,
                    naviRes.WebErrorStatus);
            }
            var html = await RunJavaScriptAsync(GET_ALL_SCRIPT);
            return html;
        }

        public async Task<string> RunJavaScriptAsync(string script)
        {
            var jsonResult = await _webView.CoreWebView2.ExecuteScriptAsync(script);
            var result = await Task.Run<string>(() => JsonSerializer.Deserialize<string>(jsonResult) ?? string.Empty);
            return result;
        }
        
        public Uri GetSource()
        {
            return _webView.Source;
        }

    }
}
