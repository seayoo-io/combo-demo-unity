using System.Collections;
using System.Collections.Generic;
using Combo;
using UnityEditor;
using UnityEngine;

public static class PlayerInfoViewController
{
    public static void ShowPlayerInfoView()
    {
        var playerInfoView = PlayerInfoView.Instantiate();
        InitView(playerInfoView);
        playerInfoView.SetCopyCallback(() => OnCopy());
        playerInfoView.SetManageAccountCallback(() => OnManageAccount());
        playerInfoView.SetChangePasswordCallback(() => OnChangePassword());
        playerInfoView.SetDeleteAccountCallback(() => OnDeleteAccount());
        playerInfoView.SetOnContactSupportCallback(() => OnContactSupport());
        playerInfoView.SetCancelCallback(() => playerInfoView.Destroy());

        playerInfoView.Show();
    }

    public static void HidePlayerInfoView()
    {
        PlayerInfoView.DestroyAll();
    }

    public static void OnCopy()
    {
        string playerId;
         if (ComboSDK.IsFeatureAvailable(Feature.SEAYOO_ACCOUNT))
        {
            playerId = ComboSDK.SeayooAccount.UserId;
        }
        else
        {
            playerId = ComboSDK.GetLoginInfo().comboId;
        }

        UnityEngine.GUIUtility.systemCopyBuffer = playerId;
        Toast.Show("复制成功");
    }

    public static void OnManageAccount()
    {
        ComboSDK.SeayooAccount.ManageAccount();
    }

    public static void OnChangePassword()
    {
        ComboSDK.SeayooAccount.ChangePassword();
    }

    public static void OnDeleteAccount()
    {
        ComboSDK.SeayooAccount.DeleteAccount();
    }

    public static void OnContactSupport()
    {
        ComboSDK.ContactSupport();
    }

    private static void InitView(PlayerInfoView view)
    {
        string accountName;
        string playerId;
        if (ComboSDK.IsFeatureAvailable(Feature.SEAYOO_ACCOUNT))
        {
            accountName = "世游通行证 ID :";
            playerId = ComboSDK.SeayooAccount.UserId;
            view.manageAccountBtn.gameObject.SetActive(true);
            view.changePasswordBtn.gameObject.SetActive(true);
            view.deleteAccountBtn.gameObject.SetActive(true);
            Log.I($"GetUserInfo: ${accountName} = {playerId}");
        }
        else
        {
            var info = ComboSDK.GetLoginInfo();
            playerId = info.comboId;
            accountName = "Combo ID :";
            Log.I($"GetUserInfo: ${accountName} = {playerId}," + $"identityToken = {info.identityToken}");
        }

        if (!ComboSDK.IsFeatureAvailable(Feature.CONTACT_SUPPORT))
        {
            view.contactSupportBtn.gameObject.SetActive(false);
        }

        view.SetPlayerId(playerId);
        view.SetAccountName(accountName);
        view.SetRole(PlayerController.GetPlayer().role);
        view.SetServer(GameManager.Instance.ZoneName, GameManager.Instance.ServerName);
        
    }

}
