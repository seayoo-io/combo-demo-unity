using Combo;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SettingViewController
{
    public static void ShowSettingView()
    {
        var settingView = SettingView.Instantiate();
        settingView.SetLogoutCallback(() => OnLogout());
        settingView.SetClearCallback(() => OnClearCache());
        settingView.SetCancelCallback(() => settingView.Destroy());
        settingView.SetAppSettingsCallback(() => OnAppSettings());
        settingView.SetPlayerAgreementCallback(() => OnOpenGameUrl(GameUrl.USER_AGREEMENT));
        settingView.SetPrivacyPolicyCallback(() => OnOpenGameUrl(GameUrl.PRIVACY_POLICY));
        settingView.SetPrivacyChildrenCallback(() => OnOpenGameUrl(GameUrl.PRIVACY_CHILDREN));
        settingView.SetThirdPartyCallback(() => OnOpenGameUrl(GameUrl.THIRD_PARTY));
        settingView.SetFangchenmiCallback(() => OnOpenGameUrl(GameUrl.FANGCHENMI));
        settingView.SetResetGuestCallback(() => OnResetGuest());
        settingView.Show();
    }

    public static void OnLogout()
    {
        ComboSDK.Logout(r =>
        {
            if (r.IsSuccess)
            {
                var result = r.Data;
                if (result == null || string.IsNullOrEmpty(result.comboId))
                {
                    Toast.Show($"用户未登录");
                    return;
                }
                Toast.Show($"用户 {result.comboId} 登出成功");
                Log.I($"登出成功: UserId - {result.comboId}");
                GameClient.Logout();
                SettingView.DestroyAll();
                SceneManager.LoadScene("Login");
            }
            else
            {
                var err = r.Error;
                Toast.Show($"登出失败：{err.Message}");
                Log.E("登出失败: " + err.DetailMessage);
            }
        });
    }

    public static void OnClearCache()
    {
        #if UNITY_STANDALONE
        _Core.Storage.UserPrefs.DeleteAll();
        // config cache dir
        var configCache = System.IO.Path.Combine(Application.temporaryCachePath, "com.seayoo.combosdk");
        if (System.IO.Directory.Exists(configCache))
        {
            System.IO.Directory.Delete(configCache, true);
        }
        Toast.Show("清理成功");
        #endif
    }

    public static void OnAppSettings()
    {
        ComboSDK.OpenAppSettings();
    }

    public static void OnOpenGameUrl(GameUrl gameUrl)
    {
        ComboSDK.OpenGameUrl(gameUrl);
    }

    public static void OnResetGuest()
    {
        var isGuestAccountReset = ComboSDK.ResetGuest();
        if (isGuestAccountReset) 
        {
            Toast.Show("已抹除当前游客账号");
            OnLogout();
        }
        else {
            Toast.Show("不存在游客账号");
        }
    }
}
