using BookDL.Infrastructure;
using CommunityToolkit.Mvvm.Messaging;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace BookDL.Presentation
{
    /// <summary>
    /// Toast.xaml の相互作用ロジック
    /// </summary>
    public partial class ToastWindow : Window
    {
        private static class ResourceKeys
        {
            public const string InfoDuration = "InfoDuration";
            public const string ErrorDuration = "ErrorDuration";
            public const string InfoBackground = "InfoBackgroundBrush";
            public const string InfoForeground = "InfoForegroundBrush";
            public const string ErrorBackground = "ErrorBackgroundBrush";
            public const string ErrorForeground = "ErrorForegroundBrush";
        }

        private readonly DispatcherTimer _timer;

        public ToastWindow(ToastMessage toast)
        {
            InitializeComponent();

            var bgKey = toast.Type == ToastType.Error ? ResourceKeys.ErrorBackground : ResourceKeys.InfoBackground;
            var fgKey = toast.Type == ToastType.Error ? ResourceKeys.ErrorForeground : ResourceKeys.InfoForeground;
            toastBorder.Background = (Brush)FindResource(bgKey);
            messageText.Foreground = (Brush)FindResource(fgKey);
            closeButton.Foreground = (Brush)FindResource(fgKey);

            messageText.Text = toast.Text;

            var durationKey = toast.Type == ToastType.Error ? ResourceKeys.ErrorDuration : ResourceKeys.InfoDuration;
            var duration = (TimeSpan)FindResource(durationKey);

            closeButton.Click += (_, __) => CloseToast();
            _timer = new DispatcherTimer { Interval = duration };
            _timer.Tick += (_, __) => CloseToast();

            this.Loaded += (_, __) => _timer.Start();
        }

        private void CloseToast()
        {
            _timer?.Stop();
            this.Close();
        }
    }
}
