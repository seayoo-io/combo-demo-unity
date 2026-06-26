// HTTPDNS 联调测试：空 GameObject 挂本脚本 → Play / 真机运行 → Console 搜 [HTTPDNS-TEST]。
// 真机点屏幕左上角按钮可重复触发。
//
// 验证三个入口：
//   ① ComboSDK.IsFeatureAvailable(Feature.HTTPDNS)  —— 可用性（凭据齐全 + 初始化成功）
//   ② HttpDns.Resolve(domain)                       —— 异步，主线程协程
//   ③ HttpDns.ResolveSync(domain)                   —— 同步，工作线程（禁主线程）
//
// 前提：ComboSDK.json 的 sdk_httpdns_config 配好 dns_id / dns_key，否则可用性为 false、解析返回 null。
// Editor / PC 无原生 HTTPDNS 桥：可用性 false、解析 null（回退系统 DNS）属预期。

using System.Collections;
using System.Threading.Tasks;
using Combo;
using UnityEngine;

public class HttpDnsTest : MonoBehaviour
{
    const string TAG = "[HTTPDNS-TEST]";

    // 待解析域名（对齐 sdk_httpdns_config.pre_resolve_domains；可按需增删）
    static readonly string[] TestDomains =
    {
        "api.seayoo.com",
        "ggd.seayooassets.com",
        "account.ggd.seayoogames.cn",
        "ggd-5.dev.seayooassets.com"
    };

    void Start()
    {
        RunTests();
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(20, 20, 360, 140), "Run HTTPDNS Test"))
        {
            RunTests();
        }
    }

    void RunTests()
    {
        bool available = ComboSDK.IsFeatureAvailable(Feature.HTTPDNS);
        Debug.Log($"{TAG} IsFeatureAvailable(HTTPDNS) = {available}");

        StartCoroutine(TestResolveAsync());   // ② 异步：主线程协程
        TestResolveSyncOnWorker();            // ③ 同步：工作线程
    }

    // ② 异步版 —— Resolve 把同步解析派发到 ThreadPool，逐帧轮询，不阻塞主线程
    IEnumerator TestResolveAsync()
    {
        foreach (var domain in TestDomains)
        {
            var op = HttpDns.Resolve(domain);
            yield return op;
            Debug.Log($"{TAG} [async] {Format(domain, op.Result)}");
        }
    }

    // ③ 同步版 —— 仅工作线程调用（这里放到 Task；切勿在主线程调 ResolveSync）
    void TestResolveSyncOnWorker()
    {
        Task.Run(() =>
        {
            foreach (var domain in TestDomains)
            {
                HttpDnsResult result = HttpDns.ResolveSync(domain);
                Debug.Log($"{TAG} [sync]  {Format(domain, result)}");
            }
        });
    }

    static string Format(string domain, HttpDnsResult r)
    {
        if (r == null)
            return $"{domain} → null（不可用/解析失败，调用方回退系统 DNS）";

        string v4 = r.IPv4 != null ? string.Join(",", r.IPv4) : "";
        string v6 = r.IPv6 != null ? string.Join(",", r.IPv6) : "";
        return $"{domain} → IPv4=[{v4}] IPv6=[{v6}]";
    }
}
