using System;
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
    public InputField roomHostId;
    public InputField matchType;
    public InputField queueRoleIdList;
    public InputField stageIdText;
    public Dropdown stageTypeDropDown;
    public Dropdown battleResultDropDown;
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

    public void OnRoundEndReport()
    {
        if (string.IsNullOrEmpty(roomHostId.text))
        {
            Toast.Show("房主 comboId 不可为空");
            return;
        }
        if (string.IsNullOrEmpty(matchType.text))
        {
            Toast.Show("匹配类型不可为空");
            return;
        }
        if (string.IsNullOrEmpty(queueRoleIdList.text))
        {
            Toast.Show("队列成员 id 列表不可为空");
            return;
        }
        if (matchType.text != "0" && matchType.text != "1")
        {
            Toast.Show("匹配类型只能为 0 或 1");
            return;
        }
        List<string> roleIdList;
        try
        {
            // 使用正则表达式验证字符串
            string input = queueRoleIdList.text;
            System.Text.RegularExpressions.Regex validPattern = new System.Text.RegularExpressions.Regex(@"^[0-9;]+$"); // 匹配只能包含数字和分号
            if (!validPattern.IsMatch(input))
            {
                Toast.Show("队列成员 id 列表格式不正确，只能包含数字和分号");
                return;
            }

            // 转换为 List<string>，分号分隔
            roleIdList = new List<string>(input.Split(';'));

            if (roleIdList.Count == 0)
            {
                Toast.Show("队列成员 id 列表不可为空");
                return;
            }

            // 如果需要进一步验证每个 ID 是否为合法数字，可以在这里处理
            foreach (var roleId in roleIdList)
            {
                if (string.IsNullOrEmpty(roleId))
                {
                    Toast.Show("队列成员 id 列表存在空的 ID");
                    return;
                }
            }
        }
        catch
        {
            Toast.Show("队列成员 id 列表格式不正确，请使用分号分隔");
            return;
        }
        RoundEndEvent.Invoke(new RoundEndEvent
        {
            roomHostId = roomHostId.text,
            matchType = matchType.text,
            queueRoleIdList = roleIdList
        });
    }

    private Dictionary<int, int> stageType = new Dictionary<int, int>
    {
        { 0, 1 },
        { 1, 2 },
        { 2, 3 },
        { 3, 4 },
        { 4, 63 },
        { 5, 65 },
        { 6, 11 }
    };

    private Dictionary<int, string> battleResult = new Dictionary<int, string>
    {
        { 0, "BATTLE_WIN" },
        { 1, "BATTLE_FAIL" },
        { 2, "1" }
    };

    public void OnBattleEndReport()
    {
        if (string.IsNullOrEmpty(stageIdText.text))
        {
            Toast.Show("关卡 id 不可为空");
            return;
        }
        
        var stageTypeVaule = stageType[stageTypeDropDown.value];
        var battleResultVaule = battleResult[battleResultDropDown.value];
        BattleEndEvent.Invoke(new BattleEndEvent
        {
            stageId = int.Parse(stageIdText.text),
            stageType = stageTypeVaule,
            battleType = battleResultVaule
        });
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
