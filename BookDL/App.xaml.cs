using BookDL.Infrastructure;
using BookDL.Infrastructure.Parser;
using BookDL.Infrastructure.Parser.Narou;
using BookDL.Presentation;
using BookDL.Services;
using BookDL.ViewModels;
using Serilog;
using System.IO;
using System.Windows;

namespace BookDL
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var localAppPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var logPath = Path.Combine(localAppPath, "Hernian", "BookDL", "logs", "log-.log");
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(
                    path: logPath,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.Debug()
                .CreateLogger();
            Log.Information("BooDL started.");

            var settingsService = new SettingsService();
            var bookParserFactory = new BookParserFactory();
            bookParserFactory.AddParser("なろう", NarouBookParser.CreateAsync);
            var browserService = new Lazy<IBrowserService>(() => new EdgeService());
            var bookDownloadService = new BookDownloadService(
                settingsService,
                bookParserFactory,
                browserService);
            var mainViewModel = new MainViewModel(settingsService, bookDownloadService);
            var mainWindow = new MainWindow(mainViewModel);
            this.MainWindow = mainWindow;
            mainWindow.Show();
        }
    }

}
