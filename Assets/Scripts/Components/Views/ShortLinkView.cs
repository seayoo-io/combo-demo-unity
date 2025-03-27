using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ViewPrefab("Prefabs/ShortLinkView")]
internal class ShortLinkView : View<ShortLinkView>
{
    public InputField shortLinkField;
    public Button openBtn;
    public Button cancelBtn;
    private Action OnCancel;

    void Awake()
    {
        cancelBtn.onClick.AddListener(OnClickCancelBtn);
        openBtn.onClick.AddListener(OnOpenShortLink); 
    }

    void OnDestroy()
    {
        cancelBtn.onClick.RemoveListener(OnClickCancelBtn);
        openBtn.onClick.RemoveListener(OnOpenShortLink);
    }

    public void OnOpenShortLink(){
        OpenShortLinkEvent.Invoke(new OpenShortLinkEvent{
             shortLink = shortLinkField.text
        });
    }


    void OnClickCancelBtn()
    {
        OnCancel.Invoke();
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
