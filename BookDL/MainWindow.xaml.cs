using System.Diagnostics;
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
using HernianLib.Controls;

namespace BookDL
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WebView2Contorl _webViewControl;
        public MainWindow()
        {
            InitializeComponent();
            _webViewControl = new WebView2Contorl(webView);
            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;
            await _webViewControl.InitializeWebViewAsync();
            this.IsEnabled = true;
            await Task.Delay(5000);
            var result = await _webViewControl.NavigateAsync("https://www.google.com");
            Debug.WriteLine($"Navigation result: IsSuccess={result.IsSuccess}, HttpStatusCode={result.HttpStatusCode}, WebErrorStatus={result.WebErrorStatus}");
        }
    }
}