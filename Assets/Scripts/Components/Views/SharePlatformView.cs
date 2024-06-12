using System;
using System.Collections;
using UnityEditor;
using UnityEngine.UI;

[ViewPrefab("Prefabs/SharePlantformOptionView")]
internal class SharePlatformView : View<SharePlatformView>
{
    public Button closeBtn;
    private Action OnClose;
    
    void Awake()
    {
        closeBtn.onClick.AddListener(OnCloseBtn);
    }

    void OnDestroy()
    {
        closeBtn.onClick.RemoveListener(OnCloseBtn);
    }

    void OnCloseBtn()
    {
        OnClose.Invoke();
    }

    public void SetCloseCallback(Action OnClose)
    {
        this.OnClose = OnClose;
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