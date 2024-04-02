using Combo;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;
#if UNITY_STANDALONE
using _Core.Storage;
#endif
using System.IO;

public class Game : MonoBehaviour
{
    public PlayerPanel playerPanel;

    void Start()
    {
        var info = ComboSDK.GetLoginInfo();
        if (info == null || string.IsNullOrEmpty(info.comboId))
        {
            SceneManager.LoadScene("Login");
            return;
        }
    }

    public void OnSetting()
    {
        SettingViewController.ShowSettingView();
    }

    public void OnShare()
    {
        ShareSelectorViewController.Show();
    }

    public void OnErrorTrack()
    {
        ErrorTrackViewController.ShowDataView();
    }

    public void OnShop()
    {
        UIController.ShowShopView();
    }

    public void OnTask()
    {
        UIController.ShowTaskView();
    }

    public void OnPlayerInfo()
    {
        PlayerInfoViewController.ShowPlayerInfoView();
    }
}
