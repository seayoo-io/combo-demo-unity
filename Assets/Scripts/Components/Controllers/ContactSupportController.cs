using System.Collections.Generic;
using Combo;

public static class ContactSupportController
{
    // 打开客服：仅在已登录且已有角色时上报角色信息，否则不带角色数据打开客服
    public static void ContactSupport()
    {
        var options = BuildOptions();
        if (options == null)
        {
            ComboSDK.ContactSupport();
            return;
        }
        ComboSDK.ContactSupport(options);
    }

    private static ContactSupportOptions BuildOptions()
    {
        if (GameManager.Instance == null || !GameManager.Instance.sdkIsLogin)
        {
            Log.I("打开客服：未登录，不上报角色信息");
            return null;
        }

        var loginInfo = ComboSDK.GetLoginInfo();
        if (loginInfo == null || string.IsNullOrEmpty(loginInfo.comboId))
        {
            Log.I("打开客服：登录信息为空，不上报角色信息");
            return null;
        }

        var player = PlayerController.GetPlayer();
        // Player 是 UnityEngine.Object，用 != null 判空而不是 ?.，以正确处理已销毁对象
        var role = player != null ? player.role : null;
        if (role == null)
        {
            Log.I("打开客服：已登录但未选择角色，不上报角色信息");
            return null;
        }

        Log.I($"打开客服：上报角色信息 serverId={role.serverId}, roleId={role.roleId}, roleLevel={role.roleLevel}");
        return new ContactSupportOptions
        {
            GameData = new Dictionary<string, string>
            {
                ["server_id"] = role.serverId.ToString(),
                ["role_id"] = role.roleId,
                ["role_name"] = role.roleName,
                ["role_level"] = role.roleLevel.ToString(),
            }
        };
    }
}
