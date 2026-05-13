using System.Configuration;
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
using BookDL.ViewModels;
using BookDL.Infrastructure;
using System.Windows.Automation.Provider;
using System.ComponentModel;

namespace BookDL.Presentation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();

            this.Closing += MainWindow_Closing;

            DataContext = viewModel;
            viewModel.ConfigRequired += viewModel_ConfigRequired;
        }
        private void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            if (this.DataContext is MainViewModel vm)
            {
                vm.SaveCurrentState();
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