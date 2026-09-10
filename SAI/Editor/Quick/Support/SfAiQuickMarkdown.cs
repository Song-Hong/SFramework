using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;

namespace SFramework.SAI.Editor.Quick.Support
{
    /// <summary>
    /// Markdown 渲染：富文本 + 表格 UI；emoji 转为可读替代或移除（Unity 字体常无法显示）。
    /// </summary>
    public static class SfAiQuickMarkdown
    {
        static readonly Regex CodeBlock = new Regex(
            @"```([\w+-]*)\n?([\s\S]*?)```",
            RegexOptions.Compiled);

        static readonly Regex InlineCode = new Regex(
            @"`([^`\n]+)`",
            RegexOptions.Compiled);

        static readonly Regex Bold = new Regex(
            @"\*\*(.+?)\*\*|__(.+?)__",
            RegexOptions.Compiled | RegexOptions.Singleline);

        static readonly Regex Italic = new Regex(
            @"(?<!\*)\*(?!\*)([^*\n]+?)(?<!\*)\*(?!\*)|(?<!_)_([^_\n]+?)_(?!_)",
            RegexOptions.Compiled);

        static readonly Regex Strike = new Regex(
            @"~~(.+?)~~",
            RegexOptions.Compiled);

        static readonly Regex Heading = new Regex(
            @"^(#{1,3})\s+(.+)$",
            RegexOptions.Compiled | RegexOptions.Multiline);

        static readonly Regex TaskItem = new Regex(
            @"^[\-\*\+]\s+\[([ xX])\]\s+(.+)$",
            RegexOptions.Compiled | RegexOptions.Multiline);

        static readonly Regex Unordered = new Regex(
            @"^[\-\*\+]\s+(.+)$",
            RegexOptions.Compiled | RegexOptions.Multiline);

        static readonly Regex Ordered = new Regex(
            @"^\d+\.\s+(.+)$",
            RegexOptions.Compiled | RegexOptions.Multiline);

        static readonly Regex Link = new Regex(
            @"\[([^\]]+)\]\(([^)]+)\)",
            RegexOptions.Compiled);

        static readonly Regex TableRow = new Regex(
            @"^\s*\|(.+)\|\s*$",
            RegexOptions.Compiled);

        static readonly Regex TableSep = new Regex(
            @"^\s*\|?\s*:?-{3,}:?\s*(\|\s*:?-{3,}:?\s*)+\|?\s*$",
            RegexOptions.Compiled);

        static readonly Dictionary<string, string> EmojiMap = new Dictionary<string, string>
        {
            { "😀", ":)" }, { "😁", ":D" }, { "😂", "哈哈" }, { "🤣", "哈哈" },
            { "😊", ":)" }, { "😍", "<3" }, { "😎", "酷" }, { "🤔", "?" },
            { "👍", "[赞]" }, { "👎", "[踩]" }, { "✅", "[OK]" }, { "❌", "[X]" },
            { "⚠️", "[!]" }, { "💡", "[提示]" }, { "🔥", "[热]" }, { "✨", "*" },
            { "🚀", "[启动]" }, { "🎯", "[目标]" }, { "📌", "[钉]" }, { "📝", "[笔记]" },
            { "🔧", "[工具]" }, { "🛠️", "[工具]" }, { "⚙️", "[设置]" }, { "🎉", "[完成]" },
            { "➡️", "->" }, { "⬅️", "<-" }, { "⬆️", "^" }, { "⬇️", "v" },
            { "•", "•" }, { "·", "·" },
        };

