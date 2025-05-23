﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ViewPrefab("Prefabs/SettingView")]
internal class SettingView : View<SettingView>
{
    public InputField shortLink;
    public InputField promoPseudoPurchaseAmount;
    public InputField activeValue;
    public Button logoutBtn;
    public Button clearBtn;
    public Button cancelBtn;
    public Button appSettingsBtn;
    public Button openShortLink;
    public Button playerAgreementBtn;
    public Button privacyPolicyBtn;
    public Button privacyChildrenBtn;
    public Button thirdPartyBtn;
    public Button fangchenmiBtn;
    public Button resetGuestBtn;
    private Action OnLogout;
    private Action OnClear;
    private Action OnCancel; 
    private Action OnAppSettings;
    private Action OnPlayerAgreement;
    private Action OnPrivacyPolicy;
    private Action OnPrivacyChildren;
    private Action OnThirdParty;
    private Action OnFangchenmi;
    private Action OnResetGuest;
    void Awake()
    {
        logoutBtn.onClick.AddListener(OnLogoutConfirmBtn);
        clearBtn.onClick.AddListener(OnClearConfirmBtn);
        cancelBtn.onClick.AddListener(OnClickCancelBtn);
        appSettingsBtn.onClick.AddListener(OnAppSettingsBtn);
        playerAgreementBtn.onClick.AddListener(OnPlayerAgreementBtn);
        privacyPolicyBtn.onClick.AddListener(OnPrivacyPolicyBtn);
        privacyChildrenBtn.onClick.AddListener(OnPrivacyChildrenBtn);
        thirdPartyBtn.onClick.AddListener(OnThirdPartyBtn);
        fangchenmiBtn.onClick.AddListener(OnFangchenmiBtn);
        resetGuestBtn.onClick.AddListener(OnResetGuestBtn);
        ButtonManager.SetButtonEnabledByType(logoutBtn, ButtonType.LogoutButton);
        ButtonManager.SetButtonEnabledByType(clearBtn, ButtonType.ClearButton);
    }

    void OnDestroy()
    {
        logoutBtn.onClick.RemoveListener(OnLogoutConfirmBtn);
        clearBtn.onClick.RemoveListener(OnClearConfirmBtn);
        cancelBtn.onClick.RemoveListener(OnClickCancelBtn);
        appSettingsBtn.onClick.RemoveListener(OnAppSettingsBtn);
        playerAgreementBtn.onClick.RemoveListener(OnPlayerAgreementBtn);
        privacyPolicyBtn.onClick.RemoveListener(OnPrivacyPolicyBtn);
        privacyChildrenBtn.onClick.RemoveListener(OnPrivacyChildrenBtn);
        thirdPartyBtn.onClick.RemoveListener(OnThirdPartyBtn);
        fangchenmiBtn.onClick.RemoveListener(OnFangchenmiBtn);
        resetGuestBtn.onClick.RemoveListener(OnResetGuestBtn);
    }

    public void OnOpenShortLink(){
        OpenShortLinkEvent.Invoke(new OpenShortLinkEvent {
            shortLink = shortLink.text,
        });
    }

    public void OnPromoPseudoPurchase()
    {
        PromoPseudoPurchaseEvent.Invoke(new PromoPseudoPurchaseEvent {
            amount = promoPseudoPurchaseAmount.text
        });
    }

    public void OnActiveAvlue()
    {
        ActiveValueEvent.Invoke(new ActiveValueEvent {
            value = activeValue.text
        });
    }

    public void OnLoginReport()
    {
        LoginEvent.Invoke(new LoginEvent {});
    }

    public void OpenLanguageView()
    {
        LanguageView.Instantiate();
        Destroy();
    }

    void OnLogoutConfirmBtn()
    {
        OnLogout.Invoke();
    }

    void OnClearConfirmBtn()
    {
        OnClear.Invoke();
    }

    void OnClickCancelBtn()
    {
        OnCancel.Invoke();
    }

    void OnAppSettingsBtn()
    {
        OnAppSettings.Invoke();
    }

    void OnPlayerAgreementBtn()
    {
        OnPlayerAgreement.Invoke();
    }

    void OnPrivacyPolicyBtn()
    {
        OnPrivacyPolicy.Invoke();
    }

    void OnPrivacyChildrenBtn()
    {
        OnPrivacyChildren.Invoke();
    }

    void OnThirdPartyBtn()
    {
        OnThirdParty.Invoke();
    }

    void OnFangchenmiBtn()
    {
        OnFangchenmi.Invoke();
    }

    void OnResetGuestBtn()
    {
        OnResetGuest.Invoke();
    }

    public void SetLogoutCallback(Action OnLogout)
    {
        this.OnLogout = OnLogout;
    }

    public void SetClearCallback(Action OnClear)
    {
        this.OnClear = OnClear;
    }

    public void SetCancelCallback(Action OnCancel)
    {
        this.OnCancel = OnCancel;
    }

    public void SetAppSettingsCallback(Action OnAppSettings)
    {
        this.OnAppSettings = OnAppSettings;
    }

    public void SetPlayerAgreementCallback(Action OnPlayerAgreement)
    {
        this.OnPlayerAgreement = OnPlayerAgreement;
    }
    public void SetPrivacyPolicyCallback(Action OnPrivacyPolicy)
    {
        this.OnPrivacyPolicy = OnPrivacyPolicy;
    }
    public void SetPrivacyChildrenCallback(Action OnPrivacyChildren)
    {
        this.OnPrivacyChildren = OnPrivacyChildren;
    }
    public void SetThirdPartyCallback(Action OnThirdParty)
    {
        this.OnThirdParty = OnThirdParty;
    }
    public void SetFangchenmiCallback(Action OnFangchenmi)
    {
        this.OnFangchenmi = OnFangchenmi;
    }
    public void SetResetGuestCallback(Action OnResetGuest)
    {
        this.OnResetGuest = OnResetGuest;
    }

    public void DisableResetGuestBtn() {
        resetGuestBtn.interactable = false;
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
