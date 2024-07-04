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

    public static void ShowAnnouncementParameterView(bool isLogin)
    {
        AnnouncementParameterView.DestroyAll();
        var view = AnnouncementParameterView.Instantiate();
        view.SetIsLogin(isLogin);
        view.Show();
    }
}