        /// <summary>把 Markdown 填入容器（表格用真实格子，其它用 RichText）</summary>
        public static void BuildInto(VisualElement parent, string markdown)
        {
            if (parent == null) return;
            parent.Clear();

            if (string.IsNullOrWhiteSpace(markdown))
                return;

            var text = Normalize(markdown);
            var lines = text.Split('\n');
            var i = 0;
            var para = new StringBuilder();

            void FlushPara()
            {
                if (para.Length == 0) return;
                var label = CreateRichLabel(ToRichText(para.ToString().TrimEnd()));
                parent.Add(label);
                para.Clear();
            }

            while (i < lines.Length)
            {
                // 代码块
                if (lines[i].TrimStart().StartsWith("```", StringComparison.Ordinal))
                {
                    FlushPara();
                    var lang = lines[i].TrimStart().Substring(3).Trim();
                    i++;
                    var code = new StringBuilder();
                    while (i < lines.Length && !lines[i].TrimStart().StartsWith("```", StringComparison.Ordinal))
                    {
                        code.AppendLine(lines[i]);
                        i++;
                    }

                    if (i < lines.Length) i++; // closing ```
                    parent.Add(CreateCodeBlock(lang, code.ToString().TrimEnd('\r', '\n')));
                    continue;
                }

                // 表格：表头 + 分隔行 + 数据行
                if (IsTableHeader(lines, i))
                {
                    FlushPara();
                    var tableLines = new List<string>();
                    while (i < lines.Length && (TableRow.IsMatch(lines[i]) || TableSep.IsMatch(lines[i])))
                    {
                        tableLines.Add(lines[i]);
                        i++;
                    }

                    var table = BuildTable(tableLines);
                    if (table != null)
                        parent.Add(table);
                    continue;
                }

                para.AppendLine(lines[i]);
                i++;
            }

            FlushPara();
        }

        public static string ToRichText(string markdown)
        {
            if (string.IsNullOrEmpty(markdown)) return "";

            var text = Normalize(markdown);

            var blocks = new List<string>();
            text = CodeBlock.Replace(text, m =>
            {
                var idx = blocks.Count;
                var lang = m.Groups[1].Value.Trim();
                var code = EscapeRich(m.Groups[2].Value.TrimEnd('\n'));
                var header = string.IsNullOrEmpty(lang) ? "" : $"<color=#888888>{EscapeRich(lang)}</color>\n";
                blocks.Add($"{header}<color=#A8D8A8>{code}</color>");
                return Marker(idx);
            });

            text = EscapeRich(text);

            text = InlineCode.Replace(text, m =>
                $"<color=#9CDCFE>{m.Groups[1].Value}</color>");

            text = Link.Replace(text, m =>
                $"<color=#6CB6FF><u>{m.Groups[1].Value}</u></color>");

            text = Bold.Replace(text, m =>
            {
                var inner = m.Groups[1].Success ? m.Groups[1].Value : m.Groups[2].Value;
                return $"<b>{inner}</b>";
            });

            text = Strike.Replace(text, m => $"<s>{m.Groups[1].Value}</s>");

            text = Italic.Replace(text, m =>
            {
                var inner = m.Groups[1].Success ? m.Groups[1].Value : m.Groups[2].Value;
                return $"<i>{inner}</i>";
            });

            text = Heading.Replace(text, m =>
            {
                var level = m.Groups[1].Value.Length;
                var size = level == 1 ? 16 : level == 2 ? 14 : 13;
                return $"\n<size={size}><b>{m.Groups[2].Value}</b></size>\n";
            });

            text = TaskItem.Replace(text, m =>
            {
                var done = m.Groups[1].Value == "x" || m.Groups[1].Value == "X";
                var mark = done ? "<color=#7DCEA0>[x]</color>" : "<color=#888888>[ ]</color>";
                return $"{mark}  {m.Groups[2].Value}";
            });

            text = Unordered.Replace(text, m => $"•  {m.Groups[1].Value}");
            text = Ordered.Replace(text, m => $"•  {m.Groups[1].Value}");

            for (var i = 0; i < blocks.Count; i++)
                text = text.Replace(Marker(i), "\n" + blocks[i] + "\n");

            text = Regex.Replace(text, @"\n{3,}", "\n\n");
            return text.Trim();
        }

        static string Normalize(string markdown)
        {
            var text = markdown.Replace("\r\n", "\n").Replace("\r", "\n");
            text = ReplaceEmoji(text);
            return text;
        }

        /// <summary>Unity 默认字体常无法画 emoji，转为 ASCII/中文替代，避免方框乱码</summary>
        public static string ReplaceEmoji(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";

            foreach (var pair in EmojiMap)
                text = text.Replace(pair.Key, pair.Value);

            var sb = new StringBuilder(text.Length);
            var enumr = StringInfo.GetTextElementEnumerator(text);
            while (enumr.MoveNext())
            {
                var el = enumr.GetTextElement();
                if (IsEmojiElement(el))
                    continue; // 去掉无法映射的 emoji，避免 □
                sb.Append(el);
            }

            return sb.ToString();
        }

