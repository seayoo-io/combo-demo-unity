// HTTPDNS 最早期生命周期测试：验证 native 在 Application.onCreate 初始化后，
// Unity C# 第一行代码执行时 HttpDns 是否已就绪。
//
// 无需挂任何 GameObject —— [RuntimeInitializeOnLoadMethod(BeforeSceneLoad)] 由 Unity
// 在「第一个场景加载前」自动调用，这是 C# 侧能执行的最早时机（早于任何 Awake/Start）。
//
// 时序预期（Android）：
//   Application.attachBaseContext → GlobalParameters.init（凭据就绪）
//   Application.onCreate          → HttpDns.setup（native 初始化）   ★ 在此完成
//   ───────────────────────────────────────────────────────────────
//   Activity.onCreate → super.onCreate() → new UnityPlayer()        ← Unity Runtime 启动
//   il2cpp 加载 → [RuntimeInitializeOnLoadMethod] BeforeSceneLoad    ← 本测试在此运行
//
// 时序预期（HarmonyOS）：
//   ComboSDKAbility.onCreate（原生 UIAbility 生命周期，主 Ability 入口）
//     → ComboSDK.onCreate → Combo.onCreate
//     → globalParameters.init（凭据就绪）→ HttpDnsClient.setup（native 初始化）  ★ 在此完成
//   ───────────────────────────────────────────────────────────────
//   onWindowStageCreate → loadContent → Unity Runtime 启动          ← 晚于上面
//   il2cpp 加载 → [RuntimeInitializeOnLoadMethod] BeforeSceneLoad    ← 本测试在此运行
//   UIAbility.onCreate 早于 onWindowStageCreate（生命周期固定顺序），故 setup 早于 Unity Runtime。
//   前提：导出工程主 Ability 为 ComboSDKAbility（ComboSDK.onCreate 必经）。
//
// 因此进入本测试时，native 早已初始化完毕：IsFeatureAvailable(HTTPDNS) 应为 true，
// ResolveSync 应返回真实 IP（前提：ComboSDK.json 配好 sdk_httpdns_config）。
//
// Console 搜 [HTTPDNS-EARLY] 看输出。

using System.Threading;
using Combo;
using UnityEngine;

public static class HttpDnsEarlyTest
{
    const string TAG = "[HTTPDNS-EARLY]";

    static readonly string[] TestDomains =
    {
        "api.seayoo.com",
        "ggd.seayooassets.com",
    };

    // BeforeSceneLoad：Unity C# 最早执行点。此时第一个场景尚未加载，无任何 MonoBehaviour。
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnBeforeSceneLoad()
    {
        // ① 可用性查询是同步的，可在主线程直接调——它只读 native 的初始化标志位。
        bool available = ComboSDK.IsFeatureAvailable(Feature.HTTPDNS);
        Debug.Log($"{TAG} BeforeSceneLoad: IsFeatureAvailable(HTTPDNS) = {available} " +
                  $"(true 即证明 native 已在 Application.onCreate 早于 Unity Runtime 完成初始化)");

        // ② ResolveSync 禁主线程（会阻塞），派到 ThreadPool 工作线程执行。
        ThreadPool.QueueUserWorkItem(_ =>
        {
            foreach (var domain in TestDomains)
            {
                HttpDnsResult r = HttpDns.ResolveSync(domain);
                if (r == null)
                {
                    Debug.Log($"{TAG} [sync] {domain} => null（不可用/解析失败，回退系统 DNS）");
                }
                else
                {
                    string v4 = r.IPv4 != null ? string.Join(",", r.IPv4) : "";
                    string v6 = r.IPv6 != null ? string.Join(",", r.IPv6) : "";
                    Debug.Log($"{TAG} [sync] {domain} => IPv4=[{v4}] IPv6=[{v6}]");
                }
            }
        });
    }
}
