using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace BookDL
{
    public static class BookSettingsDialogHelper
    {
        private const string BKSET_FILTER = "BookDL設定ファイル(*.bkset)|*.bkset|すべてのファイル(*.*)|*.*";
        private const string BKSET_EXT = ".bkset";
        public static SaveFileDialog CretateSaveFileDialog(string fileName, string outputDirectory)
        {
            if (!string.IsNullOrEmpty(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }
            return new SaveFileDialog
            {
                FileName = fileName,
                Filter = BKSET_FILTER,
                DefaultExt = BKSET_EXT,
                InitialDirectory = outputDirectory
            };
        }

        public static OpenFileDialog CreateOpenFileDialog(string initialDirectory)
        {
            if (!Directory.Exists(initialDirectory))
            {
                initialDirectory = Directory.GetCurrentDirectory();
            }
            return new OpenFileDialog
            {
                Filter = BKSET_FILTER,
                InitialDirectory = initialDirectory
            };
        }
    }
}
