using BookDL.Domain;
using BookDL.Infrastructure;
using BookDL.Infrastructure.Generator;
using BookDL.Infrastructure.Generator.SingleHtml;
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
            var resourceService = new ResourceService();
            var bookParserFactory = new BookParserFactory();
            bookParserFactory.AddParser("なろう", NarouBookParser.CreateAsync);
            var lazyEdgeService = new Lazy<EdgeService>(() => new EdgeService());
            var lazyBrowserService = new Lazy<IBrowserService>(() => lazyEdgeService.Value);
            var lazyBrowserWindow = new Lazy<IBrowserWindow>(() => lazyEdgeService.Value);
            var generatorFactory = new GeneratorFactory();
            CreateGeneratorDelegate createGenerator = (Book book, string outputDirectory) => new SingleHtmlGenerator(book, outputDirectory, resourceService);
            // var createGenerator = (Book book, string outputDirectory) => (IGenerator)new SingleHtmlGenerator(book, outputDirectory, resourceService);
            generatorFactory.AddGenerator(OutputDataKind.SingleHtml, createGenerator);
            var bookDownloadService = new BookDownloadService(
                settingsService,
                bookParserFactory,
                lazyBrowserService,
                generatorFactory);
            var mainViewModel = new MainViewModel(settingsService, bookDownloadService);
            var mainWindow = new MainWindow()
            {
                DataContext = mainViewModel,
                BrowserWindow = lazyBrowserWindow
            };
            this.MainWindow = mainWindow;
            mainWindow.Show();
        }
    }

}
