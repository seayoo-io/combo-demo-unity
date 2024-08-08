using System;
using System.Collections.Generic;
using Combo;
using UnityEngine;
using UnityEngine.UI;

public enum ShareType
{
    Image,
    Video,
    Link
}
public class SharePlatformViewController : MonoBehaviour
{
    public Button systemBtn;
    public Button taptapBtn;
    public Button agoraBtn;
    public Button weixinSessionBtn;
    public Button weixinTimelineBtn;
    public Button weixinFavoriteBtn;
    public Button weiboBtn;
    public Button douyinEditBtn;
    public Button douyinPublishBtn;
    public Button douyinContactsBtn;
    public Button[] buttons = new Button[] {};
    private ImageShareOptions imageShareOptions;
    private VideoShareOptions videoShareOptions;
    private LinkShareOptions linkShareOptions;
    private ShareType shareType = ShareType.Image;
    void Awake()
    {
        EventSystem.Register(this);
    }

    void Start()
    {
        SetSharePlatformButtonActive();
        systemBtn.onClick.AddListener(() => Share(ShareTarget.SYSTEM));
        taptapBtn.onClick.AddListener(() => Share(ShareTarget.TAPTAP));
        agoraBtn.onClick.AddListener(() => Share(ShareTarget.AGORA));
        weixinSessionBtn.onClick.AddListener(() => Share(ShareTarget.WEIXIN, ShareScene.WEIXIN_SESSION));
        weixinTimelineBtn.onClick.AddListener(() => Share(ShareTarget.WEIXIN, ShareScene.WEIXIN_TIMELINE));
        weixinFavoriteBtn.onClick.AddListener(() => Share(ShareTarget.WEIXIN, ShareScene.WEIXIN_FAVORITE));
        weiboBtn.onClick.AddListener(() => Share(ShareTarget.WEIBO));
        douyinEditBtn.onClick.AddListener(() => Share(ShareTarget.DOUYIN, ShareScene.DOUYIN_EDIT));
        douyinPublishBtn.onClick.AddListener(() => Share(ShareTarget.DOUYIN, ShareScene.DOUYIN_PUBLISH));
        douyinContactsBtn.onClick.AddListener(() => Share(ShareTarget.DOUYIN, ShareScene.DOUYIN_CONTACTS));
    }

    void OnDestroy()
    {
        systemBtn.onClick.RemoveListener(() => Share(ShareTarget.SYSTEM));
        taptapBtn.onClick.RemoveListener(() => Share(ShareTarget.TAPTAP));
        agoraBtn.onClick.RemoveListener(() => Share(ShareTarget.AGORA));
        weixinSessionBtn.onClick.RemoveListener(() => Share(ShareTarget.WEIXIN, ShareScene.WEIXIN_SESSION));
        weixinTimelineBtn.onClick.RemoveListener(() => Share(ShareTarget.WEIXIN, ShareScene.WEIXIN_TIMELINE));
        weixinFavoriteBtn.onClick.RemoveListener(() => Share(ShareTarget.WEIXIN, ShareScene.WEIXIN_FAVORITE));
        weiboBtn.onClick.RemoveListener(() => Share(ShareTarget.WEIBO));
        douyinEditBtn.onClick.RemoveListener(() => Share(ShareTarget.DOUYIN, ShareScene.DOUYIN_EDIT));
        douyinPublishBtn.onClick.RemoveListener(() => Share(ShareTarget.DOUYIN, ShareScene.DOUYIN_PUBLISH));
        douyinContactsBtn.onClick.RemoveListener(() => Share(ShareTarget.DOUYIN, ShareScene.DOUYIN_CONTACTS));
        EventSystem.UnRegister(this);
    }

    [EventSystem.BindEvent]
    public void ImgShareContent(ImgShareEvent evt)
    {
        shareType = ShareType.Image;
        imageShareOptions = new ImageShareOptions {
            Title = evt.title,
            Text = evt.text,
            ImageUrl = evt.imageUrl,
            Hashtag = evt.hashtag,
        };
    }

    [EventSystem.BindEvent]
    public void VideoShareContent(VideoShareEvent evt)
    {
        shareType = ShareType.Video;
        videoShareOptions = new VideoShareOptions {
            Title = evt.title,
            Text = evt.text,
            VideoUrl = evt.videoUrl,
            VideoCoverUrl = evt.videoCoverUrl,
            Hashtag = evt.hashtag,
        };
    }

