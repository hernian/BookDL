using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace BookDL
{
    public class ZipBookWriter : IDisposable
    {
        private const int MAX_SECTION_TITLE_LENGTH = 64;
        private static readonly HashSet<char> INVALID_FILENAME_CHARS = new HashSet<char>(Path.GetInvalidFileNameChars());
        private static readonly Encoding ENCODING = new UTF8Encoding(false);

        private readonly FileStream _fileStream;
        private readonly ZipArchive _zifArchive;
        private int _sectionIndex = 1;
        private bool disposedValue;

        public ZipBookWriter(string zipFilePath)
        {
            _fileStream = new FileStream(zipFilePath, FileMode.Create, FileAccess.Write);
            try
            {
                _zifArchive = new ZipArchive(
                            _fileStream,
                            ZipArchiveMode.Create,
                            leaveOpen: false,
                            entryNameEncoding: ENCODING);
            }
            catch
            {
                _fileStream.Dispose();
                throw;
            }
        }

        private string GetSectionTitle(ISectionSource section)
        {
            var title = section.Title;
            var sb = new StringBuilder();
            foreach (var c in title)
            {
                if (sb.Length >= MAX_SECTION_TITLE_LENGTH)
                {
                    sb.Append("…");
                    break;
                }
                if (INVALID_FILENAME_CHARS.Contains(c))
                {
                    sb.Append('_');
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        public void AddSection(ISectionSource section)
        {
            var safeTitle = GetSectionTitle(section);
            var sectionFileName = $"section{_sectionIndex:D5}:{safeTitle}.txt";
            _sectionIndex++;
            var entry = _zifArchive.CreateEntry(sectionFileName);
            using var entryStream = entry.Open();
            using var writer = new StreamWriter(entryStream, ENCODING);
            foreach (var paragraph in section.Paragraphs)
            {
                writer.WriteLine(paragraph);
            }
        }

        public TextWriter CreateSectionWriter(string sectionFileName)
        {
            var entry = _zifArchive.CreateEntry(sectionFileName);
            var entryStream = entry.Open();
            try
            {
                var writer = new StreamWriter(entryStream, ENCODING);
                return writer;
            }
            catch
            {
                entryStream.Dispose();
                throw;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _zifArchive.Dispose();
                }

                // TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
                // TODO: 大きなフィールドを null に設定します
                disposedValue = true;
            }
        }

        // // TODO: 'Dispose(bool disposing)' にアンマネージド リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします
        // ~ZipBookWriter()
        // {
        //     // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
        //     Dispose(disposing: false);
        // }

        void IDisposable.Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
