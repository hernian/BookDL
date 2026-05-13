using BookDL.Infrastructure;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BookDL.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        public event EventHandler<ConfigRequiredEventArgs>? ConfigRequired;

        [ObservableProperty]
        private AppModeKind appMode = AppModeKind.Settings;

        [ObservableProperty]
        private string bookUrl = "https://syosetu.com/";

        [ObservableProperty]
        private string title = string.Empty;

        [ObservableProperty]
        private string titleKatakana = string.Empty;

        [ObservableProperty]
        private string author = string.Empty;

        [ObservableProperty]
        private string authorKatakana = string.Empty;

        [ObservableProperty]
        private string outputDirectory = string.Empty;

        [ObservableProperty]
        private double downloadProgress = 0.5;

        private readonly ISettingsService _settingsService;
        public MainViewModel(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        [RelayCommand]
        private void Manipulate()
        {
            AppMode = AppModeKind.Manipulate;
        }
        [RelayCommand]
        private void EndManipulate()
        {
            AppMode = AppModeKind.Settings;
        }

        [RelayCommand]
        private async Task Analyze()
        {
            AppMode = AppModeKind.Analyze;
            await Task.Delay(3000);
            AppMode = AppModeKind.Settings;
        }

        [RelayCommand]
        private void Download()
        {
            AppMode = AppModeKind.Progress;
        }

        [RelayCommand]
        private void CancelDownload()
        {
            AppMode = AppModeKind.Settings;
        }

        [RelayCommand]
        private void Config()
        {
            var configViewModel = new ConfigViewModel(_settingsService);
            var eventArgs = new ConfigRequiredEventArgs(configViewModel);
            this.ConfigRequired?.Invoke(this, eventArgs);
            if (eventArgs.DialogResult)
            {
                // ToDo: 設定変更されたときの処理
            }
        }

        [RelayCommand]
        private void About()
        {
            var aboutViewModel = new AboutViewModel();
            var eventArgs = new ConfigRequiredEventArgs(aboutViewModel);
            this.ConfigRequired?.Invoke(this, eventArgs);
        }
    }
}
