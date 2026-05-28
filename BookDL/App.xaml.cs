using BookDL.Domain;
using BookDL.Infrastructure;
using BookDL.Infrastructure.Generator;
using BookDL.Infrastructure.Generator.SingleHtml;
using BookDL.Infrastructure.Parser;
using BookDL.Infrastructure.Parser.BerrysCafe;
using BookDL.Infrastructure.Parser.Narou;
using BookDL.Presentation;
using BookDL.Services;
using BookDL.ViewModels;
using Microsoft.Extensions.DependencyInjection;
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
        private static ServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IStorageService, StorageService>();
            services.AddSingleton<IShellService, ShellService>();
            services.AddSingleton<IWinApi, WinApi>();
            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<IResourceService, ResourceService>();
            services.AddSingleton<ITextWriterFactory, TextWriterFactory>();
            services.AddSingleton<EdgeService>();
            services.AddSingleton<IBrowserService>(sp => sp.GetRequiredService<EdgeService>());
            services.AddSingleton<IBrowserWindow>(sp => sp.GetRequiredService<EdgeService>());
            services.AddSingleton<IBookParserFactoryAdapter, NarouParserFactoryAdapter>();
            services.AddSingleton<IBookParserFactoryAdapter, BerrysCafeParserFactoryAdapter>();
            services.AddSingleton<IBookParserFactory>(sp =>
            {
                var browserService = sp.GetRequiredService<IBrowserService>();
                var f =  new BookParserFactory(browserService);
                f.AddAllFactoryAdapters(sp.GetServices<IBookParserFactoryAdapter>());
                return f;
            });
            services.AddSingleton<IGeneratorFactoryAdapter, SingleHtmlGeneratorFactoryAdapter>();
            services.AddSingleton<IGeneratorFactory, GeneratorFactory>(sp =>
            {
                var f = new GeneratorFactory();
                f.AddAllGeneratorAdapters(sp.GetServices<IGeneratorFactoryAdapter>());
                return f;
            });
            services.AddSingleton<IBookDownloadService, BookDownloadService>();
            services.AddSingleton<OwnerWindowProvider>();
            services.AddSingleton<IOwnerWindowProvider>(sp => sp.GetRequiredService<OwnerWindowProvider>());
            services.AddSingleton<IOwnerWindowSetter>(sp => sp.GetRequiredService<OwnerWindowProvider>());
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<ConfigViewModel>();
            services.AddSingleton<MainWindow>();
            return services.BuildServiceProvider();
        }

        private ServiceProvider _services = ConfigureServices();

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var storageService = _services.GetRequiredService<IStorageService>();
            var logPath = storageService.GetProfilePath("logs", "log-.log");
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

            var mainWindow = _services.GetRequiredService<MainWindow>();
            this.MainWindow = mainWindow;
            mainWindow.Show();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            _services?.Dispose();
        }
    }
}
