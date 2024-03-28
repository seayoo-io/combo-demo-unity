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
    public Button accountSettingsBtn;
    public Button deleteAccountBtn;
    public Button customerServiceBtn;
    private Action OnCopy;
    private Action OnCancel;
    private Action OnAccountSettings;
    private Action OnDeleteAccount;
    private Action OnCustomerService;

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
    void OnAccountSettingsConfigBtn()
    {
        OnAccountSettings.Invoke();
    }
    void OnDeleteAccountConfigBtn()
    {
        OnDeleteAccount.Invoke();
    }
    void OnOnCustomerServiceConfigBtn()
    {
        OnCustomerService.Invoke();
    }
    public void SetCopyCallback(Action OnCopy)
    {
        this.OnCopy = OnCopy;
    }
    public void SetCancelCallback(Action OnCancel)
    {
        this.OnCancel = OnCancel;
    }
    public void SetAccountSettingsCallback(Action OnAccountSettings)
    {
        this.OnAccountSettings = OnAccountSettings;
    }
    public void SetDeleteAccountCallback(Action OnDeleteAccount)
    {
        this.OnDeleteAccount = OnDeleteAccount;
    }
    public void SetOnCustomerServiceCallback(Action OnOnCustomerService)
    {
        this.OnCustomerService = OnOnCustomerService;
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
