using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BookDL.Presentation
{
    public enum DropKind
    {
        Unknown,
        Url,
        Directory
    }
    public sealed record DropResult(DropKind Kind, string[] Values);

    public static class DropDetector
    {
        // 許可するURIスキーム（必要に応じて拡張）
        private static readonly string[] AllowedSchemes =
            { "http", "https" };

        /// <summary>
        /// IDataObject を検査して種別と値を返す。
        /// 優先順位: Directory > UniformResourceLocatorW > UniformResourceLocator > Text(URI)
        /// </summary>
        public static DropResult Detect(IDataObject data)
        {
            // --- 1. FileDrop: ディレクトリ判定 ---
            // FileDrop はファイルもディレクトリも含みうるため Directory.Exists で厳密に絞る
            if (data.GetDataPresent(DataFormats.FileDrop))
            {
                if (data.GetData(DataFormats.FileDrop) is string[] paths)
                {
                    var dirs = paths.Where(Directory.Exists).ToArray();
                    if (dirs.Length > 0 && dirs.Length == paths.Length)
                        // 全アイテムがディレクトリの場合のみ Directory と判定
                        // ファイルが混在する場合は Unknown（方針次第で変更可）
                        return new DropResult(DropKind.Directory, dirs);
                }
            }

            // --- 2. UniformResourceLocatorW (Unicode, ブラウザ標準) ---
            var urlW = ReadUrlFormat(data, "UniformResourceLocatorW", Encoding.Unicode);
            if (urlW is not null)
                return new DropResult(DropKind.Url, [urlW]);

            // --- 3. UniformResourceLocator (ANSI, 旧式) ---
            var urlA = ReadUrlFormat(data, "UniformResourceLocator", Encoding.Default);
            if (urlA is not null)
                return new DropResult(DropKind.Url, [urlA]);

            // --- 4. プレーンテキスト（URI として解析できる場合のみ）---
            // ここは偽陽性のリスクが最も高いため最後に置く
            if (data.GetDataPresent(DataFormats.Text) &&
                data.GetData(DataFormats.Text) is string text)
            {
                var trimmed = text.Trim();
                if (IsStrictUri(trimmed))
                    return new DropResult(DropKind.Url, [trimmed]);
            }

            return new DropResult(DropKind.Unknown, []);
        }

        // UniformResourceLocator* フォーマットは MemoryStream で渡される
        private static string? ReadUrlFormat(IDataObject data, string format, Encoding enc)
        {
            if (!data.GetDataPresent(format)) return null;

            string raw;
            if (data.GetData(format) is MemoryStream ms)
            {
                raw = enc.GetString(ms.ToArray());
            }
            else if (data.GetData(format) is string s)
            {
                raw = s;
            }
            else return null;

            // null終端・改行除去
            var url = raw.TrimEnd('\0', '\r', '\n').Trim();
            return IsStrictUri(url) ? url : null;
        }

        /// <summary>
        /// 絶対URI かつ許可スキームであることを確認する。
        /// Uri.TryCreate は空文字や相対パスも通過させうるため二重チェックが必要。
        /// </summary>
        public static bool IsStrictUri(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;

            return Uri.TryCreate(value, UriKind.Absolute, out var uri)
                && uri.IsAbsoluteUri
                && AllowedSchemes.Contains(uri.Scheme, StringComparer.OrdinalIgnoreCase);
        }

    }


    /// <summary>
    /// SettingsControl.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingsControl : UserControl
    {
        public SettingsControl()
        {
            InitializeComponent();
        }
        private void OnDragEnter(object sender, DragEventArgs e)
        {
            var result = DropDetector.Detect(e.Data);
            UpdateVisualFeedback(result.Kind);
            e.Effects = result.Kind != DropKind.Unknown
                ? DragDropEffects.Copy
                : DragDropEffects.None;
            e.Handled = true;
        }

        private void OnDragOver(object sender, DragEventArgs e)
        {
            // DragEnter と同じ判定が必要（Windowをまたぐ操作で再評価される）
            var result = DropDetector.Detect(e.Data);
            e.Effects = result.Kind != DropKind.Unknown
                ? DragDropEffects.Copy
                : DragDropEffects.None;
            e.Handled = true;
        }

        private void OnDragLeave(object sender, DragEventArgs e)
        {
            ResetVisualFeedback();
        }

        private void OnDrop(object sender, DragEventArgs e)
        {
            ResetVisualFeedback();

            var result = DropDetector.Detect(e.Data);
            var first = result.Values.FirstOrDefault();
            switch (result.Kind)
            {
                case DropKind.Url when first is not null:
                    bookUrl.Text = first;
                    title.Text = string.Empty;
                    titleKatakana.Text = string.Empty;
                    author.Text = string.Empty;
                    authorKatakana.Text = string.Empty;
                    outputDirectory.SelectedPath = string.Empty;
                    break;

                case DropKind.Directory when first is not null:
                    outputDirectory.SelectedPath = first;
                    break;
                default:
                    break;
            }

            e.Handled = true;
        }
        private void UpdateVisualFeedback(DropKind kind)
        {
            dropBorder.BorderBrush = kind switch
            {
                DropKind.Url => Brushes.DodgerBlue,
                DropKind.Directory => Brushes.SeaGreen,
                _ => Brushes.Red,
            };
        }

        private void ResetVisualFeedback()
        {
            dropBorder.BorderBrush = Brushes.Transparent;
        }
    }
}
