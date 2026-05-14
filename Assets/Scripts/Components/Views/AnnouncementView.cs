using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

// 公告主视图，左侧列表 + 右侧详情的双栏布局
// 数据来源：由 UIController 在 Show() 前调用 Populate() 注入，默认使用 AnnouncementMockData
[ViewPrefab("Prefabs/AnnouncementView")]
internal class AnnouncementView : View<AnnouncementView>
{
    // 关闭按钮
    public Button closeBtn;

    // 左侧列表的 ScrollRect Content，CellView 动态挂载到此节点下
    public RectTransform listContent;

    // 无公告时显示的空状态面板
    public GameObject emptyPanel;

    // 有公告选中时显示的详情面板
    public GameObject detailPanel;

    // 右侧详情标题
    public Text detailTitle;

    // 右侧详情子标题，无值时隐藏
    public Text detailSubtitle;

    // 右侧详情发布日期
    public Text detailDate;

    // 右侧内容区容器，内容块动态挂载到此节点下
    public RectTransform contentContainer;

    // 内容块使用的字体，运行时赋给动态创建的 Text 组件
    public Font contentFont;

    private readonly List<AnnouncementCellView> _cells = new List<AnnouncementCellView>();
    private AnnouncementCellView _selectedCell;

    void Awake()
    {
        EventSystem.Register(this);
        closeBtn.onClick.AddListener(Destroy);
    }

    // 用指定数据填充公告列表，由 UIController 在 Show() 前调用
    // 不传则使用 Mock 数据，保持原有测试流程不变
    internal void Populate(List<AnnouncementData> announcements = null)
    {
        if (announcements == null)
            announcements = AnnouncementMockData.GetMockAnnouncements();

        foreach (Transform child in listContent)
            GameObject.Destroy(child.gameObject);
        _cells.Clear();

        foreach (var data in announcements)
        {
            var cell = AnnouncementCellView.Instantiate();
            cell.gameObject.transform.localScale = Vector3.one;
            cell.SetData(data);
            cell.gameObject.transform.SetParent(listContent, false);
            cell.Show();
            _cells.Add(cell);
        }

        if (announcements.Count == 0)
        {
            emptyPanel.SetActive(true);
            detailPanel.SetActive(false);
        }
        else
        {
            emptyPanel.SetActive(false);
            detailPanel.SetActive(false);
            SelectAnnouncementEvent.Invoke(new SelectAnnouncementEvent { Data = announcements[0] });
        }
    }

    void OnDestroy()
    {
        EventSystem.UnRegister(this);
        closeBtn.onClick.RemoveListener(Destroy);
    }

    // 监听列表选中事件，更新右侧详情面板
    [EventSystem.BindEvent]
    void OnSelectAnnouncement(SelectAnnouncementEvent evt)
    {
        _selectedCell?.SetSelected(false);
        _selectedCell = _cells.Find(c => c.Data != null && c.Data.Id == evt.Data.Id);
        _selectedCell?.SetSelected(true);

        var data = evt.Data;
        detailPanel.SetActive(true);

        detailTitle.text = data.Title;

        if (string.IsNullOrEmpty(data.Subtitle))
            detailSubtitle.gameObject.SetActive(false);
        else
        {
            detailSubtitle.gameObject.SetActive(true);
            detailSubtitle.text = data.Subtitle;
        }

        var dateStr = DateTimeOffset.FromUnixTimeSeconds(data.PublishedAt).LocalDateTime.ToString("yyyy-MM-dd");
        detailDate.text = dateStr;

        RebuildContent(data);
    }

    // 清空内容区并根据 Format 重新填充内容块
    private void RebuildContent(AnnouncementData data)
    {
        foreach (Transform child in contentContainer)
            GameObject.Destroy(child.gameObject);

        if (string.Equals(data.Format, "html", StringComparison.OrdinalIgnoreCase))
            BuildHtmlContent(data.Content);
        else
            BuildPlaintextContent(data.Content);
    }

    // 将 HTML 内容转换后用单个 Text 块渲染
    private void BuildHtmlContent(string html)
    {
        var richText = HtmlToUnityRichText.Convert(html);
        var textGo = MakeTextBlock(richText, supportRichText: true);
        textGo.transform.SetParent(contentContainer, false);
    }

