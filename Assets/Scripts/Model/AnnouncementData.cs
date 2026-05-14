// 公告数据模型，对应 Combo SDK GetAnnouncements API 返回的单条公告结构
// Content 字段格式由 Format 决定：plaintext 使用 Markdown 子集，html 使用富文本
public class AnnouncementData
{
    // 公告唯一 ID
    public string Id;

    // 公告标题
    public string Title;

    // 公告子标题，可为 null，UI 层无值时应隐藏对应区域
    public string Subtitle;

    // 公告正文内容，格式由 Format 字段决定
    public string Content;

    // 内容格式："plaintext" 或 "html"，使用 string 而非 enum 以保持向前兼容
    public string Format;

    // 本地化语言标签（IETF BCP 47，如 zh-CN），为空表示使用公告默认内容
    public string Lang;

    // 公告最后发布时间，Unix timestamp（秒）
    public int PublishedAt;
}
