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
        playerInfoView.SetCancelCallback(() => playerInfoView.Destroy());
        playerInfoView.Show();
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

}
