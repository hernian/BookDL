using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

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
}
