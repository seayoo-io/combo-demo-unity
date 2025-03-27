using System.Collections.Generic;
using Combo;
using UnityEngine;

public class ShortLinkViewController : MonoBehaviour
{
    void Start()
    {
        EventSystem.Register(this); // 注册到全局事件系统
    }

    void OnDestroy()
    {
        EventSystem.UnRegister(this); // 销毁时注销事件
    }

    //-- 事件绑定：监听打开短链接事件 --
    [EventSystem.BindEvent]
    private void HandleOpenShortLinkEvent(OpenShortLinkEvent evt)
    {
        OnOpenShortLink(evt.shortLink);
    }

    //-- 核心处理逻辑 --
    private void OnOpenShortLink(string shortLink)
    {
        if (string.IsNullOrEmpty(shortLink))
        {
            Toast.Show("短链接不能为空");
            Log.E("打开短链接失败：空内容");
            return;
        }
        
        Log.I($"开始处理短链接: {shortLink}");

        if(string.IsNullOrEmpty(shortLink))
        {
            Toast.Show("短链接为空，请输入短链接");
            return;
        }
        // 此时还没有到选服，没有角色信息
        var gameData = new Dictionary<string, string>();
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