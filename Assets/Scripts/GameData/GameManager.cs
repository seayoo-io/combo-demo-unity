using System;
using System.Collections.Generic;
using Combo;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public bool sdkIsLogin { get; set; }
    public int ZoneId { get; set; }
    public int ServerId { get; set; }
    public string ServerName { get; set; }
    public string ZoneName { get; set; }
    public int gold { get; set; }
    public Dictionary<int, Sprite> RoleDic = new Dictionary<int, Sprite>();
    public GameConfig config; // 游戏初始化配置
    public ComboSDKConfig sdkConfig { get; private set; } // ComboSDK 配置（domains）

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void OnBeforeSceneLoad()
    {
        if (Instance == null)
        {
            GameObject gameManagerObject = new GameObject("GameManager");
            Instance = gameManagerObject.AddComponent<GameManager>();
            DontDestroyOnLoad(gameManagerObject); // 保证在切换场景时不会被销毁
            Instance.InitializeSDKConfig(); // 初始化SDK配置
        }
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // 确保只有一个实例存在
        }
    }

    // 默认角色头像
    public void SetupDefaultRole(Role role)
    {
        if (role == null)
        {
            Log.E("设置默认角色资源失败：角色信息为空");
            return;
        }

        var replacingExistingRole = RoleDic.ContainsKey(role.type);
        Log.I($"开始设置默认角色资源：roleType={role.type}, replacingExistingRole={replacingExistingRole}");
        Sprite sprite = Resources.Load<Sprite>("Textures/itemIcon/Character_Sample01");
        if (sprite == null)
        {
            Log.W($"默认角色头像加载失败：roleType={role.type}");
        }

        RoleDic[role.type] = sprite;
        Log.I($"默认角色资源设置成功：roleType={role.type}");
    }

    // 获取游戏初始化配置
    public void GetGameConfig(Action onSuccess, Action onFail)
    {
        Log.I("Start Get Game Config");
        GameClient.GetGameConfig((GameConfig cfg) =>
        {
            Log.I($"Get Game Success, create_role_enabled:{cfg.createRoleEnabled}");
            config = cfg;
            onSuccess.Invoke();
        }, (error) =>
        {
            Toast.Show($"获取参数失败: {error}");
            Log.E("Get Game Config Fail: " + error);
            onFail.Invoke();
        });
    }

    // 加载 SDK 配置（用于根据 Domains 控制 Demo 功能可用性）
    private void InitializeSDKConfig()
    {
        Log.I("Start SDK Config");
        if (BuildParams.GetBuildKey() == null)
        {
            Toast.Show("请先设置 Build Key");
            return;
        }
        var distro = ComboSDK.GetDistro();
        GameClient.GetDomains(
            gameId: BuildParams.GetGameId(),
            buildKey: BuildParams.GetBuildKey(),
            distro: distro,
            action: parameters =>
            {
                sdkConfig = new ComboSDKConfig(parameters);
                Log.I($"Get domains success, domains: {string.Join(", ", sdkConfig.domains)}");
            },
            onError: errorMessage =>
            {
                Log.I($"Error Occurred: {errorMessage}");
            }
        );
    }
}
