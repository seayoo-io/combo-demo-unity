using System.Text.RegularExpressions;

// 将简单 HTML 转换为 Unity UI Text 支持的富文本格式
// 支持标签：<b>/<strong>, <i>/<em>, <br>, <p>, <ul>/<ol>/<li>, <h1>~<h3>
// 不支持的标签一律剥除，HTML 实体还原为对应字符
public static class HtmlToUnityRichText
{
    // 将 HTML 字符串转换为 Unity 富文本字符串
    public static string Convert(string html)
    {
        if (string.IsNullOrEmpty(html)) return html;

        // 用 Unicode 占位符保护 Unity 富文本标签，避免后续 Strip 步骤误删
        var result = html;
        result = result.Replace("<b>", "〈b〉").Replace("</b>", "〈/b〉");
        result = result.Replace("<i>", "〈i〉").Replace("</i>", "〈/i〉");

        // 将 HTML 语义标签映射到占位符
        result = Regex.Replace(result, @"<strong\b[^>]*>", "〈b〉", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"</strong>", "〈/b〉", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"<em\b[^>]*>", "〈i〉", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"</em>", "〈/i〉", RegexOptions.IgnoreCase);

        // 标题标签映射到 size 占位符
        result = Regex.Replace(result, @"<h1\b[^>]*>", "〈size=24〉", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"</h1>", "〈/size〉\n", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"<h2\b[^>]*>", "〈size=20〉", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"</h2>", "〈/size〉\n", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"<h3\b[^>]*>", "〈size=18〉", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"</h3>", "〈/size〉\n", RegexOptions.IgnoreCase);

        // 块级标签转换为换行
        result = Regex.Replace(result, @"<br\s*/?>", "\n", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"<p\b[^>]*>", "", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"</p>", "\n", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"<ul\b[^>]*>|</ul>|<ol\b[^>]*>|</ol>", "", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"<li\b[^>]*>", "• ", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"</li>", "\n", RegexOptions.IgnoreCase);

        // 剥除所有剩余 HTML 标签
        result = Regex.Replace(result, @"<[^>]+>", "");

        // 将占位符还原为 Unity 富文本标签
        result = result.Replace("〈b〉", "<b>").Replace("〈/b〉", "</b>");
        result = result.Replace("〈i〉", "<i>").Replace("〈/i〉", "</i>");
        result = result.Replace("〈size=24〉", "<size=24>").Replace("〈size=20〉", "<size=20>").Replace("〈size=18〉", "<size=18>");
        result = result.Replace("〈/size〉", "</size>");

        // 还原常见 HTML 实体
        result = result.Replace("&amp;", "&");
        result = result.Replace("&lt;", "<");
        result = result.Replace("&gt;", ">");
        result = result.Replace("&nbsp;", " ");
        result = result.Replace("&quot;", "\"");
        result = result.Replace("&#39;", "'");

        // 合并多余空行（超过 2 个连续换行压缩为 2 个）
        result = Regex.Replace(result, @"\n{3,}", "\n\n");

        return result.Trim();
    }
}
