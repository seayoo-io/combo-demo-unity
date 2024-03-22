using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ViewPrefab("Prefabs/SystemShareView")]
internal class SystemShareView : View<SystemShareView>
{
    public Button linkShareBtn;
    public Button onlineImageShareBtn;
    public Button localImageShareBtn;
    public Button cancelBtn;
    private Action OnLinkShare;
    private Action OnOnlineShare;
    private Action OnLocalShare;
    private Action OnCancel; 
    void Awake()
    {
        linkShareBtn.onClick.AddListener(OnLinkShareConfirmBtn);
        onlineImageShareBtn.onClick.AddListener(OnOnlineShareConfirmBtn);
        localImageShareBtn.onClick.AddListener(OnLocalShareConfirmBtn);
        cancelBtn.onClick.AddListener(OnClickCancelBtn);
        ButtonManager.SetButtonEnabledByType(linkShareBtn, ButtonType.LinkShareButton);
        ButtonManager.SetButtonEnabledByType(onlineImageShareBtn, ButtonType.OnlineShareButton);
        ButtonManager.SetButtonEnabledByType(localImageShareBtn, ButtonType.LocalShareButton);
    }

    void OnDestroy()
    {
        linkShareBtn.onClick.RemoveListener(OnLinkShareConfirmBtn);
        onlineImageShareBtn.onClick.RemoveListener(OnOnlineShareConfirmBtn);
        localImageShareBtn.onClick.RemoveListener(OnLocalShareConfirmBtn);
        cancelBtn.onClick.RemoveListener(OnClickCancelBtn);
    }

    void OnLinkShareConfirmBtn()
    {
        OnLinkShare.Invoke();
    }

    void OnOnlineShareConfirmBtn()
    {
        OnOnlineShare.Invoke();
    }

    void OnLocalShareConfirmBtn()
    {
        OnLocalShare.Invoke();
    }

    void OnClickCancelBtn()
    {
        OnCancel.Invoke();
    }

    public void SetLinkCallback(Action OnLink)
    {
        this.OnLinkShare = OnLink;
    }

    public void SetOnlineCallback(Action OnOnLine)
    {
        this.OnOnlineShare = OnOnLine;
    }

    public void SetLocalCallback(Action OnLocal)
    {
        this.OnLocalShare = OnLocal;
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
