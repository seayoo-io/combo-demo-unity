using System.Collections.Generic;
using Combo;
using UnityEngine;

public class GamerManager : MonoBehaviour
{
    void Start()
    {
        EventSystem.Register(this);
    }

    void OnDestroy()
    {
        EventSystem.UnRegister(this);
    }

    [EventSystem.BindEvent]
    void HandleOpenShortLinkEvent(OpenShortLinkEvent evt)
    {
        OnOpenShortLink(evt.shortLink);
    }

    public void OnOpenShortLink(string shortLink)
    {
        if(string.IsNullOrEmpty(shortLink))
        {
            Toast.Show("短链接为空，请输入短链接");
            return;
        }
        var role = PlayerController.GetRoleInfo(PlayerController.GetPlayer());
        var gameData = new Dictionary<string, string>(){
            {"server_id", role.serverId},
            {"role_id", role.roleId},
            {"role_name", role.roleName},
            {"role_level", role.roleLevel.ToString()},
        };
        ComboSDK.OpenShortLink(shortLink, gameData, result =>{
            if(result.IsSuccess)
            {}
            else
            {
                var err = result.Error;
                Toast.Show($"{err.Message}");
                Log.E(err.DetailMessage);
            }
        });
    }
}
