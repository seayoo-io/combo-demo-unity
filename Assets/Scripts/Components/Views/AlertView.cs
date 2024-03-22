using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[ViewPrefab("Prefabs/AlertView")]
internal class AlertView : View<AlertView>
{
    public Text titleTxt;
    public Text contentTxt;
    public Button confirmBtn;
    public Button cancelBtn;
    public Text confirmBtnTxt;
    public Text cancelBtnTxt;
    public RectTransform titlePanel;
    public RectTransform contentPanel;
    private Action OnConfirm;
    private Action OnCancel;

    public void ShowCancelBtn()
    {
        cancelBtn.gameObject.SetActive(true);
    }

    public void HideCancelBtn()
    {
        cancelBtn.gameObject.SetActive(false);
    }

    public void SetTitle(string title)
    {
        titleTxt.text = title;
        RefreshLayout();
    }

    public void SetMessage(string content)
    {
        contentTxt.text = content;
        RefreshLayout();
    }

    public void SetConfirmBtnTitle(string title)
    {
        confirmBtnTxt.text = title;
        RefreshLayout();
    }

    public void SetCancelBtnTitle(string title)
    {
        cancelBtnTxt.text = title;
        RefreshLayout();
    }

    public void SetConfirmCallback(Action OnConfirm)
    {
        this.OnConfirm = OnConfirm;
    }

    public void SetCancelCallback(Action OnCancel)
    {
        this.OnCancel = OnCancel;
    }

    void Awake()
    {
        confirmBtn.onClick.AddListener(OnClickConfirmBtn);
        cancelBtn.onClick.AddListener(OnClickCancelBtn);
    }

    void OnDestroy()
    {
        confirmBtn.onClick.RemoveListener(OnClickConfirmBtn);
        cancelBtn.onClick.AddListener(OnClickCancelBtn);
    }

    void OnClickConfirmBtn()
    {
        OnConfirm.Invoke();
    }

    void OnClickCancelBtn()
    {
        OnCancel.Invoke();
    }

    protected override IEnumerator OnHide()
    {
        yield return null;
    }

    protected override IEnumerator OnShow()
    {
        yield return null;
    }

    private void RefreshLayout() {
        LayoutRebuilder.ForceRebuildLayoutImmediate(titlePanel);
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentPanel);
    }
}
