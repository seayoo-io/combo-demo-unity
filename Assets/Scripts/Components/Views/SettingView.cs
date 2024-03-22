using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ViewPrefab("Prefabs/SettingView")]
internal class SettingView : View<SettingView>
{
    public Button logoutBtn;
    public Button clearBtn;
    public Button cancelBtn;
    public Button appSettingsBtn;
    private Action OnLogout;
    private Action OnClear;
    private Action OnCancel; 
    private Action OnAppSettings;
    void Awake()
    {
        logoutBtn.onClick.AddListener(OnLogoutConfirmBtn);
        clearBtn.onClick.AddListener(OnClearConfirmBtn);
        cancelBtn.onClick.AddListener(OnClickCancelBtn);
        appSettingsBtn.onClick.AddListener(OnAppSettingsBtn);
        ButtonManager.SetButtonEnabledByType(logoutBtn, ButtonType.LogoutButton);
        ButtonManager.SetButtonEnabledByType(clearBtn, ButtonType.ClearButton);
    }

    void OnDestroy()
    {
        logoutBtn.onClick.RemoveListener(OnLogoutConfirmBtn);
        clearBtn.onClick.RemoveListener(OnClearConfirmBtn);
        cancelBtn.onClick.RemoveListener(OnClickCancelBtn);
        appSettingsBtn.onClick.RemoveListener(OnAppSettingsBtn);
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

    protected override IEnumerator OnHide()
    {
        yield return null;
    }

    protected override IEnumerator OnShow()
    {
        yield return null;
    }
}
