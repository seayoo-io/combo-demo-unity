using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ViewPrefab("Prefabs/PlayerInfoView")]
internal class PlayerInfoView : View<PlayerInfoView>
{
    public Text playerId;
    public Button copyBtn;
    public Button cancelBtn;
    private Action OnCopy;
    private Action OnCancel;

    void Awake()
    {
        copyBtn.onClick.AddListener(OnCopyConfigBtn);
        cancelBtn.onClick.AddListener(OnCancelConfigBtn);
    }

    void OnDestroy()
    {
        copyBtn.onClick.RemoveListener(OnCopyConfigBtn);
        cancelBtn.onClick.RemoveListener(OnCancelConfigBtn);
    }
    
    public void SetPlayerId(string id) {
        playerId.text = id;
    }

    void OnCopyConfigBtn()
    {
        OnCopy.Invoke();
    }
    void OnCancelConfigBtn()
    {
        OnCancel.Invoke();
    }
    public void SetCopyCallback(Action OnCopy)
    {
        this.OnCopy = OnCopy;
    }
    public void SetCancelCallback(Action OnCancel)
    {
        this.OnCancel = OnCancel;
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
