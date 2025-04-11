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
        Sprite sprite = Resources.Load<Sprite>("Textures/itemIcon/Character_Sample01");
        RoleDic.Add(role.type, sprite);
    }

    // 获取游戏初始化配置
    public void GetGameConfig(Action onSuccess, Action onFail)
    {
        Log.I("初始化 Game 配置");
        GameClient.GetGameConfig((GameConfig cfg) =>
        {
            Log.I($"获取参数成功: {cfg.createRoleEnabled}");
            config = cfg;
            onSuccess.Invoke();
        }, (error) =>
        {
            Toast.Show($"获取参数失败: {error}");
            Log.E("获取参数失败: " + error);
            onFail.Invoke();
        });
    }

    // 加载 SDK 配置（用于根据 Domains 控制 Demo 功能可用性）
    private void InitializeSDKConfig()
    {
        Log.I("初始化 SDK 配置");
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
                Log.I($"获取参数成功，domains: {string.Join(", ", sdkConfig.domains)}");
            },
            onError: errorMessage =>
            {
                Log.I($"Error Occurred: {errorMessage}");
            }
        );
    }
}
