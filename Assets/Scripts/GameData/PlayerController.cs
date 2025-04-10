using System;
using System.Runtime.InteropServices;
using Combo;
using UnityEngine;

public class PlayerController
{
    public static Player GetPlayer()
    {
        var go = GameObject.FindGameObjectWithTag("Player");
        return go?.GetComponent<Player>();
    }

    public static GameObject SpawnPlayer(Role role)
    {
        var go = GameObject.Instantiate(Resources.Load("Prefabs/Player")) as GameObject;
        var playComp = go.GetComponent<Player>();
        playComp.CreateRole(role);
        return go;
    }

    public static void PlayerEnterGame(Player player) {
        player.EnterGame();
    }

    public static void PlayerLevelUpdate(Player player, int changeLevel) {
        player.UpDateLevel(changeLevel);
    }

    public static Role GetRoleInfo(Player player)
    {
        return player.role;
    }

    public static void UpdateRole(Player player, Role role)
    {
        player.UpdateRole(role);
    }

    public static void ClearInfo(Player player)
    {
        player.ClearInfo();
    }
    public static Role GetDefaultRole()
    {
        Role role = new Role();
        role.gender = 0;
        role.roleCreateTime = 0;
        role.roleId = "default";
        role.roleLevel = 1;
        role.roleName = "default";
        role.serverId = 1;
        role.serverName = "default";
        role.type = 0;
        role.zoneId = 1;
        return role;
    }
}