    // 将 plaintext 内容解析为块列表后逐块渲染
    private void BuildPlaintextContent(string content)
    {
        var blocks = PlaintextContentParser.Parse(content);
        foreach (var block in blocks)
        {
            GameObject go = null;
            if (block is TextBlock tb)
                go = MakeTextBlock(tb.Text, supportRichText: true);
            else if (block is HeadingBlock hb)
                go = MakeHeadingBlock(hb);
            else if (block is ListBlock lb)
                go = MakeListBlock(lb);
            else if (block is HorizontalRuleBlock)
                go = MakeHorizontalRule();
            else if (block is CodeBlock cb)
                go = MakeCodeBlock(cb);
            else if (block is BlockquoteBlock bqb)
                go = MakeBlockquoteBlock(bqb);
            else if (block is ImageBlock ib)
            {
                go = MakeImagePlaceholder(ib.Alt);
                if (go != null)
                    StartCoroutine(LoadImage(go.GetComponent<RawImage>(), ib.Url));
            }
            else if (block is TableBlock tab)
                go = MakeTableBlock(tab);

            if (go != null)
                go.transform.SetParent(contentContainer, false);
        }
    }

    // 创建文本块 GameObject（Text + ContentSizeFitter）
    private GameObject MakeTextBlock(string text, bool supportRichText)
    {
        var go = new GameObject("TextBlock", typeof(RectTransform));
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = new Vector2(1, 0);
        rt.pivot = new Vector2(0.5f, 1f);

        var t = go.AddComponent<Text>();
        t.text = text;
        t.font = contentFont != null ? contentFont : Resources.GetBuiltinResource<Font>("Arial.ttf");
        t.fontSize = 20;
        t.color = new Color(0.90f, 0.90f, 0.95f);
        t.supportRichText = supportRichText;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;

        // 不挂 LayoutElement：LayoutElement.preferredHeight 默认 -1，优先级高于 Text，
        // 会覆盖 Text.preferredHeight，导致父 VLG 把高度算成 0。
        // 父 VLG 的 childForceExpandWidth 负责撑满宽度，高度直接从 Text 读取。

        return go;
    }

    // 创建标题块，H1–H6 字号递减，H1/H2 加粗
    private GameObject MakeHeadingBlock(HeadingBlock heading)
    {
        var go = new GameObject("HeadingBlock", typeof(RectTransform));
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = new Vector2(1, 0);
        rt.pivot = new Vector2(0.5f, 1f);

        var t = go.AddComponent<Text>();
        t.text = heading.Text;
        t.font = contentFont != null ? contentFont : Resources.GetBuiltinResource<Font>("Arial.ttf");
        t.supportRichText = true;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;

        switch (heading.Level)
        {
            case 1:
                t.fontSize = 28; t.fontStyle = FontStyle.Bold;
                t.color = Color.white; break;
            case 2:
                t.fontSize = 24; t.fontStyle = FontStyle.Bold;
                t.color = Color.white; break;
            case 3:
                t.fontSize = 22; t.fontStyle = FontStyle.Normal;
                t.color = Color.white; break;
            case 4:
                t.fontSize = 20; t.fontStyle = FontStyle.Normal;
                t.color = new Color(0.85f, 0.90f, 1.00f); break;
            default:
                t.fontSize = 18; t.fontStyle = FontStyle.Normal;
                t.color = new Color(0.70f, 0.80f, 0.95f); break;
        }

        return go;
    }

    // 创建水平分隔线（1px 高度的深蓝色横线）
    private GameObject MakeHorizontalRule()
    {
        var go = new GameObject("HorizontalRule", typeof(RectTransform));
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = new Vector2(1, 0);
        rt.pivot = new Vector2(0.5f, 1f);

        var img = go.AddComponent<Image>();
        img.color = new Color(0.30f, 0.40f, 0.60f);
        img.raycastTarget = false;

        var le = go.AddComponent<LayoutElement>();
        le.minHeight = 1;
        le.preferredHeight = 1;
        le.flexibleWidth = 1;

        return go;
    }

