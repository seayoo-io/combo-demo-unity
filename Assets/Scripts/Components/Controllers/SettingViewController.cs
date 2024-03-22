using System;
using System.Collections;
using System.Collections.Generic;
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
}
