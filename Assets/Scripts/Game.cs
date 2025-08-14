using Combo;
using System.Collections.Generic;
using System.Linq;
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
    public Button getAnnouncementsBtn;
    public Image roleImage;
    public Transform leftControlPanel;
    public Transform rightControlPanel;

    void Start()
    {
        EventSystem.Register(this);
        var info = ComboSDK.GetLoginInfo();
        if (info == null || string.IsNullOrEmpty(info.comboId))
        {
            Log.E("Enter Game Failed: loginInfo = null");
            SceneManager.LoadScene("Login");
            return;
        }
        var role = PlayerController.GetRoleInfo(PlayerController.GetPlayer());
        Sprite sprite;
        GameManager.Instance.RoleDic.TryGetValue(role.type, out sprite);
        roleImage.sprite = sprite;
        CheckAnnouncements(role.roleId, role.roleLevel);
        // SetupBtnStatus();
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

    public void OnRank()
    {
        UIController.ShowRankView();
    }

    public void OnTask()
    {
        UIController.ShowTaskView();
    }

    public void OnRedeemGiftCode()
    {
        UIController.ShowRedeemGiftCodeView();
    }

    public void OnPlayerInfo()
    {
        PlayerInfoViewController.ShowPlayerInfoView();
    }

    public void OpenAnnouncement()
    {
        UIController.ShowAnnouncementParameterView(true);
    }

    // 使用 GetAnnouncements 方案打开公告（真实数据），供 Scene 中按钮直接绑定
    public void GetAnnouncementsMock()
    {
        var role = PlayerController.GetRoleInfo(PlayerController.GetPlayer());
        var opts = new GetAnnouncementsOptions
        {
            Profile = role.roleId,
            Level = role.roleLevel,
        };

        UIController.ShowLoading();
        ComboSDK.GetAnnouncements(opts, result =>
        {
            UIController.HideLoading();
            if (result.IsSuccess)
            {
                var items = result.Data.Announcements;
                Log.D($"[GetAnnouncements] count={items.Count}");
                foreach (var a in items)
                    Log.D($"[GetAnnouncements] id={a.Id} title={a.Title} subtitle={a.Subtitle} format={a.Format} lang={a.Lang} publishedAt={a.PublishedAt} content={a.Content}");

                var announcements = items
                    .Select(a => new AnnouncementData
                    {
                        Id = a.Id,
                        Title = a.Title,
                        Subtitle = a.Subtitle,
                        Content = a.Content,
                        Format = a.Format,
                        Lang = a.Lang,
                        PublishedAt = a.PublishedAt,
                    })
                    .ToList();
                UIController.ShowAnnouncementView(announcements);
                var openAnnoImage = FindImageByTag(openAnnouncementsBtn.transform, "announcement");
                if (openAnnoImage != null) openAnnoImage.gameObject.SetActive(false);
                var getAnnoImage = FindImageByTag(getAnnouncementsBtn.transform, "announcement");
                if (getAnnoImage != null) getAnnoImage.gameObject.SetActive(false);
            }
            else
            {
                Toast.Show($"获取公告失败：{result.Error.Message}");
            }
        });
    }

    // 使用 Mock 数据打开公告，用于本地测试（保留，不删除）
    public void GetAnnouncementsWithMockData()
    {
        UIController.ShowAnnouncementView();
    }

    public void OpenStatusView()
    {
        StatusManager.OpenStatusView();
    }

    public void OpenMailView()
    {
        MailView.Instantiate();
    }

    [EventSystem.BindEvent]
    void OpenAnnouncement(OpenAnnouncementsEvent evt)
    {
        var image = FindImageByTag(openAnnouncementsBtn.transform, "announcement");
        image.gameObject.SetActive(false);
        var getAnnoImage = FindImageByTag(getAnnouncementsBtn.transform, "announcement");
        if (getAnnoImage != null)
            getAnnoImage.gameObject.SetActive(false);
    }

    [EventSystem.BindEvent]
    void ChangeRole(ChangeRoleEvent evt)
    {
        Sprite sprite;
        GameManager.Instance.RoleDic.TryGetValue(evt.role.type, out sprite);
        roleImage.sprite = sprite;
        CheckAnnouncements(evt.role.roleId, evt.role.roleLevel);
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
                var getAnnoImage = FindImageByTag(getAnnouncementsBtn.transform, "announcement");
                if (getAnnoImage != null)
                    getAnnoImage.gameObject.SetActive(res.Data.newAnnouncementsAvailable);
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

    private void SetupBtnStatus()
    {
        Button shareBtn = leftControlPanel.Find("share_btn")?.GetComponent<Button>();
        if (shareBtn != null)
        {
            shareBtn.gameObject.SetActive(GameManager.Instance.sdkConfig.supportShare);
        }
        Button taskBtn = rightControlPanel.Find("task_btn")?.GetComponent<Button>();
        if (taskBtn != null)
        {
            taskBtn.gameObject.SetActive(GameManager.Instance.sdkConfig.supportAds);
        }
    }
}
