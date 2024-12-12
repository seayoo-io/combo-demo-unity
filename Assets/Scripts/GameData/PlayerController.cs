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

    public static void PlayerLevelUp(Player player) {
        player.LevelUp();
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
}
