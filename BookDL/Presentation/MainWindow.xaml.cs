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

        public Lazy<IBrowserWindow>? BrowserWindow { get; set; }

        private bool _initialized = false;
        private bool _closing = false;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContextChanged += MainWindow_DataContextChanged;
            this.ContentRendered += MainWindow_ContentRendered;
            this.Closing += MainWindow_Closing;
            this.IsEnabled = false;
        }

        private void MainWindow_DataContextChanged(object? sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is MainViewModel oldViewModel)
            {
                oldViewModel.ConfigRequired -= viewModel_ConfigRequired;
            }
            if (e.NewValue is MainViewModel newViewModel)
            {
                newViewModel.ConfigRequired += viewModel_ConfigRequired;
            }
        }
        private async void MainWindow_ContentRendered(object? sender, EventArgs e)
        {
            if (_initialized)
            {
                return;
            }
            Log.Debug($"MainWindow_ContentRendered. DataContext: {this.DataContext is MainViewModel}, BrowserWindow: {this.BrowserWindow != null}");
            if (this.DataContext is MainViewModel vm)
            {
                if (this.BrowserWindow != null)
                {
                    _initialized = true;
                    var wih= new WindowInteropHelper(this);
                    var hWndSelf = wih.Handle;
                    // this.BrowserWindow.Valueの初回参照は時間がかかるので非UIスレッドで実行する
                    var hWndBrowser = await Task.Run<IntPtr>(() => this.BrowserWindow.Value.GetBrowserWindow());
                    this.BrowserWindow.Value.BrowserClosed += BrowserWindow_BrowserClosed;
                    Log.Debug($"MainWindow_ContentRendered. hWndSelf: 0x{hWndSelf:x8}, hWndBrowser: 0x{hWndBrowser:x8}");
                    WinApi.SetWindowOwner(hWndSelf, hWndBrowser);
                    WinApi.SetForegroundWindow(hWndSelf);
                    await vm.InitializeAsync();
                    this.IsEnabled = true;
                }
            }
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
            if (this.DataContext is MainViewModel vm)
            {
                vm.Cleanup();
            }
        }

        private void viewModel_ConfigRequired(object? sender, ConfigRequiredEventArgs e)
        {
            var dialog = CreateDialog(e.ViewModel);
            dialog.Owner = this;
            var res = dialog.ShowDialog();
            e.DialogResult = res.HasValue ? res.Value : false;
        }

        private Window CreateDialog(object viewModel)
        {
            if (viewModel is ConfigViewModel configViewModel)
            {
                return new ConfigDialog(configViewModel);
            }
            if (viewModel is AboutViewModel aboutViewModel)
            {
                return new AboutDialog(aboutViewModel);
            }
            throw new InvalidOperationException($"Unknown viewModel. viewModel: {viewModel}");
        }

    }
}