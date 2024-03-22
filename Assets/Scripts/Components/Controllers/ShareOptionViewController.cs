using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Combo;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ShareOptionViewController
{
    public static void Show(ShareTarget shareTarget)
    {
        var view = ShareOptionView.Instantiate();
        switch(shareTarget) {
            case ShareTarget.SYSTEM: {
                view.SetTitleLabel("文本内容");
                view.SetContentLabel("网络链接");
                view.SetImageTypeEnabled(true);
                view.SetMultiImageEnabled(false);
            };
            break;
            case ShareTarget.TAPTAP: {
                view.SetTitleLabel("标题");
                view.SetContentLabel("文本");
                view.SetImageTypeEnabled(false);
                view.SetMultiImageEnabled(true);
            };
            break;
        }

        view.SetSubmitCallback(data => Share(shareTarget, data));

        view.SetCancelCallback(() => view.Destroy());

        view.Show(new ViewCallbacks { AfterHide = () => view.Destroy() });
    }
    
    private static void Share(ShareTarget shareTarget, ShareOptionViewModel model) {
        ShareOptions opts = null;
        switch(shareTarget) {
            case ShareTarget.SYSTEM: {
                opts = new SystemShareOptions {
                    text = model.title,
                    linkUrl = model.content,
                    imageUrl = model.imageUrls.FirstOrDefault()
                };
            };
            break;
            case ShareTarget.TAPTAP: {
                opts = new TapTapShareOptions {
                    title = model.title,
                    contents = model.content,
                    imageUriList = model.imageUrls
                };
            };
            break;
        }

        Log.D($"Share type: {opts.GetType()} value: " + opts.ToJson());

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
