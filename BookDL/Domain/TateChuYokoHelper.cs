using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace BookDL.Domain
{
    public static class TateChuYokoHelper
    {
        private static readonly Regex TATE_CYU_YOKO = new(@"([a-zA-Z0-9][a-zA-Z0-9!?,.]*)|([!?]+)|(\.+)");

        public static ParagraphNode ConvertForParagraph(ParagraphNode srcPara)
        {
            var newNodeList = new List<IBookNode>();
            foreach (var node in srcPara.Nodes)
            {
                if (node is TextNode text)
                {
                    var newNodes = ConvertForText(text.TextContent);
                    newNodeList.AddRange(newNodes);
                }
                else
                {
                    newNodeList.Add(node);
                }
            }
            return new ParagraphNode(newNodeList);
        }

        public static IEnumerable<IBookNode> ConvertForText(string text)
        {
            var matches = TATE_CYU_YOKO.Matches(text);

            var sb = new StringBuilder();
            var lastIndex = 0;
            foreach (Match match in matches)
            {
                if (match.Index > lastIndex)
                {
                    sb.Append(text[lastIndex..match.Index]);
                    lastIndex = match.Index;
                }
                if (match.Groups[1].Success)    // 先頭英数字、次文字以降は英数字か '!', '?', ',', '.' の連続
                {
                    var group = match.Groups[1];
                    var groupLength = group.Length;
                    var groupValue = group.Value;
                    if (groupLength <= 3 && IsNumberWithDot(groupValue))
                    {
                        // 3桁以下の数字は1つの縦中横に組む
                        if (sb.Length > 0)
                        {
                            yield return new TextNode(sb.ToString());
                            sb.Clear();
                        }
                        yield return new TateChuYokoNode(groupValue);
                    }
                    else if (groupLength <= 2)
                    {
                        // 2文字以下の英数字!?,.は1つの縦中横に組む
                        if (sb.Length > 0)
                        {
                            yield return new TextNode(sb.ToString());
                            sb.Clear();
                        }
                        yield return new TateChuYokoNode(groupValue);
                    }
                    else
                    {
                        // 3文字以上の英数字!?,.は縦中横にしない
                        sb.Append(groupValue);
                    }
                }
                else if (match.Groups[2].Success)   // '!'、'?' の連続
                {
                    var group = match.Groups[2];
                    if (group.Length == 2)
                    {
                        if (sb.Length > 0)
                        {
                            yield return new TextNode(sb.ToString());
                            sb.Clear();
                        }
                        // 2文字連続の場合にだけ縦中横に組む
                        yield return new TateChuYokoNode(group.Value);
                    }
                    else
                    {
                        // 1文字または3文字以上の場合は全角文字に変換する。
                        // それだけで縦中横と同等の見た目になる。
                        foreach (char c in group.ValueSpan)
                        {
                            var newText = (c == '!') ? "！" : "？";   // ! か ? のみなので c != '!' なら ? となるはず
                            sb.Append(newText);
                        }
                    }
                }
                else if (match.Groups[3].Success)  // '.' の連続
                {
                    var group = match.Groups[3];
                    if (group.Length % 3 == 0)
                    {
                        // 三点リーダーへ変換する。
                        for (var i = 0; i < group.Length; i += 3)
                        {
                            sb.Append("…");
                        }
                    }
                    else
                    {
                        if (sb.Length > 0)
                        {
                            yield return new TextNode(sb.ToString());
                            sb.Clear();
                        }
                        // 1文字毎に縦中横とする
                        var endIndex = group.Index + group.Length;
                        for (var i = group.Index; i < endIndex; i++)
                        {
                            yield return new TateChuYokoNode(text[i].ToString());
                        }
                    }
                }
                lastIndex = match.Index + match.Length;
            }
            if (lastIndex < text.Length)
            {
                sb.Append(text.AsSpan().Slice(lastIndex));
            }
            if (sb.Length > 0)
            {
                yield return new TextNode(sb.ToString());
            }
        }
        private static bool IsNumberWithDot(string text)
        {
            foreach (var ch in text[0..^1])
            {
                if (!char.IsBetween(ch, '0', '9'))
                {
                    return false;
                }
            }
            var lastChar = text[^1];
            if (!char.IsBetween(lastChar, '0', '9') && lastChar != '.')
            {
                return false;
            }
            return true;
        }
    }
}
