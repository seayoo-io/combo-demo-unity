using System;
using Combo;
using UnityEngine;

public class PlayerController
{
    public static Player GetPlayer()
    {
        var go = GameObject.FindGameObjectWithTag("Player");
        return go?.GetComponent<Player>();
    }

    public static GameObject SpawnPlayer(string roleId, string roleName, string serverId)
    {
        var go = GameObject.Instantiate(Resources.Load("Prefabs/Player")) as GameObject;
        var playComp = go.GetComponent<Player>();
        playComp.CreateRole(roleId, roleName, serverId);
        return go;
    }

    public static void PlayerEnterGame(Player player) {
        player.EnterGame();
    }

    public static void PlayerLevelUp(Player player) {
        player.LevelUp();
    }
}
