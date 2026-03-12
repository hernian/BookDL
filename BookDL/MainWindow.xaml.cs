using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using HernianLib.Controls;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.IO.Enumeration;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
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
        private const string DEFAULT_OUTPUT_DIRECTORY = @"D:\MyKindle";

        private WebView2Control _webViewControl;
        private CancellationTokenSource? _tcs;
        private BookDownloadController _downloadController;

        public MainWindow()
        {
            InitializeComponent();
            _webViewControl = new WebView2Control(webView);
            _downloadController = new BookDownloadController(_webViewControl);
            Loaded += MainWindow_Loaded;

            outputDirectoryTextBox.Text = DEFAULT_OUTPUT_DIRECTORY;
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
                await _downloadController.DownloadAsync(urlTextBox.Text, titleTextBox.Text, outputDirectoryTextBox.Text, ct);
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

        private void menuNewSettings_Click(object sender, RoutedEventArgs e)
        {
            urlTextBox.Text = string.Empty;
            titleTextBox.Text = string.Empty;
            titleKanaTextBox.Text = string.Empty;
            authorTextBox.Text = string.Empty;
            authorKanaTextBox.Text = string.Empty;
            outputDirectoryTextBox.Text = DEFAULT_OUTPUT_DIRECTORY;
        }

        private async void menuLoadSettings_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = BookSettingsDialogHelper.CreateOpenFileDialog(outputDirectoryTextBox.Text);
            if (openFileDialog.ShowDialog(this) == true)
            {
                try
                {
                    var repository = new BookSettingsRepository(openFileDialog.FileName);
                    var settings = await repository.LoadAsync();
                    urlTextBox.Text = settings.BookUrl;
                    titleTextBox.Text = settings.Title;
                    titleKanaTextBox.Text = settings.TitleKana;
                    authorTextBox.Text = settings.Author;
                    authorKanaTextBox.Text = settings.AuthorKana;
                    outputDirectoryTextBox.Text = settings.OutputDirectory;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, $"設定の読み込みに失敗しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void menuSaveSettings_Click(object sender, RoutedEventArgs e)
        {
            var outputDirectory = outputDirectoryTextBox.Text;
            try {
                if (string.IsNullOrEmpty(outputDirectory))
                {
                    throw new Exception("出力ディレクトリが指定されていません。");
                }
                if (!Directory.Exists(outputDirectory))
                {
                    Directory.CreateDirectory(outputDirectory);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"出力ディレクトリの確認に失敗しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var fileName = BookConverter.GetFileNameFromTitle(titleTextBox.Text);
            var saveFileDialog = BookSettingsDialogHelper.CretateSaveFileDialog(fileName, outputDirectoryTextBox.Text);
            if (saveFileDialog.ShowDialog(this) == true)
            {
                try
                {
                    var settings = new BookSettings(
                        urlTextBox.Text,
                        titleTextBox.Text,
                        titleKanaTextBox.Text,
                        authorTextBox.Text,
                        authorKanaTextBox.Text,
                        outputDirectoryTextBox.Text
                    );
                    var repository = new BookSettingsRepository(saveFileDialog.FileName);
                    await repository.SaveAsync(settings);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, $"設定の保存に失敗しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void menuExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void menuAbout_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(this, "BookDL\nバージョン 1.0", "バージョン情報", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
