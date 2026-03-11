using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using HernianLib.Controls;
using System.Diagnostics;
using System.IO;
using System.Reflection.Metadata;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace BookDL
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WebView2Control _webViewControl;
        private CancellationTokenSource? _tcs;
        private BookDownloadController _downloadController;

        public MainWindow()
        {
            InitializeComponent();
            _webViewControl = new WebView2Control(webView);
            _downloadController = new BookDownloadController(_webViewControl);
            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var dataFolderPath = System.IO.Path.Combine(localAppData, "Hernian\\BookDL");
            await _webViewControl.InitializeWebViewAsync(dataFolderPath);
            this.IsEnabled = true;
        }
        private CancellationToken NewTaskCancellationToken()
        {
            if (_tcs != null)
            {
                throw new InvalidOperationException("A cancellation token source is already active. Cancel it before creating a new one");
            }
            _tcs = new CancellationTokenSource();
            cancelButton.IsEnabled = true;
            return _tcs.Token;
        }

        private void FreeTaskCancellationToken()
        {
            _tcs?.Dispose();
            _tcs = null;
            cancelButton.IsEnabled = false;
        }

        private async void analyzeButton_Click(object sender, RoutedEventArgs e)
        {
            if (!analyzeButton.IsEnabled)
            {
                return;
            }
            analyzeButton.IsEnabled = false;
            downloadButton.IsEnabled = false;
            var ct = NewTaskCancellationToken();
            try
            {
                var bookSource = await _downloadController.GetInfoAsync(urlTextBox.Text, ct);
                titleTextBox.Text = bookSource.Title;
                authorTextBox.Text = bookSource.Author;
                downloadButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"An error occurred during analysis: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                FreeTaskCancellationToken();
                analyzeButton.IsEnabled = true;
            }
        }

        private async void downloadButton_Click(object sender, RoutedEventArgs e)
        {
            if (!downloadButton.IsEnabled)
            {
                return;
            }
            downloadButton.IsEnabled = false;
            var ct = NewTaskCancellationToken();
            try
            {
                await _downloadController.DownloadAsync(
                    titleTextBox.Text,
                    titleKanaTextBox.Text,
                    authorTextBox.Text,
                    authorKanaTextBox.Text,
                    outputDirectoryTextBox.Text, ct);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"An error occurred during download: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                FreeTaskCancellationToken();
                downloadButton.IsEnabled = true;
            }
        }
    }
}