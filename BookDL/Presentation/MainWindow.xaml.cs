using BookDL.Infrastructure;
using BookDL.ViewModels;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;

namespace BookDL.Presentation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly TagLog<MainWindow> Log = new();

        private readonly MainViewModel _mainViewModel;
        private readonly IWinApi _winApi;
        private readonly IBrowserWindow _browserWindow;
        private readonly Toast _toast;
        private bool _initialized = false;
        private bool _closing = false;

        public MainWindow(
            MainViewModel viewModel,
            IWinApi winApi,
            IBrowserWindow browserWindow,
            IOwnerWindowSetter ownerSetter)
        {
            InitializeComponent();

            this.DataContext = viewModel;
            _mainViewModel = viewModel;
            _mainViewModel.ConfigRequired += viewModel_ConfigRequired;

            _winApi = winApi;
            _browserWindow = browserWindow;
            _browserWindow.BrowserClosed += BrowserWindow_BrowserClosed;
            ownerSetter.SetOwner(this);

            _toast = new Toast(this);

            this.ContentRendered += MainWindow_ContentRendered;
            this.Closing += MainWindow_Closing;
            this.IsEnabled = false;

        }

        private async void MainWindow_ContentRendered(object? sender, EventArgs e)
        {
            if (_initialized)
            {
                return;
            }
            Log.Debug($"MainWindow_ContentRendered.");
            _initialized = true;
            var wih= new WindowInteropHelper(this);
            var hWndSelf = wih.Handle;
            var hWndBrowser = _browserWindow.GetBrowserWindow();
            Log.Debug($"MainWindow_ContentRendered. hWndSelf: 0x{hWndSelf:x8}, hWndBrowser: 0x{hWndBrowser:x8}");
            _winApi.SetWindowOwner(hWndSelf, hWndBrowser);
            _winApi.SetForeground(hWndSelf);
            _mainViewModel.Initialize();
            this.IsEnabled = true;
        }

        private void BrowserWindow_BrowserClosed(object? sender, EventArgs e)
        {
            // ブラウザのウィンドウが閉じられたら自身のウィンドウも閉じる
            // ただし、このイベントはProcessが発生させるUIスレッドと異なるスレッドで発火するものである
            Log.Debug($"BrowserWindow_BrowserClosed. _closing: {_closing}");
            // ここは非UIスレッドで実行するので _closing == false だからといって安心して処理できない
            // _closingが false -> true はいつ変化するか分からない
            // ただし true -> false の変化はしないため、ざっくりとtrueなら後の処理は不要と分かる。
            if (_closing)
            {
                return;
            }
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                // ここはUIスレッドで実行するので確実に_closing判定ができる
                if (_closing)
                {
                    return;
                }
                this.Close();
            });
        }

        private void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            _closing = true;
            _mainViewModel?.Cleanup();
            _toast?.Dispose();
        }

        private void viewModel_ConfigRequired(object? sender, ConfigRequiredEventArgs e)
        {
            var dialog = CreateDialog(e.ViewModel);
            dialog.Owner = this;
            e.DialogResult = dialog.ShowDialog();
        }

        private Window CreateDialog(object viewModel)
        {
            return viewModel switch
            {
                ConfigViewModel vm => new ConfigDialog(vm),
                AboutViewModel vm => new AboutDialog(vm),
                _ => throw new NotSupportedException($"Unknown ViewModel: {viewModel.GetType()}")
            };
        }

    }
}