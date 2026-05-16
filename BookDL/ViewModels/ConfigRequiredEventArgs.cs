using CommunityToolkit.Mvvm.ComponentModel;

namespace BookDL.ViewModels
{
    public class ConfigRequiredEventArgs : EventArgs
    {
        public ObservableObject ViewModel { get; init; }
        public bool? DialogResult { get; set; }

        public ConfigRequiredEventArgs(ObservableObject viewModel)
        {
            ViewModel = viewModel;
        }
    }
}
