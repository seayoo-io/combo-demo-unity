using System;
using System.Collections;
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
    public Button closeBtn;
    public InputField widthInput;
    public InputField heightInput;
    public WebViewType webViewType;
    private bool isLogin;
    
    void Awake()
    {
        EventSystem.Register(this);
        closeBtn.onClick.AddListener(Destroy);
        openAnnouncementBtn.onClick.AddListener(OnConfirm);
    }

    void OnDestroy()
    {
        EventSystem.UnRegister(this);
        closeBtn.onClick.RemoveListener(Destroy);
        openAnnouncementBtn.onClick.RemoveListener(OnConfirm);
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
        var opts = new RedeemGiftCodeOptions()
        {
            ServerId = "10001",
            RoleId = "1",
            RoleName = "测试人员1",
            Width = GetInputValue(widthInput),
            Height = GetInputValue(heightInput)
        };
        ComboSDK.RedeemGiftCode(opts, result => {
            if(result.IsSuccess)
            {
                Destroy();
                Log.I("礼包码兑换成功成功");
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

    public void SetGift(string btnText, WebViewType webViewType)
    {
        this.btnText.text = btnText;
        this.webViewType = webViewType;
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