    // 创建列表块，无序用 • 前缀，有序用数字前缀
    private GameObject MakeListBlock(ListBlock list)
    {
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < list.Items.Count; i++)
        {
            if (i > 0) sb.Append('\n');
            sb.Append(list.Ordered ? $"{i + 1}. {list.Items[i]}" : $"• {list.Items[i]}");
        }
        return MakeTextBlock(sb.ToString(), supportRichText: true);
    }

    // 创建引用块：左侧蓝色竖条 + 斜体文本
    private GameObject MakeBlockquoteBlock(BlockquoteBlock blockquote)
    {
        var root = new GameObject("BlockquoteBlock", typeof(RectTransform));
        var rt = root.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = new Vector2(1, 0);
        rt.pivot = new Vector2(0.5f, 1f);

        var hlg = root.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 8;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = true;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;

        // 左侧竖条
        var barGo = new GameObject("Bar", typeof(RectTransform));
        barGo.transform.SetParent(root.transform, false);
        var barImg = barGo.AddComponent<Image>();
        barImg.color = new Color(0.40f, 0.60f, 0.90f);
        barImg.raycastTarget = false;
        var barLe = barGo.AddComponent<LayoutElement>();
        barLe.minWidth = 3;
        barLe.preferredWidth = 3;
        barLe.flexibleWidth = 0;

        // 引用文本
        var textGo = new GameObject("Text", typeof(RectTransform));
        textGo.transform.SetParent(root.transform, false);
        var t = textGo.AddComponent<Text>();
        t.text = blockquote.Text;
        t.font = contentFont != null ? contentFont : Resources.GetBuiltinResource<Font>("Arial.ttf");
        t.fontSize = 19;
        t.color = new Color(0.70f, 0.76f, 0.90f);
        t.fontStyle = FontStyle.Italic;
        t.supportRichText = true;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        var textLe = textGo.AddComponent<LayoutElement>();
        textLe.flexibleWidth = 1;

        return root;
    }

    // 创建代码块：深色背景 + 浅蓝色等宽文本，顶部可选显示语言标签
    private GameObject MakeCodeBlock(CodeBlock code)
    {
        var root = new GameObject("CodeBlock", typeof(RectTransform));
        var rt = root.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = new Vector2(1, 0);
        rt.pivot = new Vector2(0.5f, 1f);

        var bg = root.AddComponent<Image>();
        bg.color = new Color(0.08f, 0.12f, 0.22f);
        bg.raycastTarget = false;

        var vlg = root.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(10, 10, 8, 8);
        vlg.spacing = 4;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;

        if (!string.IsNullOrEmpty(code.Language))
        {
            var langGo = new GameObject("Lang", typeof(RectTransform));
            langGo.transform.SetParent(root.transform, false);
            var langText = langGo.AddComponent<Text>();
            langText.text = code.Language;
            langText.font = contentFont != null ? contentFont : Resources.GetBuiltinResource<Font>("Arial.ttf");
            langText.fontSize = 14;
            langText.color = new Color(0.50f, 0.65f, 0.85f);
            langText.horizontalOverflow = HorizontalWrapMode.Wrap;
            langText.verticalOverflow = VerticalWrapMode.Overflow;
            langText.raycastTarget = false;
        }

        var codeGo = new GameObject("Code", typeof(RectTransform));
        codeGo.transform.SetParent(root.transform, false);
        var codeText = codeGo.AddComponent<Text>();
        codeText.text = code.Code;
        codeText.font = contentFont != null ? contentFont : Resources.GetBuiltinResource<Font>("Arial.ttf");
        codeText.fontSize = 17;
        codeText.color = new Color(0.80f, 0.90f, 1.00f);
        codeText.supportRichText = false;
        codeText.horizontalOverflow = HorizontalWrapMode.Wrap;
        codeText.verticalOverflow = VerticalWrapMode.Overflow;
        codeText.raycastTarget = false;

        return root;
    }

    // 创建图片占位 GameObject（RawImage，先显示灰色占位，异步加载完成后替换纹理）
    private GameObject MakeImagePlaceholder(string alt)
    {
        var go = new GameObject("ImageBlock", typeof(RectTransform));
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = new Vector2(1, 0);
        rt.pivot = new Vector2(0.5f, 1f);

        var raw = go.AddComponent<RawImage>();
        raw.color = new Color(0.85f, 0.85f, 0.85f);
        raw.raycastTarget = false;

        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 160;
        le.flexibleWidth = 1;

        return go;
    }

    // URL → 已下载纹理的会话级缓存，跨公告切换时复用，避免重复请求
    private static readonly Dictionary<string, Texture2D> _textureCache = new Dictionary<string, Texture2D>();

    // 异步下载图片并设置到 RawImage；缓存命中时同步赋值
    private IEnumerator LoadImage(RawImage rawImage, string url)
    {
        if (_textureCache.TryGetValue(url, out var cached))
        {
            rawImage.texture = cached;
            rawImage.color = Color.white;
            yield break;
        }

        using (var req = UnityWebRequestTexture.GetTexture(url))
        {
            yield return req.SendWebRequest();
            if (rawImage == null) yield break;
#if UNITY_2020_1_OR_NEWER
            if (req.result == UnityWebRequest.Result.Success)
#else
            if (!req.isNetworkError && !req.isHttpError)
#endif
            {
                var tex = DownloadHandlerTexture.GetContent(req);
                _textureCache[url] = tex;
                rawImage.texture = tex;
                rawImage.color = Color.white;
            }
        }
    }

    // 创建表格块 GameObject（VerticalLayoutGroup 嵌套 HorizontalLayoutGroup）
    private GameObject MakeTableBlock(TableBlock table)
    {
        var root = new GameObject("TableBlock", typeof(RectTransform));
        var rootVlg = root.AddComponent<VerticalLayoutGroup>();
        rootVlg.spacing = 1;
        rootVlg.childForceExpandWidth = true;
        rootVlg.childForceExpandHeight = false;
        rootVlg.childControlWidth = true;
        rootVlg.childControlHeight = true;

        AppendTableRow(root.transform, table.Headers, isHeader: true);
        foreach (var row in table.Rows)
            AppendTableRow(root.transform, row, isHeader: false);

        return root;
    }

    // 向表格容器追加一行（HorizontalLayoutGroup 包含多个 Text 单元格）
    private void AppendTableRow(Transform parent, List<string> cells, bool isHeader)
    {
        var row = new GameObject("TableRow", typeof(RectTransform));
        var hlg = row.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 1;
        hlg.childForceExpandWidth = true;
        hlg.childForceExpandHeight = true;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;

        var rowImg = row.AddComponent<Image>();
        rowImg.color = isHeader ? new Color(0.18f, 0.26f, 0.45f) : new Color(0.12f, 0.17f, 0.30f);
        rowImg.raycastTarget = false;

        var rowLe = row.AddComponent<LayoutElement>();
        rowLe.minHeight = 32;
        rowLe.preferredHeight = 32;

        row.transform.SetParent(parent, false);

        foreach (var cell in cells)
        {
            var cellGo = new GameObject("Cell", typeof(RectTransform));
            cellGo.transform.SetParent(row.transform, false);

            var cellImg = cellGo.AddComponent<Image>();
            cellImg.color = new Color(0.14f, 0.19f, 0.34f);
            cellImg.raycastTarget = false;

            var cellLe = cellGo.AddComponent<LayoutElement>();
            cellLe.flexibleWidth = 1;

            // Text 必须放在子节点，Image 和 Text 不能共存于同一 GameObject
            var textGo = new GameObject("Text", typeof(RectTransform));
            textGo.transform.SetParent(cellGo.transform, false);
            var textRt = textGo.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(2, 2);
            textRt.offsetMax = new Vector2(-2, -2);

            var t = textGo.AddComponent<Text>();
            t.text = cell;
            t.font = contentFont != null ? contentFont : Resources.GetBuiltinResource<Font>("Arial.ttf");
            t.fontSize = 18;
            t.color = new Color(0.90f, 0.90f, 0.95f);
            t.fontStyle = FontStyle.Normal;
            t.alignment = TextAnchor.MiddleCenter;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
        }
    }

    protected override IEnumerator OnShow() { yield return null; }
    protected override IEnumerator OnHide() { yield return null; }
}