        static bool IsEmojiElement(string el)
        {
            if (string.IsNullOrEmpty(el)) return false;
            var it = StringInfo.GetTextElementEnumerator(el);
            it.MoveNext();
            // 取首码点
            int cp;
            if (el.Length >= 2 && char.IsSurrogatePair(el, 0))
                cp = char.ConvertToUtf32(el, 0);
            else
                cp = el[0];

            return
                (cp >= 0x1F300 && cp <= 0x1FAFF) || // Emoticons & Symbols
                (cp >= 0x2600 && cp <= 0x27BF) ||   // Misc symbols
                (cp >= 0xFE00 && cp <= 0xFE0F) ||   // Variation selectors
                (cp >= 0x1F1E6 && cp <= 0x1F1FF) || // Flags
                cp == 0x200D ||                     // ZWJ
                cp == 0x20E3;                       // keycap
        }

        static bool IsTableHeader(string[] lines, int i)
        {
            if (i + 1 >= lines.Length) return false;
            return TableRow.IsMatch(lines[i]) && TableSep.IsMatch(lines[i + 1]);
        }

        static VisualElement BuildTable(List<string> tableLines)
        {
            if (tableLines == null || tableLines.Count < 2) return null;

            var headers = SplitRow(tableLines[0]);
            if (headers.Count == 0) return null;

            var root = new VisualElement();
            root.AddToClassList("sfai-md-table");

            var headerRow = new VisualElement();
            headerRow.AddToClassList("sfai-md-table-row");
            headerRow.AddToClassList("sfai-md-table-header");
            foreach (var h in headers)
            {
                var cell = CreateRichLabel(ToRichText(h.Trim()));
                cell.AddToClassList("sfai-md-table-cell");
                cell.AddToClassList("sfai-md-table-cell-header");
                headerRow.Add(cell);
            }

            root.Add(headerRow);

            for (var r = 2; r < tableLines.Count; r++)
            {
                if (TableSep.IsMatch(tableLines[r])) continue;
                var cols = SplitRow(tableLines[r]);
                var row = new VisualElement();
                row.AddToClassList("sfai-md-table-row");

                for (var c = 0; c < headers.Count; c++)
                {
                    var raw = c < cols.Count ? cols[c].Trim() : "";
                    var cell = CreateRichLabel(ToRichText(raw));
                    cell.AddToClassList("sfai-md-table-cell");
                    row.Add(cell);
                }

                root.Add(row);
            }

            return root;
        }

        static List<string> SplitRow(string line)
        {
            var list = new List<string>();
            var t = line.Trim();
            if (t.StartsWith("|", StringComparison.Ordinal)) t = t.Substring(1);
            if (t.EndsWith("|", StringComparison.Ordinal)) t = t.Substring(0, t.Length - 1);
            foreach (var part in t.Split('|'))
                list.Add(part.Trim());
            return list;
        }

        static VisualElement CreateCodeBlock(string lang, string code)
        {
            var box = new VisualElement();
            box.AddToClassList("sfai-md-code");
            if (!string.IsNullOrEmpty(lang))
            {
                var langLabel = new Label(lang);
                langLabel.AddToClassList("sfai-md-code-lang");
                box.Add(langLabel);
            }

            var codeLabel = CreateRichLabel($"<color=#A8D8A8>{EscapeRich(code)}</color>");
            codeLabel.AddToClassList("sfai-md-code-body");
            box.Add(codeLabel);
            return box;
        }

        static Label CreateRichLabel(string rich)
        {
            var label = new Label(rich ?? "");
            label.enableRichText = true;
            label.AddToClassList("sfai-quick-body");
            label.style.whiteSpace = WhiteSpace.Normal;
            return label;
        }

        static string Marker(int idx) => $"\uE000{idx}\uE001";

        static string EscapeRich(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            var sb = new StringBuilder(s.Length);
            foreach (var c in s)
            {
                switch (c)
                {
                    case '<': sb.Append("&lt;"); break;
                    case '>': sb.Append("&gt;"); break;
                    case '&': sb.Append("&amp;"); break;
                    default: sb.Append(c); break;
                }
            }

            return sb.ToString();
        }
    }
}
