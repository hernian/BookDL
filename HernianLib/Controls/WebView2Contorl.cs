using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Wpf;      // WebView2 コントロール本体
using Microsoft.Web.WebView2.Core;     // CoreWebView2, 設定、イベント引数など


namespace HernianLib.Controls
{
    public class WebView2Contorl
    {
        public record NavigationResult(bool IsSuccess, int HttpStatusCode, CoreWebView2WebErrorStatus WebErrorStatus);

        private readonly WebView2 _webView;
        private TaskCompletionSource<NavigationResult>? _navigationTcs;

        public WebView2Contorl(WebView2 webView)
        {
            _webView = webView;
            _webView.NavigationCompleted += WebView_NavigationCompleted;
        }

        public async Task InitializeWebViewAsync()
        {
            _webView.IsEnabled = false;
            await _webView.EnsureCoreWebView2Async();
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

        public Task<NavigationResult> NavigateAsync(string url)
        {
            if (_navigationTcs != null)
            {
                throw new InvalidOperationException("前のナビゲーションが完了していません。");
            }
            _navigationTcs = new TaskCompletionSource<NavigationResult>();
            _webView.CoreWebView2.Navigate(url);
            return _navigationTcs.Task;
        }
    }
}
