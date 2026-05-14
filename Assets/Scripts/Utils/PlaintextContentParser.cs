using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

// plaintext 格式公告内容解析器
// 将 Content 字符串按 Markdown 子集规则解析为 PlaintextBlock 列表，供 UI 层逐块渲染
public static class PlaintextContentParser
{
    // ── 块级正则 ────────────────────────────────────────────────────────────
    private static readonly Regex HeadingPattern =
        new Regex(@"^(#{1,6})\s+(.+)$", RegexOptions.Compiled);

    private static readonly Regex HorizontalRulePattern =
        new Regex(@"^([-*_])\1{2,}\s*$", RegexOptions.Compiled);

    private static readonly Regex UnorderedListPattern =
        new Regex(@"^[-*+]\s+(.+)$", RegexOptions.Compiled);

    private static readonly Regex OrderedListPattern =
        new Regex(@"^\d+\.\s+(.+)$", RegexOptions.Compiled);

    private static readonly Regex FenceOpenPattern =
        new Regex(@"^```(\w*)\s*$", RegexOptions.Compiled);

    private static readonly Regex FenceClosePattern =
        new Regex(@"^```\s*$", RegexOptions.Compiled);

    private static readonly Regex BlockquotePattern =
        new Regex(@"^>\s*(.*)$", RegexOptions.Compiled);

    private static readonly Regex ImagePattern =
        new Regex(@"^!\[([^\]]*)\]\(([^\)]+)\)\s*$", RegexOptions.Compiled);

    private static readonly Regex TableRowPattern =
        new Regex(@"^\|.*\|\s*$", RegexOptions.Compiled);

    private static readonly Regex SimplePipeRowPattern =
        new Regex(@"^[^\|]+(\|[^\|]+)+\s*$", RegexOptions.Compiled);

    private static readonly Regex TableSeparatorPattern =
        new Regex(@"^[\|\s:\-]+$", RegexOptions.Compiled);

    // ── 行内正则（按优先级顺序处理）────────────────────────────────────────
    private static readonly Regex BoldPattern =
        new Regex(@"\*\*(.+?)\*\*", RegexOptions.Compiled);

    private static readonly Regex ItalicPattern =
        new Regex(@"\*(.+?)\*", RegexOptions.Compiled);

    private static readonly Regex InlineCodePattern =
        new Regex(@"`(.+?)`", RegexOptions.Compiled);

    // ────────────────────────────────────────────────────────────────────────

