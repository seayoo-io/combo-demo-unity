using System;
using System.Collections.Generic;
using System.Reflection;
using Combo;
using UnityEngine;
using UnityEngine.UI;

public struct APIInfo
{
    public enum Status
    {
        Passed,
        Failed,
        Unknown
    }
    public string api;
    public string name;
    public Status status;
}

public class APIStatusController : MonoBehaviour
{
    public Transform parentTransform;
    Dictionary<string, APIInfo> apiList = new Dictionary<string, APIInfo>();

    private static readonly Dictionary<string, string> APIs = new Dictionary<string, string>
    {
        { "GetLoginInfo", "获取用户信息" },
        { "GetGameId", "获取游戏ID" },
        { "GetDeviceId", "获取设备ID" },
        { "GetVersion", "获取版本" },
        { "GetVersionNative", "获取 Native SDK 版本" },
        { "IsFeatureAvailable", "检查功能是否可用" },
#if UNITY_ANDROID || UNITY_IOS
        { "GetAvailableShareTargets", "获取可分享的平台" },
#endif
        { "GetDownloadUrl", "获取下载链接" },
        { "CheckAnnouncements", "检查公告" },
        { "GetDistro", "获取发行版" },
#if UNITY_ANDROID
        { "GetVariant", "获取分包标识" },
#endif
    };

    // Start is called before the first frame update
    void Start()
    {
        Log.I("[APIStatusController] awake");
        EventSystem.Register(this);
        LoadAPIList();
        AppendAPIListView();
        StartTest();
    }

    void LoadAPIList()
    {
        foreach (var kvp in APIs)
        {
            AddAPIInfo(kvp.Key, kvp.Value);
        }
    }

    void AddAPIInfo(string func, string apiName)
    {
        var apiInfo = new APIInfo
        {
            api = func,
            name = apiName,
            status = APIInfo.Status.Unknown
        };
        apiList[func] = apiInfo;
    }

    void OnDestroy()
    {
        EventSystem.UnRegister(this);
    }

    void AppendAPIListView()
    {
        foreach (var dict in apiList)
        {
            var view = APICellView.Instantiate();
            view.SetAPIInfo(dict.Value);
            view.gameObject.transform.SetParent(parentTransform, false);
            view.Show();
        }
    }

    void StartTest()
    {
        Log.I("Start Test");
        foreach (var info in apiList.Values)
        {
            InvokeAPI(info.api);
        }
    }

    [EventSystem.BindEvent]
    public void TestSingleAPI(APIClickEvent action)
    {
        Log.D("TestSingleAPI");
        if (apiList.TryGetValue(action.api, out var info))
        {
            Log.D($"Test {info.api}");
            InvokeAPI(info.api);
        }
    }

    public static void ShowStatusView()
    {
        APIStatusView.DestroyAll();
        var view = APIStatusView.Instantiate();
        view.Show();
    }

    void InvokeAPI(string apiKey)
    {
        MethodInfo methodInfo = GetType().GetMethod(apiKey, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (methodInfo != null)
        {
            methodInfo.Invoke(this, null);
        }
        else
        {
            Log.E($"Method {apiKey} not found.");
        }
    }

    void GetLoginInfo()
    {
        Log.D("[APIStatusController] GetLoginInfo");
        var data = apiList["GetLoginInfo"];
        var info = ComboSDK.GetLoginInfo();
        if (info == null || string.IsNullOrEmpty(info.comboId))
        {
            SendEvent(data.api, false);
        }
        else
        {
            SendEvent(data.api, true);
        }
    }

    void GetAvailableShareTargets()
    {
        Log.D("[APIStatusController] GetAvailableShareTargets");
        var data = apiList["GetAvailableShareTargets"];
        var shareTargets = ComboSDK.GetAvailableShareTargets();
        if (shareTargets == null)
        {
            SendEvent(data.api, false);
        }
        else
        {
            SendEvent(data.api, true);
        }
    }

    void GetDownloadUrl()
    {
        Log.I("[APIStatusController] GetDownloadUrl");
        var data = apiList["GetDownloadUrl"];
        ComboSDK.GetDownloadUrl(result =>
        {
            if (result.IsSuccess)
            {
                SendEvent(data.api, true);
            }
            else
            {
                SendEvent(data.api, false);
            }
        });
    }

    void CheckAnnouncements()
    {
        Log.D("[APIStatusController] CheckAnnouncements");
        var data = apiList["CheckAnnouncements"];
        var opts = new CheckAnnouncementsOptions();
        opts.Profile = PlayerController.GetPlayer().role.roleId;
        opts.Level = PlayerController.GetPlayer().role.roleLevel;
        ComboSDK.CheckAnnouncements(opts, result =>
        {
            if (result.IsSuccess)
            {
                SendEvent(data.api, true);
            }
            else
            {
                SendEvent(data.api, false);
            }
        });
    }

    void GetDistro()
    {
        Log.D("[APIStatusController] GetDistro");
        var data = apiList["GetDistro"];
        if (ComboSDK.GetDistro() == null)
        {
            SendEvent(data.api, false);
        } else
        {
            SendEvent(data.api, true);
        }
    }

    void GetVariant()
    {
        Log.D("[APIStatusController] GetVariant");
        var data = apiList["GetVariant"];
        if (ComboSDK.GetVariant() == null)
        {
            SendEvent(data.api, false);
        } else
        {
            SendEvent(data.api, true);
        }
    }

    void GetGameId()
    {
        Log.D("[APIStatusController] GetGameId");
        var data = apiList["GetGameId"];
        if (ComboSDK.GetGameId() == null)
        {
            SendEvent(data.api, false);
        } else
        {
            SendEvent(data.api, true);
        }
    }

    void GetDeviceId()
    {
        Log.D("[APIStatusController] GetDeviceId");
        var data = apiList["GetDeviceId"];
        if (ComboSDK.GetDeviceId() == null)
        {
            SendEvent(data.api, false);
        } else
        {
            SendEvent(data.api, true);
        }
    }

    void GetVersion()
    {
        Log.D("[APIStatusController] GetVersion");
        var data = apiList["GetVersion"];
        if (ComboSDK.GetVersion() == null)
        {
            SendEvent(data.api, false);
        } else
        {
            SendEvent(data.api, true);
        }
    }

    void GetVersionNative()
    {
        Log.D("[APIStatusController] GetVersionNative");
        var data = apiList["GetVersionNative"];
        if (ComboSDK.GetVersionNative() == null)
        {
            SendEvent(data.api, false);
        } else
        {
            SendEvent(data.api, true);
        }
    }

    void IsFeatureAvailable()
    {
        Log.D("[APIStatusController] IsFeatureAvailable");
        var data = apiList["IsFeatureAvailable"];
        if (ComboSDK.IsFeatureAvailable(Feature.SEAYOO_ACCOUNT) == false)
        {
            SendEvent(data.api, false);
        } else
        {
            SendEvent(data.api, true);
        }
    }

    private void SendEvent(string api, bool isSuccess)
    {
        APITestResultEvent.Invoke(new APITestResultEvent
        {
            api = api,
            status = isSuccess ? APIInfo.Status.Passed : APIInfo.Status.Failed
        });
    }
}
