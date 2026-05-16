using BookDL.Domain;
using BookDL.Infrastructure;
using BookDL.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using BookDL.Infrastructure.Parser;
using BookDL.Presentation;

namespace BookDL.ViewModels
{
    public class ConfigRequiredEventArgs : EventArgs
    {
        public ObservableObject ViewModel { get; init; }
        public bool DialogResult { get; set; } = false;

        public ConfigRequiredEventArgs(ObservableObject viewModel)
        {
            ViewModel = viewModel;
        }
    }

    public partial class MainViewModel : ObservableObject
    {
        public event EventHandler<ConfigRequiredEventArgs>? ConfigRequired;

        [ObservableProperty]
        private AppModeKind appMode = AppModeKind.Settings;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ManipulateCommand))]
        [NotifyCanExecuteChangedFor(nameof(AnalyzeCommand))]
        [NotifyCanExecuteChangedFor(nameof(DownloadCommand))]
        private string bookUrl = "https://syosetu.com/";

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DownloadCommand))]
        [NotifyCanExecuteChangedFor(nameof(SuggestOutputDirectoryPathCommand))]
        private string title = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DownloadCommand))]
        private string titleKatakana = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DownloadCommand))]
        [NotifyCanExecuteChangedFor(nameof(SuggestOutputDirectoryPathCommand))]
        private string author = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DownloadCommand))]
        [NotifyCanExecuteChangedFor(nameof(SuggestOutputDirectoryPathCommand))]
        private string authorKatakana = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DownloadCommand))]
        [NotifyCanExecuteChangedFor(nameof(SuggestOutputDirectoryPathCommand))]
        [NotifyCanExecuteChangedFor(nameof(OpenOutputDirectoryCommand))]
        private string outputDirectory = string.Empty;

        [ObservableProperty]
        private double downloadProgress = 0.5;
        [ObservableProperty]
        private int startEpisode;

        [ObservableProperty]
        private DownloadReport downloadReport = new DownloadReport(0, 0, 0);

        private readonly ISettingsService _settingsService;
        private readonly IBookDownloadService _bookDownloadService;

        public MainViewModel(
            ISettingsService settingsService,
            IBookDownloadService bookDownloadService
            )
        {
            _settingsService = settingsService;
            _bookDownloadService = bookDownloadService;
            var currentState = _settingsService.CurrentState;
            this.BookUrl = currentState.BookInfo.BookUrl;
            this.Title = currentState.BookInfo.Title;
            this.TitleKatakana = currentState.BookInfo.TitleKatakana;
            this.Author = currentState.BookInfo.Author;
            this.AuthorKatakana = currentState.BookInfo.AuthorKatakana;
            this.OutputDirectory = currentState.OutputDirectory;
        }

        public void Initialize()
        {
            if (string.IsNullOrWhiteSpace(_settingsService.OutputDirectory))
            {
                this.Config();
            }
        }

        [RelayCommand(CanExecute = nameof(CanManipulate))]
        private void Manipulate()
        {
            AppMode = AppModeKind.Manipulate;
        }

        private bool CanManipulate()
        {
            return !string.IsNullOrWhiteSpace(this.BookUrl);
        }

        [RelayCommand]
        private void EndManipulate()
        {
            AppMode = AppModeKind.Settings;
        }

        [RelayCommand(CanExecute = nameof(CanAnalyze))]
        private async Task Analyze(CancellationToken ct)
        {
            AppMode = AppModeKind.Analyze;
            try
            {
                var bookUrl = this.BookUrl;
                var bookInfo = await _bookDownloadService.AnalyzeAsync(bookUrl, ct);
                this.Title = bookInfo.Title;
                this.TitleKatakana = bookInfo.TitleKatakana;
                this.Author = bookInfo.Author;
                this.AuthorKatakana = bookInfo.AuthorKatakana;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                AppMode = AppModeKind.Settings;
            }
        }

        private bool CanAnalyze()
        {
            return !string.IsNullOrWhiteSpace(this.BookUrl);
        }

        [RelayCommand(CanExecute = nameof(CanSuggestOutputDirectoryPath))]
        private void SuggestOutputDirectoryPath()
        {
            Debug.Write("CreateOutputDirectoryPath");
            var bookInfo = GetBookInfo();
            this.OutputDirectory = _bookDownloadService.ConstructOutputDirectory(bookInfo);
            ToastMessage.SendInformation("出力ディレクトリへ推奨値を設定しました");
        }

        private bool CanSuggestOutputDirectoryPath()
        {
            var r = !string.IsNullOrWhiteSpace(this.Title)
                && !string.IsNullOrWhiteSpace(this.Author)
                && !string.IsNullOrWhiteSpace(this.AuthorKatakana)
                && !string.IsNullOrWhiteSpace(_settingsService.OutputDirectory);
            Debug.WriteLine($"CanSuggestOutputDirectoryPath. r: {r}");
            return r;
        }

        [RelayCommand]
        private void BrowseOutputDirectory()
        {
            Debug.Write("BrowseOutputDirectory");
        }

        [RelayCommand(CanExecute = nameof(CanDownload), IncludeCancelCommand = true)]
        private async Task Download(CancellationToken ct)
        {
            AppMode = AppModeKind.Progress;
            try
            {
                var bookInfo = GetBookInfo();
                _settingsService.CurrentState = new CurrentState(bookInfo, this.OutputDirectory);
                _settingsService.Save();
                var outputDirectory = this.OutputDirectory;
                var progress = new Progress<DownloadReport>(DownloadReportChanged);
                await _bookDownloadService.DownloadAsync(bookInfo, outputDirectory, progress, ct);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                AppMode = AppModeKind.Settings;
            }
        }
        
        private void DownloadReportChanged(DownloadReport downloadReport)
        {
            Debug.WriteLine($"MainViewModel.DownloadReportChanged. downloadReport: {downloadReport}");
            this.DownloadReport = downloadReport;
        }

        private bool CanDownload()
        {
            return !string.IsNullOrWhiteSpace(this.BookUrl)
                && !string.IsNullOrWhiteSpace(this.Title)
                && !string.IsNullOrWhiteSpace(this.TitleKatakana)
                && !string.IsNullOrWhiteSpace(this.Author)
                && !string.IsNullOrWhiteSpace(this.AuthorKatakana)
                && !string.IsNullOrWhiteSpace(this.OutputDirectory);
        }

        [RelayCommand(CanExecute = nameof(CanOpenOutputDirectory))]
        private void OpenOutputDirectory()
        {
            Debug.Write("OpenOutputDirectory");
        }

        private bool CanOpenOutputDirectory()
        {
            return !string.IsNullOrWhiteSpace(this.OutputDirectory);
        }

        [RelayCommand]
        private void Config()
        {
            var configViewModel = new ConfigViewModel(_settingsService);
            var eventArgs = new ConfigRequiredEventArgs(configViewModel);
            this.ConfigRequired?.Invoke(this, eventArgs);
            if (eventArgs.DialogResult)
            {
                this.SuggestOutputDirectoryPathCommand.NotifyCanExecuteChanged();
            }
        }

        [RelayCommand]
        private void About()
        {
            var aboutViewModel = new AboutViewModel();
            var eventArgs = new ConfigRequiredEventArgs(aboutViewModel);
            this.ConfigRequired?.Invoke(this, eventArgs);
        }

        public void Cleanup()
        {
            var bookInfo = GetBookInfo();
            _settingsService.CurrentState = new CurrentState(bookInfo, this.OutputDirectory);
            _settingsService.Save();
        }

        private BookInfo GetBookInfo()
        {
            return new BookInfo(
                this.BookUrl,
                this.Title,
                this.TitleKatakana,
                this.Author,
                this.AuthorKatakana);
        }
    }
}
