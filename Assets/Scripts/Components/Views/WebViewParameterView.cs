using System;
using System.Collections;
using System.Text.RegularExpressions;
using Combo;
using UnityEngine;
using UnityEngine.UI;

public enum WebViewType
{
    ANNOUNCEMENT,
    GIFT
}

[ViewPrefab("Prefabs/WebViewParameterView")]
internal class WebViewParameterView : View<WebViewParameterView>
{
    public Button openAnnouncementBtn;
    public Text btnText;
    public Text titleText;
    public Button closeBtn;
    public InputField giftInput;
    public InputField widthInput;
    public InputField heightInput;
    public WebViewType webViewType;
    public GameObject giftPanel;
    private bool isLogin;
    
    void Awake()
    {
        EventSystem.Register(this);
        closeBtn.onClick.AddListener(Destroy);
        openAnnouncementBtn.onClick.AddListener(OnConfirm);
        giftInput.onValueChanged.AddListener(ValidateInput);

        // 设置 widthInput 和 heightInput 的默认值为 100
        widthInput.text = "100";
        heightInput.text = "100";
    }

    void OnDestroy()
    {
        EventSystem.UnRegister(this);
        closeBtn.onClick.RemoveListener(Destroy);
        openAnnouncementBtn.onClick.RemoveListener(OnConfirm);
    }

        void ValidateInput(string input)
        {
            // 用正则表达式匹配并移除空格
            giftInput.text = Regex.Replace(input, @"\s+", "");
        }

    public void OnConfirm()
    {
        switch (webViewType)
        {
            case WebViewType.ANNOUNCEMENT:
                OpenAnnouncement();
                break;
            case WebViewType.GIFT:
                RedeemGiftCode();
                break;
        }
    }

    public void OpenAnnouncement()
    {
        var opts = new OpenAnnouncementsOptions();
        if(isLogin)
        {
            var currentPlayer = PlayerController.GetPlayer();
            opts = new OpenAnnouncementsOptions()
            {
                Profile = currentPlayer.role.roleId,
                Level = currentPlayer.role.roleLevel,
                Width = GetInputValue(widthInput),
                Height = GetInputValue(heightInput),
            };
            Log.I($"OpenAnnouncementsOptions: Profile =  {opts.Profile}, Level =  {opts.Level}");
        }
        else
        {
            opts = new OpenAnnouncementsOptions()
            {
                Width = GetInputValue(widthInput),
                Height = GetInputValue(heightInput),
            };
            Log.I($"OpenAnnouncementsOptions: 未登录状态");
        }
        ComboSDK.OpenAnnouncements(opts, result =>{
            if(result.IsSuccess)
            {
                OpenAnnouncementsEvent.Invoke();
                Destroy();
                Log.I("公告打开成功");
            }
            else
            {
                Toast.Show($"公告打开失败：{result.Error.Message}");
            }
        });
    }

    public void RedeemGiftCode()
    {
        var currentPlayer = PlayerController.GetPlayer();
        RedeemGiftCodeOptions opts = new RedeemGiftCodeOptions();
        if(giftInput.text == "")
        {
            opts = new RedeemGiftCodeOptions()
            {
                ServerId = currentPlayer.role.serverId,
                RoleId = currentPlayer.role.roleId,
                RoleName = currentPlayer.role.roleName,
                Width = GetInputValue(widthInput),
                Height = GetInputValue(heightInput)
            };
        }
        else
        {
            opts = new RedeemGiftCodeOptions()
            {
                GiftCode = giftInput.text,
                ServerId = currentPlayer.role.serverId,
                RoleId = currentPlayer.role.roleId,
                RoleName = currentPlayer.role.roleName,
                Width = GetInputValue(widthInput),
                Height = GetInputValue(heightInput)
            };
        }
        
        ComboSDK.RedeemGiftCode(opts, result => {
            if(result.IsSuccess)
            {
                Destroy();
                Log.I("礼包码兑换流程结束");
            }
            else
            {
                Toast.Show($"礼包码兑换失败：{result.Error.Message}");
            }
        });
    }

    public void SetAnnounceInfoLogin(bool isLogin, string btnText, WebViewType webViewType)
    {
        this.isLogin = isLogin;
        this.btnText.text = btnText;
        this.webViewType = webViewType;
    }

    public void SetGift(string btnText, string title, WebViewType webViewType)
    {
        this.btnText.text = btnText;
        titleText.text = title;
        this.webViewType = webViewType;
        giftPanel.SetActive(true);
    }

    private int GetInputValue(InputField inputField)
    {
        if(string.IsNullOrEmpty(inputField.text))
        {
            return 0;
        }
        else
        {
            return int.Parse(inputField.text);
        }
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
