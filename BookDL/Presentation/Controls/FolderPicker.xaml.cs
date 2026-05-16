using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace BookDL.Presentation.Controls
{
    /// <summary>
    /// FolderPicker.xaml の相互作用ロジック
    /// </summary>
    public partial class FolderPicker : UserControl
    {
        public static readonly DependencyProperty SelectedPathProperty =
            DependencyProperty.Register(
                nameof(SelectedPath),
                typeof(string),
                typeof(FolderPicker),
                new FrameworkPropertyMetadata(
                    string.Empty,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty DialogTitleProperty =
            DependencyProperty.Register(
                nameof(DialogTitle),
                typeof(string),
                typeof(FolderPicker),
                new PropertyMetadata("フォルダーを選択"));

        public string SelectedPath
        {
            get => (string)GetValue(SelectedPathProperty);
            set => SetValue(SelectedPathProperty, value);
        }

        public string DialogTitle
        {
            get => (string) GetValue(DialogTitleProperty);
            set => SetValue(DialogTitleProperty, value);
        }

        public FolderPicker()
        {
            InitializeComponent();
        }

        private void OnBrowseClick(object? sender, RoutedEventArgs e)
        {
            var dlg = new OpenFolderDialog()
            {
                InitialDirectory = Directory.Exists(this.SelectedPath) ? this.SelectedPath : null,
                Title = "フォルダーを選択"
            };
            if (dlg.ShowDialog(Window.GetWindow(this)) == true)
            {
                this.SelectedPath = dlg.FolderName;
            }
        }
    }
}
