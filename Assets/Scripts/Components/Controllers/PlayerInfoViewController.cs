using System.Collections;
using System.Collections.Generic;
using Combo;
using UnityEditor;
using UnityEngine;

public static class PlayerInfoViewController
{
    public static void ShowPlayerInfoView(string playerId)
    {
        var playerInfoView = PlayerInfoView.Instantiate();
        playerInfoView.SetPlayerId(playerId);
        playerInfoView.SetCopyCallback(() => OnCopy());
        playerInfoView.SetAccountSettingsCallback(() => OnAccountSettings());
        playerInfoView.SetDeleteAccountCallback(() => OnDeleteAccount());
        playerInfoView.SetOnCustomerServiceCallback(() => OnCustomerService());
        playerInfoView.SetCancelCallback(() => playerInfoView.Destroy());
        playerInfoView.Show();
        if(!ComboSDK.IsFeatureAvailable(Feature.ACCOUNT_SETTINGS)){
            playerInfoView.accountSettingsBtn.gameObject.SetActive(false);
        }
        if(!ComboSDK.IsFeatureAvailable(Feature.DELETE_ACCOUNT)){
            playerInfoView.deleteAccountBtn.gameObject.SetActive(false);
        }
        if(!ComboSDK.IsFeatureAvailable(Feature.CUSTOMER_SERVICE)){
            playerInfoView.customerServiceBtn.gameObject.SetActive(false);
        }
    }

    public static void HidePlayerInfoView()
    {
        PlayerInfoView.DestroyAll();
    }

    public static void OnCopy()
    {
        var info = ComboSDK.GetLoginInfo();
        if (info == null || string.IsNullOrEmpty(info.comboId))
        {
            Toast.Show("用户未登录");
            return;
        }
        UnityEngine.GUIUtility.systemCopyBuffer = info.comboId;
        Toast.Show("复制成功");
    }

    public static void OnAccountSettings()
    {
        ComboSDK.AccountSettings();
    }

    public static void OnDeleteAccount()
    {
        ComboSDK.DeleteAccount();
    }

    public static void OnCustomerService()
    {
        ComboSDK.CustomerService();
    }

}
