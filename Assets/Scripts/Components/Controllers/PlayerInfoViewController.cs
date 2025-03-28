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
        string playerId;
        string seayooId;
        if (ComboSDK.IsFeatureAvailable(Feature.SEAYOO_ACCOUNT))
        {
            playerId = ComboSDK.GetLoginInfo().comboId;
            seayooId = ComboSDK.SeayooAccount.UserId;
            view.manageAccountBtn.gameObject.SetActive(true);
            view.changePasswordBtn.gameObject.SetActive(true);
            view.deleteAccountBtn.gameObject.SetActive(true);
            Log.I($"GetUserInfo: 世游通行证 ID : = {seayooId}, Combo ID : = {playerId}");
        }
        else
        {
            var info = ComboSDK.GetLoginInfo();
            playerId = info.comboId;
            seayooId = "无";
            Log.I($"GetUserInfo: Combo ID : = {playerId}," + $"identityToken = {info.identityToken}");
        }

        if (!ComboSDK.IsFeatureAvailable(Feature.CONTACT_SUPPORT))
        {
            view.contactSupportBtn.gameObject.SetActive(false);
        }

        view.SetPlayerId(playerId);
        view.SetSeayooId(seayooId);
        view.SetIdp($"idp : {ComboSDK.GetLoginInfo().idp}");
        view.SetRole(PlayerController.GetPlayer().role);
        view.SetServer(GameManager.Instance.ZoneName, GameManager.Instance.ServerName);
        
    }

}
