using BookDL.Infrastructure;
using BookDL.ViewModels;
using BookDL.Presentation;
using System.Configuration;
using System.Data;
using System.Windows;
using BookDL.Services;

namespace BookDL
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var settingsService = new SettingsService();
            var applicationService = new ApplicationService(settingsService);
            var mainViewModel = new MainViewModel(settingsService, applicationService);
            var mainWindow = new MainWindow(mainViewModel);
            this.MainWindow = mainWindow;
            mainWindow.Show();
        }
    }

}
