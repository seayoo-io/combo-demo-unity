using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[ViewPrefab("Prefabs/CategoryItemView")]
internal class CategoryItemView : View<CategoryItemView>
{
    public Button itemBtn;
    public Text titleTxt;
    private Action onClick;

    void Awake()
    {
        itemBtn.onClick.AddListener(OnClickItem);
    }

    void OnDestroy()
    {
        itemBtn.onClick.RemoveListener(OnClickItem);
    }

    void OnClickItem()
    {
        onClick?.Invoke();
    }

    public void SetTitle(string title)
    {
        titleTxt.text = title;
    }

    public void SetClickCallback(Action callback)
    {
        onClick = callback;
    }

    protected override IEnumerator OnHide()
    {
        yield return null;
    }

    protected override IEnumerator OnShow()
    {
        yield return null;
    }
}
