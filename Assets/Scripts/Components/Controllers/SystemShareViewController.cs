using System.Collections;
using System.Collections.Generic;
using System.IO;
using Combo;
using UnityEngine;

public static class SystemShareViewController
{
    public static void ShowShareView()
    {
        var shareView = SystemShareView.Instantiate();
        shareView.SetLinkCallback(() => OnLinkShare());
        shareView.SetOnlineCallback(() => OnOnlineImageShare());
        shareView.SetLocalCallback(() => OnLocalImageShare());
        shareView.SetCancelCallback(() => shareView.Destroy());
        shareView.Show();
    }

    public static void HideShareView()
    {
        SystemShareView.DestroyAll();
    }

    public static void OnLinkShare()
    {
        var opts = new SystemShareOptions
        {
           text = "链接分享",
           linkUrl = "https://docs.seayoo.com/",
        };

        Share(opts);
    }

    private static void OnLocalImageShare()
    {
        byte[] bytes = ScreenCapture.CaptureScreenshotAsTexture().EncodeToPNG();
        string path = Path.Combine(Application.temporaryCachePath, "sharePicture.png");
        File.WriteAllBytes(path, bytes);

        var opts = new SystemShareOptions
        {
           imageUrl = path
        };

        Share(opts);
    }

    private static void OnOnlineImageShare()
    {
        var opts = new SystemShareOptions
        {
           text = "网络图片分享",
           imageUrl = "https://cn.bing.com/th?id=OHR.CERNCenter_EN-US9854867489_1920x1080.jpg"
        };

        Share(opts);
    }

    private static void Share(SystemShareOptions opts) 
    {
        ComboSDK.Share(opts, r =>
        {
           if (r.IsSuccess)
           {
               var result = r.Data;
               Toast.Show("分享成功");
               Log.I("分享成功");
           }
           else
           {
               var err = r.Error;
               Toast.Show("分享失败：" + err.Message);
               Log.E("分享失败: " + err.DetailMessage);
           }
        });
    }
}
