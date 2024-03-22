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
            serverId = role.serverId,
            serverName = role.serverName
        });
    }

    public void CreateRole(string roleId, string roleName, string serverId) {
        role = new Role
        {
            roleCreateTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            roleId = roleId,
            roleLevel = 1,
            roleName = roleName,
            serverId = serverId,
            serverName = $"mock-server-{serverId}",
        };

        ComboSDK.ReportCreateRole(
            new RoleInfo
            {
                roleCreateTime = role.roleCreateTime,
                roleId = role.roleId,
                roleLevel = role.roleLevel,
                roleName = role.roleName,
                serverId = role.serverId,
                serverName = role.serverName
            }
        );
    }

    public void LevelUp()
    {
        role.roleLevel++;
    }
}