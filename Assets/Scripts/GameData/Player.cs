using System;
using Combo;
using UnityEngine;

public class Player : MonoBehaviour {
    public Role role;
    public void EnterGame() { 
        ComboSDK.ReportEnterGame(new RoleInfo {
            roleCreateTime = role.roleCreateTime,
            roleId = role.roleId,
            roleLevel = role.roleLevel,
            roleName = role.roleName,
            serverId = $"{role.serverId}",
            serverName = role.serverName
        });
    }

    public void CreateRole(Role r) {
        role = r;

        ComboSDK.ReportCreateRole(
            new RoleInfo
            {
                roleCreateTime = role.roleCreateTime,
                roleId = role.roleId,
                roleLevel = role.roleLevel,
                roleName = role.roleName,
                serverId = $"{role.serverId}",
                serverName = role.serverName
            }
        );
    }

    public void UpdateRole(Role r) {
        role = r;
    }

    public void UpDateLevel(int changeLevel)
    {
        role.roleLevel = changeLevel;
        Log.I("current level: " + role.roleLevel);
    }

    public void ClearInfo()
    {
        role = null;
    }
}