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
    public Button openAnnouncementsBtn;

    void Start()
    {
        EventSystem.Register(this);
        var info = ComboSDK.GetLoginInfo();
        if (info == null || string.IsNullOrEmpty(info.comboId))
        {
            SceneManager.LoadScene("Login");
            return;
        }
        CheckAnnouncements(PlayerController.GetPlayer().role.roleId, PlayerController.GetPlayer().role.roleLevel);
    }

    void OnDestroy()
    {
        EventSystem.UnRegister(this);
    }

    public void OnSetting()
    {
        SettingViewController.ShowSettingView();
    }

    public void OnShare()
    {
        ShareContentViewController.ShowShareContentView();
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

    public void OpenAnnouncement()
    {
        UIController.ShowAnnouncementParameterView(true);
    }

    [EventSystem.BindEvent]
    void OpenAnnouncement(OpenAnnouncementsEvent evt)
    {
        var image = FindImageByTag(openAnnouncementsBtn.transform, "announcement");
        image.gameObject.SetActive(false);
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
