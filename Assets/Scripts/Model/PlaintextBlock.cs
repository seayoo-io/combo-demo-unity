using System.Collections.Generic;

// plaintext 格式公告内容的渲染块基类，由 PlaintextContentParser 解析生成
// UI 层根据子类类型选择对应渲染方式
public abstract class PlaintextBlock { }

// 文本段落块，Content 已完成行内 Markdown → Unity 富文本标签转换
public class TextBlock : PlaintextBlock
{
    public string Text;
}

// 标题块，对应 # ~ ###### 语法，Level 为 1–6
public class HeadingBlock : PlaintextBlock
{
    public int Level;
    public string Text;
}

// 列表块，支持无序（- * +）和有序（1. 2.）
public class ListBlock : PlaintextBlock
{
    public bool Ordered;
    public List<string> Items;
}

// 水平分隔线，对应 --- / *** / ___ （3个以上同类字符）
public class HorizontalRuleBlock : PlaintextBlock { }

// 代码块，对应 ``` ``` 围栏语法；Language 为可选语言提示
public class CodeBlock : PlaintextBlock
{
    public string Language;
    public string Code;
}

// 引用块，对应 > 开头的行，Text 已完成行内 Markdown 转换
public class BlockquoteBlock : PlaintextBlock
{
    public string Text;
}

// 图片块，对应 ![alt](url) 格式
public class ImageBlock : PlaintextBlock
{
    public string Url;
    public string Alt;
}

// 表格块，对应 GFM 格式表格（| col1 | col2 |）
public class TableBlock : PlaintextBlock
{
    public List<string> Headers;
    public List<List<string>> Rows;
}