    public static List<PlaintextBlock> Parse(string content)
    {
        var blocks = new List<PlaintextBlock>();
        if (string.IsNullOrEmpty(content))
            return blocks;

        var lines = content.Split('\n');

        var textLines   = new List<string>();
        var tableLines  = new List<string>();
        var listItems   = new List<string>();
        var bqLines     = new List<string>();
        var codeLines   = new List<string>();
        bool inTable    = false;
        bool inCode     = false;
        bool listOrdered = false;
        string codeLang = "";

        void FlushText()
        {
            if (textLines.Count == 0) return;
            blocks.Add(new TextBlock { Text = ApplyInlineMarkdown(string.Join("\n", textLines)) });
            textLines.Clear();
        }

        void FlushTable()
        {
            if (tableLines.Count == 0) return;
            var table = new TableBlock
            {
                Headers = ParseRow(tableLines[0]),
                Rows    = new List<List<string>>()
            };
            for (int i = 1; i < tableLines.Count; i++)
                table.Rows.Add(ParseRow(tableLines[i]));
            blocks.Add(table);
            tableLines.Clear();
        }

        void FlushList()
        {
            if (listItems.Count == 0) return;
            var items = new List<string>();
            foreach (var item in listItems)
                items.Add(ApplyInlineMarkdown(item));
            blocks.Add(new ListBlock { Ordered = listOrdered, Items = items });
            listItems.Clear();
        }

        void FlushBlockquote()
        {
            if (bqLines.Count == 0) return;
            blocks.Add(new BlockquoteBlock { Text = ApplyInlineMarkdown(string.Join("\n", bqLines)) });
            bqLines.Clear();
        }

        void FlushAll()
        {
            FlushText();
            FlushList();
            if (tableLines.Count > 0) { FlushTable(); inTable = false; }
            FlushBlockquote();
        }

        foreach (var raw in lines)
        {
            var line = raw.TrimEnd('\r');

            // ── Fenced code block（有状态，最先处理）─────────────────────
            if (inCode)
            {
                if (FenceClosePattern.IsMatch(line))
                {
                    blocks.Add(new CodeBlock { Language = codeLang, Code = string.Join("\n", codeLines) });
                    codeLines.Clear(); codeLang = ""; inCode = false;
                }
                else
                {
                    codeLines.Add(line);
                }
                continue;
            }

            // ── Fence open ───────────────────────────────────────────────
            var fenceMatch = FenceOpenPattern.Match(line);
            if (fenceMatch.Success)
            {
                FlushAll();
                inCode = true;
                codeLang = fenceMatch.Groups[1].Value;
                continue;
            }

            // ── 空行：结束所有累积块 ──────────────────────────────────────
            if (string.IsNullOrWhiteSpace(line))
            {
                FlushAll();
                inTable = false;
                continue;
            }

            // ── 标题 ─────────────────────────────────────────────────────
            var headingMatch = HeadingPattern.Match(line);
            if (headingMatch.Success)
            {
                FlushAll();
                int level = headingMatch.Groups[1].Value.Length;
                string text = ApplyInlineMarkdown(headingMatch.Groups[2].Value.Trim());
                blocks.Add(new HeadingBlock { Level = level, Text = text });
                continue;
            }

            // ── 水平分隔线（在无序列表之前判断，避免 --- 被误识别为列表）──
            if (HorizontalRulePattern.IsMatch(line))
            {
                FlushAll();
                blocks.Add(new HorizontalRuleBlock());
                continue;
            }

            // ── 独立图片行 ───────────────────────────────────────────────
            var imageMatch = ImagePattern.Match(line);
            if (imageMatch.Success)
            {
                FlushAll();
                blocks.Add(new ImageBlock
                {
                    Alt = imageMatch.Groups[1].Value,
                    Url = imageMatch.Groups[2].Value
                });
                continue;
            }

            // ── 引用块 ───────────────────────────────────────────────────
            var bqMatch = BlockquotePattern.Match(line);
            if (bqMatch.Success)
            {
                FlushText();
                FlushList();
                if (tableLines.Count > 0) { FlushTable(); inTable = false; }
                bqLines.Add(bqMatch.Groups[1].Value);
                continue;
            }
            else if (bqLines.Count > 0)
            {
                FlushBlockquote();
            }

            // ── 无序列表 ─────────────────────────────────────────────────
            var ulMatch = UnorderedListPattern.Match(line);
            if (ulMatch.Success)
            {
                FlushText();
                if (tableLines.Count > 0) { FlushTable(); inTable = false; }
                if (listItems.Count > 0 && listOrdered) FlushList();
                listOrdered = false;
                listItems.Add(ulMatch.Groups[1].Value);
                continue;
            }

            // ── 有序列表 ─────────────────────────────────────────────────
            var olMatch = OrderedListPattern.Match(line);
            if (olMatch.Success)
            {
                FlushText();
                if (tableLines.Count > 0) { FlushTable(); inTable = false; }
                if (listItems.Count > 0 && !listOrdered) FlushList();
                listOrdered = true;
                listItems.Add(olMatch.Groups[1].Value);
                continue;
            }

            // 非列表行到来时结束列表
            if (listItems.Count > 0) FlushList();

            // ── GFM 表格行 ───────────────────────────────────────────────
            if (TableRowPattern.IsMatch(line))
            {
                if (!inTable)
                {
                    FlushText();
                    FlushBlockquote();
                    inTable = true;
                }
                if (!TableSeparatorPattern.IsMatch(line))
                    tableLines.Add(line);
                continue;
            }

            // ── 简单管道表格行 ───────────────────────────────────────────
            if (SimplePipeRowPattern.IsMatch(line))
            {
                if (!inTable)
                {
                    FlushText();
                    FlushBlockquote();
                    inTable = true;
                }
                tableLines.Add(line.Trim());
                continue;
            }

            // 非表格行结束表格
            if (inTable) { FlushTable(); inTable = false; }

            // ── 普通文本段落 ─────────────────────────────────────────────
            textLines.Add(line);
        }

        // 末尾兜底：将未关闭的 code block 强制收尾
        if (inCode)
            blocks.Add(new CodeBlock { Language = codeLang, Code = string.Join("\n", codeLines) });

        FlushAll();
        return blocks;
    }

    // 将行内 Markdown 标记转换为 Unity 富文本标签
    // 顺序：bold（**）→ italic（*）→ inline code（`）
    private static string ApplyInlineMarkdown(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        text = BoldPattern.Replace(text, m => $"<b>{m.Groups[1].Value}</b>");
        text = ItalicPattern.Replace(text, m => $"<i>{m.Groups[1].Value}</i>");
        text = InlineCodePattern.Replace(text, m => $"<color=#afd7ff>{m.Groups[1].Value}</color>");
        return text;
    }

    private static List<string> ParseRow(string line)
    {
        var trimmed = line.Trim();
        var parts   = trimmed.Split('|');
        var cells   = new List<string>();
        bool isGfm  = trimmed.StartsWith("|");
        int  start  = isGfm ? 1 : 0;
        int  end    = isGfm ? parts.Length - 1 : parts.Length;
        for (int i = start; i < end; i++)
            cells.Add(parts[i].Trim());
        return cells;
    }
}
