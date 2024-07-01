using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

internal class ImgShareOptionViewModel {
    public string title;
    public string content;
    public string imageUrl;
    public string hashtag;
}

internal class VideoShareOptionViewModel {
    public string title;
    public string content;
    public string videoUrl;
    public string videoCoverUrl;
    public string hashtag;
}

internal class TextShareOptionViewModel {
    public string title;
    public string contents;
}

internal class LinkShareOptionViewModel {
    public string title;
    public string contents;
    public string linkUrl;
    public string linkCoverUrl;
}

[ViewPrefab("Prefabs/ShareContentOptionView")]
internal class ShareContentView : View<ShareContentView>
{
    public Button closeBtn;
    public Button imgSelectPlatformBtn;
    public Button videoSelectPlatformBtn;
    public Button textSelectPlatformBtn;
    public Button linkSelectPlatformBtn;
    public InputField imgTitle;
    public InputField imgContent;
    public InputField imgHashtag;
    public InputField videoTitle;
    public InputField videoContent;
    public InputField videoHashtag;
    public InputField textTitle;
    public InputField textContent;
    public InputField linkTitle;
    public InputField linkContent;
    private Action OnClose;
    private Action<ImgShareOptionViewModel> OnImgSelectPlatform;
    private Action<VideoShareOptionViewModel> OnVideoSelectPlatform;
    private Action<TextShareOptionViewModel> OnTextSelectPlatform;
    public Action<LinkShareOptionViewModel> OnLinkSelectPlatform;
    private string localVideoUrl;

    void Awake()
    {
        closeBtn.onClick.AddListener(OnCloseBtn);
        imgSelectPlatformBtn.onClick.AddListener(()=>{
            OnImgSelectPlatform?.Invoke(new ImgShareOptionViewModel {
                title = imgTitle.text,
                content = imgContent.text,
                imageUrl = GenerateImagePath(),
                hashtag = imgHashtag.text
            });
        });
        videoSelectPlatformBtn.onClick.AddListener(()=>{
            if(!File.Exists(localVideoUrl))
            {
                Toast.Show("视频文件未下载完成，请稍后");
                return;
            }
            Texture2D texture = Resources.Load<Texture2D>("Textures/coverImg");
            string coverUrl = Path.Combine(Application.persistentDataPath, $"coverImg.jpg");
            byte[] bytes = texture.EncodeToPNG();
            File.WriteAllBytes(coverUrl, bytes);
            OnVideoSelectPlatform?.Invoke(new VideoShareOptionViewModel {
                title = videoTitle.text,
                content = videoContent.text,
                videoUrl = localVideoUrl,
                videoCoverUrl = coverUrl,
                hashtag = videoHashtag.text
            });
        });
        textSelectPlatformBtn.onClick.AddListener(()=>{
            OnTextSelectPlatform?.Invoke(new TextShareOptionViewModel {
                title = textTitle.text,
                contents = textContent.text,
            });
        });
        linkSelectPlatformBtn.onClick.AddListener(()=> {
            Texture2D texture = Resources.Load<Texture2D>("Textures/coverImg");
            string coverUrl = Path.Combine(Application.persistentDataPath, $"coverImg.jpg");
            byte[] bytes = texture.EncodeToPNG();
            File.WriteAllBytes(coverUrl, bytes);
            OnLinkSelectPlatform?.Invoke(new LinkShareOptionViewModel {
                title = linkTitle.text,
                contents = linkContent.text,
                linkUrl = "https://catsnsoup.seayoo.com/",
                linkCoverUrl = coverUrl
            });
        });
    }

    void Start()
    {
        if (!File.Exists(localVideoUrl))
        {
            localVideoUrl = Path.Combine(Application.temporaryCachePath, $"shareVideo.mp4");
            StartCoroutine(DownloadVideo(localVideoUrl));
        }
    }

    void OnDestroy()
    {
        closeBtn.onClick.RemoveListener(OnCloseBtn);
        imgSelectPlatformBtn.onClick.RemoveAllListeners();
        videoSelectPlatformBtn.onClick.RemoveAllListeners();
    }

    public void SetImgShareOptionCallback(Action<ImgShareOptionViewModel> OnImgSelectPlatform) => this.OnImgSelectPlatform = OnImgSelectPlatform;

    public void SetVideoShareOptionCallback(Action<VideoShareOptionViewModel> OnVideoSelectPlatform) => this.OnVideoSelectPlatform = OnVideoSelectPlatform;

    public void SetTextShareOptionCallback(Action<TextShareOptionViewModel> OnTextSelectPlatform) => this.OnTextSelectPlatform = OnTextSelectPlatform;

    public void SetLinkShareOptionCallback(Action<LinkShareOptionViewModel> OnLinkSelectPlatform) => this.OnLinkSelectPlatform = OnLinkSelectPlatform;
    
    void OnCloseBtn()
    {
        OnClose.Invoke();
    }

    public void SetCloseCallback(Action OnClose)
    {
        this.OnClose = OnClose;
    }

    private string GenerateImagePath() {
        var bytes = ScreenCapture.CaptureScreenshotAsTexture().EncodeToPNG();
        var path = Path.Combine(Application.temporaryCachePath, $"share.png");
        File.WriteAllBytes(path, bytes);
        return path;
    }

    public IEnumerator DownloadVideo(string filePath)
    {
        var videoUrl = "https://media.w3.org/2010/05/sintel/trailer.mp4";
        using (UnityWebRequest www = UnityWebRequest.Get(videoUrl))
        {
            yield return www.SendWebRequest();
            byte[] videoData = www.downloadHandler.data;
            SaveVideoToFile(videoData, filePath);
        }
    }

    private void SaveVideoToFile(byte[] videoData, string filePath)
    {
        File.WriteAllBytes(filePath, videoData);
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