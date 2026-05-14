using System.IO;
using System.Text;

namespace BookDL.Infrastructure.Generator
{
    public static class PathHelper
    {
        private const int MAX_SEGMENT_LENGTH = 64;
        private const int MAX_PATH_LENGTH = 1024;

        private static readonly char[] Sepalators = [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar];

        // Windows ファイル名として不正な文字セット
        private static readonly HashSet<char> InvalidWindowsChars =
            new HashSet<char>(Path.GetInvalidPathChars());

        private static readonly Encoding ShiftJis;

        static PathHelper()
        {
            // .NET Core / .NET 5+ では必須
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            ShiftJis = Encoding.GetEncoding(
                932,
                new EncoderExceptionFallback(),   // エンコード失敗 → 例外
                new DecoderExceptionFallback()
            );
        }

        public static string SanitizeForWindowsFileName(string filename)
        {
            // …で省略する可能性があるのでファイル名の部分から拡張子を取り除く
            var basename = Path.GetFileNameWithoutExtension(filename);
            var ext = Path.GetExtension(filename);
            var sanitizedFileName = SanitizeForWindowsFileNameSegment(basename) + ext;
            return sanitizedFileName;
        }

        public static string SanitizeForWindowsPathName(string path)
        {
            var segments = path.Trim().Split(Sepalators, StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length == 0)
            {
                throw new InvalidOperationException($"Empty path: {path}");
            }
            // …で省略する可能性があるのでファイル名の部分から拡張子を取り除く
            var baseName = Path.GetFileNameWithoutExtension(segments[^1]);
            var ext = Path.GetExtension(segments[^1]);
            segments[^1] = baseName;

            var listSegment = new List<string>();
            foreach (var segment in segments)
            {
                var sanitizedSegment = SanitizeForWindowsFileNameSegment(segment);
                listSegment.Add(sanitizedSegment);
            }
            var sanitizedPath = string.Join(Path.DirectorySeparatorChar, listSegment) + ext;
            if (sanitizedPath.Length >= MAX_PATH_LENGTH)
            {
                throw new InvalidOperationException($"Too long path. Path: {path}");
            }
            return sanitizedPath;
        }

        public static string SanitizeForWindowsFileNameSegment(string input)
        {
            if (string.IsNullOrEmpty(input)) return input ?? string.Empty;

            var sb = new StringBuilder(input.Length);

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];

                // ① サロゲートペアの処理（Shift_JIS は BMP のみ対応）
                if (char.IsHighSurrogate(c))
                {
                    if (i + 1 < input.Length && char.IsLowSurrogate(input[i + 1]))
                        i++; // ローサロゲートもスキップ
                             // ペア・孤立ともに除外
                    continue;
                }
                if (char.IsLowSurrogate(c)) // 孤立ローサロゲート
                    continue;

                // ② Windows ファイル名不正文字を除外
                // 例: \ / : * ? " < > | および制御文字 (U+0000〜U+001F)
                if (InvalidWindowsChars.Contains(c))
                    continue;

                // ③ Shift_JIS でエンコード不可な文字を除外
                if (!IsShiftJisEncodable(c))
                    continue;

                sb.Append(c);
                if (sb.Length >= MAX_SEGMENT_LENGTH)
                {
                    sb.Append("…");
                    break;
                }
            }

            return sb.ToString();
        }

        private static bool IsShiftJisEncodable(char c)
        {
            try
            {
                // EncoderExceptionFallback 付きなのでエンコード不可なら例外が飛ぶ
                ShiftJis.GetByteCount(new[] { c });
                return true;
            }
            catch (EncoderFallbackException)
            {
                return false;
            }
        }
    }
}
