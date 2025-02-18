using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[ViewPrefab("Prefabs/TabButtonView")]
internal class TabButtonView : View<TabButtonView>
{
    public TabButton button;
    public Text btnText;
    public void SetInfo(string text)
    {
        btnText.text = text;
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