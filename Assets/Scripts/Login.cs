using System;
using System.Collections.Generic;
using Combo;
using ThinkingData.Analytics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Login : MonoBehaviour
{
    private static readonly int MAX_LOGIN_RETRY = 3;
    private static bool isFirstStart = true;
    public Button enterGameBtn;
    public Button loginBtn;
    public Button logoutBtn;
    public Button contactSupportBtn;
    public Button switchAccountBtn;
    public Button openAnnouncementsBtn;
    public Dropdown selectServerDropdown;

    private int loginRetryCount = 0;
    private string lastError = "";
    private bool isLogin = false;

    void Awake()
    {
        EventSystem.Register(this);
        ComboSDK.OnKickOut(result =>
        {
            if (result.IsSuccess)
            {
                if (result.Data.shouldExit)
                {
                    Application.Quit();
                }
                else
                {
                    SceneManager.LoadScene("Login");
                }
            }
        });
    }
    void Start()
    {
        var buildParams = BuildParams.Load();

        if (buildParams.hotUpdate || buildParams.forceUpdate)
        {
            UpdateGameViewController.Show(buildParams.forceUpdate);
        }
        else
        {
            LoginInit();
        }
        if(!ComboSDK.IsFeatureAvailable(Feature.CONTACT_SUPPORT)) {
            contactSupportBtn.gameObject.SetActive(false);
        }

        CheckAnnouncements();
    }

    void OnDestroy()
    {
        EventSystem.UnRegister(this);
    }

    [EventSystem.BindEvent]
    void OnUpdateGame(UpdateGameFinishedEvent evt)
    {

        LoginInit();
    }

    void LoginInit()
    {
        if (isFirstStart)
        {
            LoadingBar.Show();
            enterGameBtn.interactable = false;
        }

        loginRetryCount = 0;
        ShowLoginBtn();
        AutoLogin();
        isFirstStart = false;
    }

    public void OnLogin()
    {
        LoginGame(() =>
        {
            ShowEnterGameBtn();
        }, () =>
        {
            ShowLoginBtn();
        });
    }

    public void OnEnterGame()
    {
        // PlayerController.PlayerEnterGame(PlayerController.GetPlayer());
        SceneManager.LoadScene("Select");
    }

    public void OnLogout()
    {
        ComboSDK.Logout(result =>
        {
            if (result.IsSuccess)
            {
                isLogin = false;
                GameClient.Logout();
                var currentPlayer = PlayerController.GetPlayer();
                if(currentPlayer != null)
                {
                    Destroy(currentPlayer.gameObject);
                }
                CheckAnnouncements();
                Toast.Show($"玩家 {result.Data.comboId} 退出登录");
                ShowLoginBtn();
            }
            else
            {
                Toast.Show($"登出失败: {result.Error.Message}");
                Log.E("登出失败：" + result.Error.ToString());
            }
        });
    }

    private void CheckAndUpdateGame(Action OnCancel)
    {
        if (ComboSDK.IsFeatureAvailable(Feature.UPDATE_GAME))
        {
            ComboSDK.UpdateGame(result =>
            {
                if (result.IsSuccess)
                {
                    Toast.Show("更新成功");
                    CheckAndUpdateGame(OnCancel);
                }
                else
                {
                    Toast.Show("更新失败：" + result.Error.ToString());
                    OnCancel.Invoke();
                }
            });
        }
        else
        {
            ComboSDK.GetDownloadUrl(result =>
            {
                if (result.IsSuccess)
                {
                    Log.D("GameUpdateUrl: " + result.Data.downloadUrl);
                    UIController.Alert(UIAlertType.Stackable, "更新游戏", "检测到游戏有新版本，请更新游戏", "确定更新", "取消更新", () =>
                    {
                        Application.OpenURL(result.Data.downloadUrl);
                        CheckAndUpdateGame(OnCancel);
                    }, () =>
                    {
                        OnCancel.Invoke();
                    });
                }
                else
                {
                    Log.W("Failed to get game update url:" + result.Error.ToString());
                    OnCancel.Invoke();
                }
            });
        }
    }

    public void OnContactSupport() 
    {
        ComboSDK.ContactSupport();
    }

    public void OnSwitchAccount()
    {
        OnLogout();
        OnLogin();
    }

    public void OpenAnnouncement()
    {
        UIController.ShowAnnouncementParameterView(isLogin);
    }

    [EventSystem.BindEvent]
    void OpenAnnouncement(OpenAnnouncementsEvent evt)
    {
        var image = FindImageByTag(openAnnouncementsBtn.transform, "announcement");
        image.gameObject.SetActive(false);
    }

    private void LoginGame(Action onSuccess, Action onFail)
    {
        UIController.ShowLoading();

        void GameClientLogin(LoginResult result) =>
            GameClient.Login(
                result.loginInfo.identityToken,
                isSuccess =>
                {
                    UIController.HideLoading();
                    if (isSuccess)
                    {
                        Toast.Show($"游戏客户端登录成功");
                        Log.I("游戏客户端登录成功");
                        onSuccess.Invoke();
                        
                        isLogin = true;
                        var currentPlayer = PlayerController.GetPlayer();
                        if(currentPlayer != null)
                        {
                            if(currentPlayer.role.roleId == result.loginInfo.comboId)
                            {
                                CheckAnnouncements(PlayerController.GetPlayer().role.roleId, PlayerController.GetPlayer().role.roleLevel);
                                return;
                            }
                            else
                            {
                                Destroy(currentPlayer.gameObject);
                            }
                        }
                        var newPlayer = PlayerController.SpawnPlayer(result.loginInfo.comboId, "测试角色01", "1");
                        CheckAnnouncements(PlayerController.GetPlayer().role.roleId, PlayerController.GetPlayer().role.roleLevel);
                        DontDestroyOnLoad(newPlayer);
                    }
                    else
                    {
                        Toast.Show($"游戏客户端登录失败");
                        Log.E("游戏客户端登录失败");
                        onFail.Invoke();
                    }
                }
            );

        ComboSDK.Login(r =>
        {
            if (r.IsSuccess)
            {
                var result = r.Data;
                Log.I(
                    $"登录成功: COMBOID - {result.loginInfo.comboId}; TOKEN - {result.loginInfo.identityToken}"
                );
                TDAnalytics.Track(
                    "sdk_login",
                    new Dictionary<string, object>() { { "role_name", "test_player" } }
                );
                GameClientLogin(result);
            }
            else
            {
                UIController.HideLoading();
                var error = r.Error;
                if (error.Code == Combo.ErrorCode.UserCancelled)
                {
                    lastError = Combo.ErrorCode.UserCancelled;
                    Toast.Show("用户取消登录");
                }
                else
                {
                    lastError = "";
                    Toast.Show($"登录失败：{error.Message}");
                }
                Log.E("登录失败: " + error.DetailMessage);
                onFail.Invoke();
            }
        });
    }

    private void AutoLogin()
    {

        LoginGame(() =>
        {
            ShowEnterGameBtn();
        }, () =>
        {
            if (++loginRetryCount > MAX_LOGIN_RETRY)
            {
                ShowLoginBtn();
                return;
            }
            else
            {
                if (lastError == Combo.ErrorCode.UserCancelled)
                {
                    ShowLoginBtn();
                    Toast.Show("用户取消登录");
                    return;
                }
                Toast.Show($"登录失败，正在重试 ({loginRetryCount}/{MAX_LOGIN_RETRY})");
                Invoke(nameof(AutoLogin), 0.3f);
            }
        });
    }

    private void ShowLoginBtn()
    {
        enterGameBtn.gameObject.SetActive(false);
        selectServerDropdown.gameObject.SetActive(false);
        loginBtn.gameObject.SetActive(true);
        logoutBtn.gameObject.SetActive(false);
        switchAccountBtn.gameObject.SetActive(false);

    }

    private void ShowEnterGameBtn()
    {
        selectServerDropdown.ClearOptions();
        GameClient.GetServerList(datas => {
            foreach(var data in datas)
            {
                selectServerDropdown.options.Add(new Dropdown.OptionData(data.serverName));
            }
        });
        selectServerDropdown.value = 0;
        enterGameBtn.gameObject.SetActive(true);
        selectServerDropdown.gameObject.SetActive(true);
        loginBtn.gameObject.SetActive(false);
        logoutBtn.gameObject.SetActive(true);
        switchAccountBtn.gameObject.SetActive(true);
    }

    [EventSystem.BindEvent]
    private void OnLoadGameFinished(GameLoadFinishedEvent evt)
    {
        loginBtn.interactable = true;
        enterGameBtn.interactable = true;
        logoutBtn.interactable = true;
        switchAccountBtn.interactable = true;
    }

    private void CheckAnnouncements(string profile = null, int? level = null)
    {
        var opts = new CheckAnnouncementsOptions();
        if(profile != null)
        {
            opts = new CheckAnnouncementsOptions()
            {
                Profile = profile,
                Level = (int)level
            };
        }
        ComboSDK.CheckAnnouncements(opts, res =>{
            if(res.IsSuccess)
            {
                var image = FindImageByTag(openAnnouncementsBtn.transform, "announcement");
                image.gameObject.SetActive(res.Data.newAnnouncementsAvailable);
            }
            else
            {
                Toast.Show($"检查公告信息失败：{res.Error.Message}");
            }
        });
    }


    private Image FindImageByTag(Transform parent, string tag)
    {
        foreach (Transform child in parent)
        {
            if (child.CompareTag(tag))
            {
                Image image = child.GetComponent<Image>();
                if (image != null)
                {
                    return image;
                }
            }
            Image foundImage = FindImageByTag(child, tag);
            if (foundImage != null)
            {
                return foundImage;
            }
        }
        return null;
    }

}
