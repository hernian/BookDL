using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using BookDL.ViewModels;

namespace BookDL.Presentation
{
    /// <summary>
    /// ConfigDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class ConfigDialog : Window
    {
        public ConfigDialog(ConfigViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
            viewModel.CloseDialogRequired += ViewModel_CloseDialogRequired;
        }

        private void ViewModel_CloseDialogRequired(object? sender, DialogResultEventArgs e)
        {
            this.DialogResult = e.DialogResult;
            this.Close();
        }
    }
}
