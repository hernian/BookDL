using BookDL.Services;
using BookDL.ViewModels;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using BookDL.Infrastructure.Parser;

namespace BookDL.Presentation
{
    /// <summary>
    /// ProgressControl.xaml の相互作用ロジック
    /// </summary>
    public partial class ProgressControl : UserControl
    {
        private MainViewModel? _mainViewModel;
        public ProgressControl()
        {
            InitializeComponent();
            this.DataContextChanged += ProgressControl_DataContextChanged;
        }

        private void ProgressControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Debug.WriteLine("ProgressControl_DataContextChanged");
            if (_mainViewModel != null && _mainViewModel == e.OldValue)
            {
                Debug.WriteLine("_mainViewModel.PropertyChanged -= mainViewModel_PropertyChanged");
                _mainViewModel.PropertyChanged -= mainViewModel_PropertyChanged;
            }
            _mainViewModel = (MainViewModel)e.NewValue;
            if (_mainViewModel != null)
            {
                Debug.WriteLine("_mainViewModel.PropertyChanged += mainViewModel_PropertyChanged");
                _mainViewModel.PropertyChanged += mainViewModel_PropertyChanged;
            }
        }

        private void mainViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            Debug.WriteLine($"mainViewModel_PropertyChanged. Name: {e.PropertyName}");
            if (e.PropertyName == nameof(MainViewModel.DownloadReport))
            {
                SetDownloadReport(_mainViewModel!.DownloadReport);
            }
        }

        private void SetDownloadReport(DownloadReport downloadReport)
        {
            Debug.WriteLine($"SetDownloadReport. downloadReport: {downloadReport}");
            progressText.Text = $"{downloadReport.Current} / {downloadReport.Total}";
            var current = downloadReport.Current - downloadReport.Start;
            var total = downloadReport.Total - downloadReport.Start;
            progressBar.Value = total > 0 ? (double)current / (double)total : 0.0;
        }
    }
}
