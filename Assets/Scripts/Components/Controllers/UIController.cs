using System;
using System.Collections.Generic;

public static class UIController
{
    public static void Alert(UIAlertType uIAlertType, string title, string message, string confirmBtnTxt, Action OnConfirm) {
        if (uIAlertType == UIAlertType.Singleton) {
            AlertView.DestroyAll();
        }
        var alertView = AlertView.Instantiate();
        alertView.SetTitle(title);
        alertView.SetMessage(message);
        alertView.SetConfirmBtnTitle(confirmBtnTxt);
        alertView.SetConfirmCallback(OnConfirm);
        alertView.HideCancelBtn();
        alertView.SetConfirmCallback(() => {
            alertView.Destroy();
            OnConfirm.Invoke();
        });
        alertView.Show();
    }

    public static void Alert(UIAlertType uIAlertType, string title, string message, string confirmBtnTxt, string cancelBtnTxt, Action OnConfirm, Action OnCancel) {
        if (uIAlertType == UIAlertType.Singleton) {
            AlertView.DestroyAll();
        }
        var alertView = AlertView.Instantiate();
        alertView.SetTitle(title);
        alertView.SetMessage(message);
        alertView.SetConfirmBtnTitle(confirmBtnTxt);
        alertView.SetCancelBtnTitle(cancelBtnTxt);
        alertView.SetConfirmCallback(() => {
            alertView.Destroy();
            OnConfirm.Invoke();
        });
        alertView.SetCancelCallback(() => {   
            alertView.Destroy();
            OnCancel.Invoke();
        });
        alertView.Show();
    }
    public static void ShowLoading() {
        LoadingView.DestroyAll();
        var view = LoadingView.Instantiate();
        view.Show();
    }

    public static void HideLoading() {
        LoadingView.DestroyAll();
    }

    public static void ShowShopView()
    {
        ShopView.DestroyAll();
        var shopView = ShopView.Instantiate();
        shopView.SetGoHomeCallback(() => {
            shopView.Destroy();
        });
        shopView.Show();
    }

    public static void HideShopView()
    {
        ShopView.DestroyAll();
    }

    public static void ShowRankView()
    {
        RankView.DestroyAll();
        var rankView = RankView.Instantiate();
        rankView.SetGoHomeCallback(() => {
            rankView.Destroy();
        });
        rankView.Show();
    }

    public static void ShowTaskView()
    {
        TaskView.DestroyAll();
        var taskView = TaskView.Instantiate();
        taskView.SetCancelCallback(() => taskView.Destroy());
        taskView.Show();
    }

    public static void HideTaskView()
    {
        TaskView.DestroyAll();
    }

    public static void ShowShortLinkView(){
        ShortLinkView.DestroyAll();
        var shortLinkView = ShortLinkView.Instantiate();
        shortLinkView.SetCancelCallback(() => shortLinkView.Destroy());
        shortLinkView.Show();
    }

    public static void HideShortLinkView()
    {
        ShortLinkView.DestroyAll();
    }


    public static void ShowAnnouncementParameterView(bool isLogin)
    {
        WebViewParameterView.DestroyAll();
        var view = WebViewParameterView.Instantiate();
        view.SetAnnounceInfoLogin(isLogin, "打开公告", WebViewType.ANNOUNCEMENT);
        view.Show();
    }

    public static void ShowRedeemGiftCodeView()
    {
        WebViewParameterView.DestroyAll();
        var view = WebViewParameterView.Instantiate();
        view.SetGift("兑换/打开页面", "礼包码", WebViewType.GIFT);
    }
}
