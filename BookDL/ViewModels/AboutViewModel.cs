using AngleSharp.Dom;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BookDL.ViewModels
{
    public partial class AboutViewModel : ObservableObject
    {
        public event EventHandler? CloseRequired;

        [ObservableProperty]
        private string appVersion;

        public AboutViewModel()
        {
            AppVersion = "1.2.3.4";
        }

        [RelayCommand]
        private void Close()
        {
            this.CloseRequired?.Invoke(this, EventArgs.Empty);
        }
    }
}
