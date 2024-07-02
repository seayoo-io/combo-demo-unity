using UnityEngine;
using UnityEngine.UI;

public static class ShareContentViewController
{
    public static void ShowShareContentView()
    {
        ShareContentView.DestroyAll();
        var shareContentView = ShareContentView.Instantiate();
        shareContentView.SetCloseCallback(() => shareContentView.Destroy());
        shareContentView.SetImgShareOptionCallback(data => OpenImgPlatformShareView(data));
        shareContentView.SetVideoShareOptionCallback(data => OpenVideoPlatformShareView(data));
        shareContentView.SetLinkShareOptionCallback(data => OpenLinkPlatformShareView(data));
        shareContentView.Show();
    }

    public static void HideShareContentView()
    {
        ShareContentView.DestroyAll();
    }

    private static void OpenImgPlatformShareView(ImgShareOptionViewModel model)
    {
        SharePlatformView.DestroyAll();
        var sharePlatformView = SharePlatformView.Instantiate();
        sharePlatformView.SetCloseCallback(() => sharePlatformView.Destroy());
        sharePlatformView.Show();
        ImgShareEvent.Invoke(new ImgShareEvent {
            title = model.title,
            contents = model.content,
            imageUrl = model.imageUrl,
            hashtag = model.hashtag,
        });
        HideShareContentView();
    }

    private static void OpenVideoPlatformShareView(VideoShareOptionViewModel model)
    {
        SharePlatformView.DestroyAll();
        var sharePlatformView = SharePlatformView.Instantiate();
        sharePlatformView.SetCloseCallback(() => sharePlatformView.Destroy());
        sharePlatformView.Show();
        VideoShareEvent.Invoke(new VideoShareEvent {
            title = model.title,
            contents = model.content,
            videoUrl = model.videoUrl,
            videoCoverUrl = model.videoCoverUrl,
            hashtag = model.hashtag,
        });
        HideShareContentView();
    }

    private static void OpenLinkPlatformShareView(LinkShareOptionViewModel model)
    {
        SharePlatformView.DestroyAll();
        var sharePlatformView = SharePlatformView.Instantiate();
        sharePlatformView.SetCloseCallback(() => sharePlatformView.Destroy());
        sharePlatformView.Show();
        LinkShareEvent.Invoke(new LinkShareEvent {
            title = model.title,
            contents = model.contents,
            linkUrl = model.linkUrl,
            linkCoverUrl = model.linkCoverUrl,
        });
        HideShareContentView();
    }
}