    [EventSystem.BindEvent]
    public void LinkShareContent(LinkShareEvent evt)
    {
        shareType = ShareType.Link;
        linkShareOptions = new LinkShareOptions {
            Title = evt.title,
            Text = evt.text,
            LinkUrl = evt.linkUrl,
            LinkCoverUrl = evt.linkCoverUrl,
        };
    }

    private void Share(ShareTarget shareTarget, ShareScene? shareScene = null)
    {
        ShareOptions opts = null;
        switch (shareType)
        {
            case ShareType.Image:
                if(shareScene == null)
                {
                    opts = new ImageShareOptions { 
                        Target = shareTarget,
                        Title = imageShareOptions.Title,
                        Text = imageShareOptions.Text,
                        ImageUrl = imageShareOptions.ImageUrl,
                        Hashtag = imageShareOptions.Hashtag,
                    };
                }
                else
                {
                    opts = new ImageShareOptions { 
                        Target = shareTarget,
                        Title = imageShareOptions.Title,
                        Text = imageShareOptions.Text,
                        ImageUrl = imageShareOptions.ImageUrl,
                        Hashtag = imageShareOptions.Hashtag,
                        Scene = (ShareScene)shareScene,
                    };
                }
                break;
            case ShareType.Video:
                if(shareScene == null)
                {
                    opts = new VideoShareOptions { 
                        Target = shareTarget,
                        Title = videoShareOptions.Title,
                        Text = videoShareOptions.Text,
                        VideoUrl = videoShareOptions.VideoUrl,
                        VideoCoverUrl = videoShareOptions.VideoCoverUrl,
                        Hashtag = videoShareOptions.Hashtag,
                    };
                }
                else
                {
                    string videoUrl = videoShareOptions.VideoUrl;
                    if(shareTarget == ShareTarget.WEIXIN)
                    {
                        videoUrl = "https://media.w3.org/2010/05/sintel/trailer.mp4";
                    }
                    opts = new VideoShareOptions { 
                        Target = shareTarget,
                        Title = videoShareOptions.Title,
                        Text = videoShareOptions.Text,
                        VideoUrl = videoUrl,
                        VideoCoverUrl = videoShareOptions.VideoCoverUrl,
                        Hashtag = videoShareOptions.Hashtag,
                        Scene = (ShareScene)shareScene,
                    };
                }
                break;
            case ShareType.Link:
                if(shareScene == null)
                {
                    opts = new LinkShareOptions { 
                        Target = shareTarget,
                        Title = linkShareOptions.Title,
                        Text = linkShareOptions.Text,
                        LinkUrl = linkShareOptions.LinkUrl,
                        LinkCoverUrl = linkShareOptions.LinkCoverUrl,
                    };
                }
                else
                {
                    opts = new LinkShareOptions { 
                        Target = shareTarget,
                        Title = linkShareOptions.Title,
                        Text = linkShareOptions.Text,
                        LinkUrl = linkShareOptions.LinkUrl,
                        LinkCoverUrl = linkShareOptions.LinkCoverUrl,
                        Scene = (ShareScene)shareScene,
                    };
                }
                break;
            default:
                break;
        }
        
        if (opts != null)
        {
            ComboSDK.Share(opts, result =>
            {
                if(result.IsSuccess)
                {
                    Toast.Show("分享成功");
                    Log.I("分享成功");
                }
                else
                {   
                    var err = result.Error;
                    Toast.Show("分享失败：" + err.Message);
                    Log.E("分享失败: " + err.DetailMessage);
                }
            });
        }
    }

    private void SetSharePlatformButtonActive()
    {
        var dictionary = new Dictionary<ShareTarget, Button>(){
            {ShareTarget.SYSTEM, buttons[0]},
            {ShareTarget.TAPTAP, buttons[1]},
            {ShareTarget.AGORA, buttons[2]},
            {ShareTarget.WEIXIN, buttons[3]},
            {ShareTarget.WEIBO, buttons[4]},
            {ShareTarget.DOUYIN, buttons[5]},
        };
        var shareTargets = ComboSDK.GetAvailableShareTargets();
        foreach(ShareTarget shareTarget in shareTargets)
        {
            Button button;
            dictionary.TryGetValue(shareTarget, out button);
            if (button != null)
            {
                button.gameObject.SetActive(true);
            }
        }
    }
}