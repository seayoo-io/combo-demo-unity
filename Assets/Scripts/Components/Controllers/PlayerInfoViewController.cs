using System.Collections;
using System.Collections.Generic;
using Combo;
using UnityEditor;
using UnityEngine;

public static class PlayerInfoViewController
{
    public static void ShowPlayerInfoView()
    {
        Log.I("开始打开个人中心");
        var playerInfoView = PlayerInfoView.Instantiate();
        if (playerInfoView == null)
        {
            Log.E("打开个人中心失败：页面实例化失败");
            return;
        }

        playerInfoView.SetCopyCallback(() => OnCopy());
        playerInfoView.SetCopyComboIdCallback(() => OnCopyComboId());
        playerInfoView.SetManageAccountCallback(() => OnManageAccount());
        playerInfoView.SetChangePasswordCallback(() => OnChangePassword());
        playerInfoView.SetDeleteAccountCallback(() => OnDeleteAccount());
        playerInfoView.SetOnContactSupportCallback(() => OnContactSupport());
        playerInfoView.SetCancelCallback(() => playerInfoView.Destroy());

        try
        {
            if (!TryInitView(playerInfoView))
            {
                playerInfoView.Destroy();
                Toast.Show("个人中心数据无效，请重新进入游戏");
                return;
            }

            playerInfoView.Show();
            Log.I("个人中心打开成功");
        }
        catch (System.Exception exception)
        {
            Log.E($"个人中心初始化失败：exceptionType={exception.GetType().Name}");
            playerInfoView.Destroy();
            Toast.Show("个人中心打开失败，请稍后重试");
        }
    }

    public static void HidePlayerInfoView()
    {
        PlayerInfoView.DestroyAll();
    }

    public static void OnCopy()
    {
        string playerId;
        playerId = ComboSDK.SeayooAccount.UserId;

        UnityEngine.GUIUtility.systemCopyBuffer = playerId;
        Toast.Show("复制成功");
    }
    
    public static void OnCopyComboId()
    {
        var loginInfo = ComboSDK.GetLoginInfo();
        if (loginInfo == null)
        {
            Log.E("复制 Combo ID 失败：登录信息为空");
            Toast.Show("登录信息无效，请重新登录");
            return;
        }

        UnityEngine.GUIUtility.systemCopyBuffer = loginInfo.comboId;
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

    private static bool TryInitView(PlayerInfoView view)
    {
        Log.I("开始初始化个人中心");
        var player = PlayerController.GetPlayer();
        if (player == null || player.role == null)
        {
            Log.E("个人中心初始化失败：玩家或角色信息为空");
            return false;
        }

        var loginInfo = ComboSDK.GetLoginInfo();
        if (loginInfo == null)
        {
            Log.E("个人中心初始化失败：登录信息为空");
            return false;
        }

        var playerId = loginInfo.comboId;
        string seayooId;
        var seayooAccountAvailable = ComboSDK.IsFeatureAvailable(Feature.SEAYOO_ACCOUNT);
        if (seayooAccountAvailable)
        {
            seayooId = ComboSDK.SeayooAccount.UserId;
            view.manageAccountBtn.gameObject.SetActive(true);
            view.changePasswordBtn.gameObject.SetActive(true);
            view.deleteAccountBtn.gameObject.SetActive(true);
        }
        else
        {
            seayooId = "无";
            view.copyBtn.gameObject.SetActive(false);
        }

        if (!ComboSDK.IsFeatureAvailable(Feature.CONTACT_SUPPORT))
        {
            view.contactSupportBtn.gameObject.SetActive(false);
        }

        bool createRoleEnabled = GameManager.Instance.config.createRoleEnabled;
        view.changeRoleBtn.gameObject.SetActive(createRoleEnabled);
        view.addBtn.gameObject.SetActive(createRoleEnabled);
        view.subBtn.gameObject.SetActive(createRoleEnabled);
        view.levelText.interactable = createRoleEnabled;

        view.SetPlayerId(playerId);
        view.SetSeayooId(seayooId);
        view.SetIdp($"idp : {loginInfo.idp}");
        view.SetRole(player.role);
        view.SetServer(GameManager.Instance.ZoneName, GameManager.Instance.ServerName);

        Log.I($"个人中心初始化成功：idp={loginInfo.idp}, seayooAccountAvailable={seayooAccountAvailable}");
        return true;
    }

}
