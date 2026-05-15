using BookDL.Infrastructure;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace BookDL.Presentation
{
    /// <summary>
    /// Toast.xaml の相互作用ロジック
    /// </summary>
    public partial class Toast : Window
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
        public Toast(MessageType type, string message)
        {
            InitializeComponent();

            var bgKey = type == MessageType.Error ? ResourceKeys.ErrorBackground : ResourceKeys.InfoBackground;
            var fgKey = type == MessageType.Error ? ResourceKeys.ErrorForeground : ResourceKeys.InfoForeground;
            ToastBorder.Background = (Brush)FindResource(bgKey);
            MessageText.Foreground = (Brush)FindResource(fgKey);
            CloseButton.Foreground = (Brush)FindResource(fgKey);

            MessageText.Text = message;

            var durationKey = type == MessageType.Error ? ResourceKeys.ErrorDuration : ResourceKeys.InfoDuration;
            var duration = (TimeSpan)FindResource(durationKey);

            CloseButton.Click += (_, __) => CloseToast();
            _timer = new DispatcherTimer { Interval = duration };
            _timer.Tick += (_, __) => CloseToast();
            _timer.Start();
        }

        private void CloseToast()
        {
            _timer?.Stop();
            this.Close();
        }
    }
}
