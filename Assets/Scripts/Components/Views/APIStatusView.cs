using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ViewPrefab("Prefabs/APIStatusView")]
internal class APIStatusView : View<APIStatusView>
{

    public Button backBtn;

    void Awake()
    {
        backBtn.onClick.AddListener(Destroy);
    }

    void OnDestroy()
    {
        backBtn.onClick.RemoveListener(Destroy);
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
