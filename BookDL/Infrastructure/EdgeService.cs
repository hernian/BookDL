using OpenQA.Selenium.Edge;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;

namespace BookDL.Infrastructure
{
    public interface IBrowserService
    {
        void SetBrowserForeground();
        void Navigate(string url);
        void RunJavaScript(string script);
        string CallJavaScript(string script);
        string GetCurrentUrl();
        string GetDom();
    }

    public interface IBrowserWindow
    {
        event EventHandler? BrowserClosed;
        IntPtr GetBrowserWindow();
        void Initialize(int left, int top);
    }

    public class EdgeService : IBrowserService, IBrowserWindow, IDisposable
    {
        private static readonly TagLog<EdgeService> Log = new();

        private const string SCRIPT_GET_DOM = "return document.documentElement.outerHTML;";
        private const string SCRIPT_GET_URL = "return window.location.href;";
        private const int GET_BROWSER_WINDOW_DELAY = 100; // 100ms
        private const int GET_BROWSER_WINDOW_RETRY_COUNT = 100;
        private const int GET_BROWSER_WINDOW_RETRY_INTERVAL = 100; // 200ms

        public event EventHandler? BrowserClosed;

        private readonly IWinApi _winApi;
        private readonly IStorageService _storageService;
        private readonly Stream _lockStream;
        private EdgeDriver _driver;
        private IntPtr _hWnd;
        private Process _process;
        private bool _disposed;

        public EdgeService(IWinApi winApi, IStorageService storageService)
        {
            _winApi = winApi;
            _storageService = storageService;

            var profileRoot = _storageService.GetProfilePath("profile");
            _storageService.CreateDirectory(profileRoot);

            var lockFilePath = Path.Combine(profileRoot, "BookDL.lock");
            _lockStream = _storageService.OpenWriteSteam(lockFilePath);
            var service = EdgeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;
            var options = new EdgeOptions();
            options.AddArgument("--no-first-run");
            options.AddArgument("--no-default-browser-check");
            options.AddArgument($"--user-data-dir={profileRoot}");
            options.AddArgument("--profile-directory=Default");
            // Edgeのデフォルト画面がニュース等表示されているので
            // 最初は見えないところでEdgeを表示し、
            // 後に about:blank を表示させてから見えるところへ移動する。
            options.AddArgument("--window-position=-32000,-32000");
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
            _process = _winApi.GetWindowProcess(_hWnd);
            if (_process == null)
            {
                var msg = $"Missing browser process. hWnd: 0x{_hWnd:x8}";
                Log.Error(msg);
                throw new InvalidOperationException(msg);
            }
            _process.EnableRaisingEvents = true;
            _process.Exited += process_Exited;
            _driver.ExecuteScript($"document.title = 'BookDL'");
            _driver.Navigate().GoToUrl("about:blank");
            _driver.Manage().Window.Position = new System.Drawing.Point(0, 0);
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
                _lockStream?.Dispose();
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Log.Debug("Dispose called.");
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public void Initialize(int left, int top)
        {
            var swpp = new SetWindowPosParam(
                ChangePosition: true,
                Left: left + 80,
                Top: top + 10);
            _winApi.SetWindowPos(_hWnd, swpp);
        }

        public IntPtr GetBrowserWindow()
        {
            return _hWnd;
        }

        public void SetBrowserForeground()
        {
            _winApi.SetForeground(_hWnd);
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

        private IntPtr FindBrowserWindow(EdgeDriver driver, out string uuidTitle)
        {
            uuidTitle = $"BookDL-{Guid.NewGuid()}";
            Thread.Sleep(GET_BROWSER_WINDOW_DELAY);
            var hWnd = IntPtr.Zero;
            for (var i = 0; i < GET_BROWSER_WINDOW_RETRY_COUNT; ++i)
            {
                try
                {
                    driver.ExecuteScript($"document.title = '{uuidTitle}'");
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Setting Edge title failed.");
                }
                Thread.Sleep(GET_BROWSER_WINDOW_RETRY_INTERVAL);
                hWnd = _winApi.FindWindowByTitleContains(uuidTitle);
                if (hWnd != IntPtr.Zero)
                {
                    break;
                }
            }
            return hWnd;
        }

        private void process_Exited(object? sender, EventArgs e)
        {
            this.BrowserClosed?.Invoke(this, e);
        }
}

public static class EdgeBlankProfileFactory
    {
        public static void EnsureBlankProfile(string profilePath)
        {
            var preferencesPath = Path.Combine(profilePath, "Preferences");

/*
            if (File.Exists(preferencesPath))
            {
                // 既存を壊したくない場合は何もしない
                return;
            }
*/
            var preferences = new
            {
                session = new
                {
                    startup_urls = new[] { "about:blank" },
                    restore_on_startup = 4
                },
                homepage = "about:blank",
                homepage_is_newtabpage = false,
                browser = new
                {
                    show_home_button = true,
                    check_default_browser = false
                },
                ntp = new
                {
                    custom_links = new
                    {
                        initialized = true
                    }
                },
                distribution = new
                {
                    import_bookmarks = false,
                    import_history = false,
                    import_home_page = false,
                    import_search_engine = false,
                    import_saved_passwords = false,
                    import_autofill_form_data = false,
                    import_extensions = false,
                    import_cookies = false
                },
                profile = new
                {
                    exit_type = "None",
                    last_used = "Default"
                },
                browser_onboarding = new
                {
                    enabled = false
                },
                first_run_tabs = Array.Empty<string>()
            };

            var json = JsonSerializer.Serialize(
                preferences,
                new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });

            File.WriteAllText(preferencesPath, json, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }
    }
}
