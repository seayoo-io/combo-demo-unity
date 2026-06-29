using System.Collections;
using UnityEngine;

// 深链处理器：监听 iOS/Android 自定义 URL scheme 等深链唤起，并以 Toast 显示链接
// 通过 RuntimeInitializeOnLoadMethod + DontDestroyOnLoad 常驻，跨场景持续生效
public class DeepLinkHandler : MonoBehaviour
{
    // 单例实例
    public static DeepLinkHandler Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void OnBeforeSceneLoad()
    {
        if (Instance == null)
        {
            GameObject handlerObject = new GameObject("DeepLinkHandler");
            Instance = handlerObject.AddComponent<DeepLinkHandler>();
            DontDestroyOnLoad(handlerObject); // 保证在切换场景时不会被销毁
        }
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // 确保只有一个实例存在
            return;
        }

        // 订阅深链唤起事件：App 已在运行时被深链唤起会回调
        Application.deepLinkActivated += OnDeepLinkActivated;

        // 冷启动：App 被深链直接拉起时，deepLinkActivated 不会再回调，需在启动时主动读取 absoluteURL
        if (!string.IsNullOrEmpty(Application.absoluteURL))
        {
            // 延后一帧，待首个场景加载完成后再弹 Toast
            StartCoroutine(ShowColdStartLink(Application.absoluteURL));
        }
    }

    void OnDestroy()
    {
        Application.deepLinkActivated -= OnDeepLinkActivated;
    }

    // 深链唤起回调：打印日志并以 Toast 显示链接
    private void OnDeepLinkActivated(string url)
    {
        Log.I($"DeepLink activated: {url}");
        Toast.Show($"DeepLink: {url}");
    }

    // 冷启动场景下延后一帧再处理，避免首个场景尚未加载完成
    private IEnumerator ShowColdStartLink(string url)
    {
        yield return null;
        OnDeepLinkActivated(url);
    }
}
