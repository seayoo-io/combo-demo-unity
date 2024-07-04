using System;
using System.Collections;
using Combo;
using UnityEngine;
using UnityEngine.UI;

[ViewPrefab("Prefabs/AnnouncementParameterView")]
internal class AnnouncementParameterView : View<AnnouncementParameterView>
{
    public Button openAnnouncementBrn;
    public Button closeBtn;
    public InputField widthInput;
    public InputField heightInput;
    private bool isLogin;
    
    void Awake()
    {
        EventSystem.Register(this);
        closeBtn.onClick.AddListener(Destroy);
        openAnnouncementBrn.onClick.AddListener(OpenAnnouncement);
    }

    void OnDestroy()
    {
        EventSystem.UnRegister(this);
        closeBtn.onClick.RemoveListener(Destroy);
        openAnnouncementBrn.onClick.RemoveListener(OpenAnnouncement);
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
                }
                else
                {
                    Toast.Show($"公告打开失败：{result.Error.Message}");
                }
        });
    }

    public void SetIsLogin(bool isLogin)
    {
        this.isLogin = isLogin;
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
