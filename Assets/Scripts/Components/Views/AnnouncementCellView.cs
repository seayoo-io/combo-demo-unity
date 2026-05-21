using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[ViewPrefab("Prefabs/AnnouncementCellView")]
internal class AnnouncementCellView : View<AnnouncementCellView>
{
    public Text titleText;
    public Text dateText;
    public Button clickBtn;
    public Image selectionBar;

    // 外部通过 Data.Id 来定位当前选中项
    public AnnouncementData Data { get; private set; }

    // 未选中：暗蓝钢色，与游戏主题协调但偏沉
    private static readonly Color NormalBgColor   = new Color(0.16f, 0.21f, 0.36f, 1f);
    private static readonly Color SelectedBgColor = new Color(0.25f, 0.36f, 0.62f, 1f);
    private static readonly Color SelectedBarColor = new Color(0.55f, 0.75f, 1.00f, 1f);

    void Awake()
    {
        clickBtn.onClick.AddListener(OnClick);
        // Transition=None，颜色完全由代码控制，设初始底色
        clickBtn.image.color = NormalBgColor;
    }

    void OnDestroy()
    {
        clickBtn.onClick.RemoveListener(OnClick);
    }

    public void SetData(AnnouncementData announcementData)
    {
        Data = announcementData;
        titleText.text = announcementData.Subtitle;
        dateText.text = DateTimeOffset.FromUnixTimeSeconds(announcementData.PublishedAt)
            .LocalDateTime.ToString("yyyy-MM-dd");
    }

    // 由 AnnouncementView 调用，切换选中/未选中外观
    public void SetSelected(bool selected)
    {
        clickBtn.image.color = selected ? SelectedBgColor : NormalBgColor;
        if (selectionBar != null)
        {
            selectionBar.color = selected ? SelectedBarColor : Color.clear;
        }
    }

    private void OnClick()
    {
        SelectAnnouncementEvent.Invoke(new SelectAnnouncementEvent { Data = Data });
    }

    protected override IEnumerator OnShow() { yield return null; }
    protected override IEnumerator OnHide() { yield return null; }
}
