// HTTPDNS POC v3：验证 UnityWebRequest / HttpWebRequest / HttpClient 在 IP 直连场景下
//   ① 能否可靠设置 Host 请求头   ② 能否把 SNI 指定为原域名（而非连接用的 IP）
//
// v3：SNI 靶子改为多域名（交叉验证），判定改通用（看证书是否目标域名的，不硬编码 CDN 名）。
//   · 打印 issuer 区分真证书 / MITM   · 显式禁代理   · fake-ip + 代理环境自检
//
// 用法：空 GameObject 挂本脚本 → Play → Console 搜 [HTTPDNS-POC]。务必先彻底关代理。
//
// 判定原理：IP 直连 + 设 Host=域名后，看 TLS 实际返回的证书 subject——
//   含目标域名 + issuer 为公共 CA → 客户端真发了 SNI=域名（能设 SNI）
//   是别家/默认证书           → 客户端未发 SNI=域名（无法设 SNI）
//
// SNI 靶子（均 SNI-required，无 SNI 时返回别家默认证书）：
//   ggd.seayooassets.com（游戏资源，EdgeOne）、api.seayoo.com（Combo SDK 后端，共享 CDN）
// Host 靶子：account-3.ggd.seayoogames.cn（Host=域名→404，Host=IP→403）

using System;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class HttpDnsSniHostProbe : MonoBehaviour
{
    const string TAG = "[HTTPDNS-POC]";

    static volatile string _capSubject = "";
    static volatile string _capIssuer = "";

    struct Target { public string domain, path, expect; }
    static readonly Target[] SniTargets =
    {
        new Target { domain = "ggd.seayooassets.com", path = "/android/version.txt", expect = "seayooassets.com" },
        new Target { domain = "api.seayoo.com",       path = "/",                    expect = ".seayoo.com" },
    };

    const string HOST_DOMAIN = "account-3.ggd.seayoogames.cn";
    const string HOST_PATH   = "/Account";

    static bool IsFakeIp(string ip) =>
        ip != null && (ip.StartsWith("198.18.") || ip.StartsWith("198.19.") || ip.StartsWith("100.64."));

    static bool RealCa(string issuer) =>
        issuer != null && (issuer.Contains("Let's Encrypt") || issuer.Contains("DigiCert") || issuer.Contains("GlobalSign")
        || issuer.Contains("Sectigo") || issuer.Contains("Amazon") || issuer.Contains("WoTrus") || issuer.Contains("GoDaddy"));

    static string Verdict(string subject, string issuer, string expect)
    {
        if (string.IsNullOrEmpty(subject)) return "未抓到证书";
        if (subject.Contains(expect))
            return RealCa(issuer)
                ? "✓ SNI 发了域名 + 公共CA → 客户端确实设上了 SNI"
                : "⚠ subject=目标域名但 issuer 非公共CA → 疑似 MITM，请核 issuer/关代理";
        return $"★ 拿到别家证书（非目标域名）→ SNI 未发域名 → 无法设 SNI";
    }

    void Start()
    {
        Debug.Log($"{TAG} ===== 开始 (Unity {Application.unityVersion}, backend={Application.platform}) =====");

        string probe = "";
        try { probe = Dns.GetHostAddresses(SniTargets[0].domain)[0].ToString(); } catch { }
        if (IsFakeIp(probe))
            Debug.LogError($"{TAG} ⚠️⚠️⚠️ fake-ip 代理（{SniTargets[0].domain} → {probe}）！IP 直连测试失真，请彻底关代理后重跑。");
        else
            Debug.Log($"{TAG} 自检①fake-ip：通过（{SniTargets[0].domain} → {probe}）");

        try
        {
            var u = new Uri($"https://{SniTargets[0].domain}/");
            var p = WebRequest.DefaultWebProxy?.GetProxy(u);
            bool via = p != null && p.AbsoluteUri != u.AbsoluteUri;
            Debug.Log($"{TAG} 自检②系统代理：DefaultWebProxy={(via ? p.ToString() : "无（直连）")} | " +
                      $"https_proxy={Environment.GetEnvironmentVariable("https_proxy") ?? "<空>"} " +
                      $"HTTPS_PROXY={Environment.GetEnvironmentVariable("HTTPS_PROXY") ?? "<空>"} " +
                      $"all_proxy={Environment.GetEnvironmentVariable("all_proxy") ?? "<空>"}");
        }
        catch (Exception e) { Debug.Log($"{TAG} 自检②异常: {e.Message}"); }

        Task.Run(RunDotNetProbes);
        StartCoroutine(RunUnityWebRequestProbes());
    }

    // ---------------------------------------------------------------- UnityWebRequest

    class CaptureCert : CertificateHandler
    {
        public string subject = "", issuer = "";
        protected override bool ValidateCertificate(byte[] der)
        {
            try { var c = new X509Certificate2(der); subject = c.Subject; issuer = c.Issuer; }
            catch (Exception e) { subject = "解析失败:" + e.Message; }
            return true;
        }
    }

    IEnumerator RunUnityWebRequestProbes()
    {
        foreach (var t in SniTargets)
        {
            string ip = SafeResolve(t.domain);
            if (ip == null) continue;
            var req = UnityWebRequest.Get($"https://{ip}{t.path}");
            var cap = new CaptureCert();
            req.certificateHandler = cap; req.disposeCertificateHandlerOnDispose = true;
            string err = "ok";
            try { req.SetRequestHeader("Host", t.domain); } catch (Exception e) { err = e.GetType().Name + ":" + e.Message; }
            yield return req.SendWebRequest();
            Debug.Log($"{TAG} [UWR/SNI {t.domain}] {Verdict(cap.subject, cap.issuer, t.expect)}  ||  subj={cap.subject} | issuer={cap.issuer} | setHost={err}");
            req.Dispose();
        }

        string hip = SafeResolve(HOST_DOMAIN);
        if (hip != null)
        {
            var req = UnityWebRequest.Get($"https://{hip}{HOST_PATH}");
            req.certificateHandler = new CaptureCert(); req.disposeCertificateHandlerOnDispose = true;
            string err = "ok";
            try { req.SetRequestHeader("Host", HOST_DOMAIN); } catch (Exception e) { err = e.GetType().Name + ":" + e.Message; }
            yield return req.SendWebRequest();
            string v = req.responseCode == 404 ? "★ Host 头生效（发了域名）"
                     : req.responseCode == 403 ? "✗ Host 头未生效（按 IP 拒绝）" : $"其他({req.responseCode})";
            Debug.Log($"{TAG} [UWR/Host] {v}  ||  code={req.responseCode} | setHost={err}  (404=生效/403=忽略)");
            req.Dispose();
        }
        Debug.Log($"{TAG} ===== UnityWebRequest 部分结束 =====");
    }

    static string SafeResolve(string d)
    {
        try { return Dns.GetHostAddresses(d)[0].ToString(); }
        catch (Exception e) { Debug.LogError($"{TAG} 解析 {d} 失败: {e.Message}"); return null; }
    }

    // ---------------------------------------------------------------- HttpWebRequest / HttpClient / 反射

    async Task RunDotNetProbes()
    {
        ServicePointManager.ServerCertificateValidationCallback =
            (s, cert, chain, err) => { if (cert != null) { _capSubject = cert.Subject; _capIssuer = cert.Issuer; } return true; };

        var sockType = Type.GetType("System.Net.Http.SocketsHttpHandler, System.Net.Http")
                       ?? Type.GetType("System.Net.Http.SocketsHttpHandler");
        var cb = sockType?.GetProperty("ConnectCallback");
        Debug.Log($"{TAG} [反射/ConnectCallback] SocketsHttpHandler={(sockType == null ? "无" : "有")}, ConnectCallback={(cb == null ? "无" : "有")}" +
                  $"  （注：设 Host header 即可令 SNI=Host，不依赖 ConnectCallback；后者仅 SNI≠Host 高级场景）");

        // 对照实验：同客户端、同 IP，唯一变量 = 是否设 Host=域名。
        // 若"设 Host→域名证书 / 不设→默认证书"，则铁证 SNI 来源就是 Host（.Host 属性是唯一自变量）。
        foreach (var t in SniTargets)
        {
            string ip;
            try { ip = Dns.GetHostAddresses(t.domain)[0].ToString(); }
            catch (Exception e) { Debug.LogError($"{TAG} 解析 {t.domain} 失败: {e.Message}"); continue; }
            string ipUrl = $"https://{ip}{t.path}";

            HttpWebReqProbe($"HttpWebReq/SNI {t.domain} [设.Host=域名]", ipUrl, t.domain, t.expect);
            HttpWebReqProbe($"HttpWebReq/SNI {t.domain} [不设.Host→默认IP]", ipUrl, null, t.expect);
            await ProbeHttpClient($"HttpClient/SNI {t.domain} [设Host=域名]", ipUrl, t.domain, t.expect);
            await ProbeHttpClient($"HttpClient/SNI {t.domain} [不设Host→默认IP]", ipUrl, null, t.expect);
        }
        Debug.Log($"{TAG} ===== .NET 部分结束 =====");
    }

    static void HttpWebReqProbe(string label, string url, string host, string expect)
    {
        _capSubject = ""; _capIssuer = "";
        try
        {
            var req = (HttpWebRequest)WebRequest.Create(url);
            if (host != null) req.Host = host;   // 唯一变量
            req.Proxy = null; req.Timeout = 10000;
            try { using (var r = (HttpWebResponse)req.GetResponse()) { } }
            catch (WebException we) { (we.Response as IDisposable)?.Dispose(); }
        }
        catch (Exception e) { if (string.IsNullOrEmpty(_capSubject)) _capSubject = "请求异常:" + e.Message; }
        Debug.Log($"{TAG} [{label}] {Verdict(_capSubject, _capIssuer, expect)}  ||  subj={_capSubject} | issuer={_capIssuer}");
    }

    static async Task ProbeHttpClient(string label, string url, string hostHeader, string expect)
    {
        _capSubject = ""; _capIssuer = "";
        var handler = new HttpClientHandler();
        try { handler.UseProxy = false; } catch { }
        try { handler.ServerCertificateCustomValidationCallback =
                (msg, cert, chain, err) => { if (cert != null) { _capSubject = cert.Subject; _capIssuer = cert.Issuer; } return true; }; }
        catch { }
        using (var client = new HttpClient(handler))
        {
            client.Timeout = TimeSpan.FromSeconds(10);
            var req = new HttpRequestMessage(HttpMethod.Get, url);
            if (hostHeader != null) req.Headers.Host = hostHeader;
            try { using (var resp = await client.SendAsync(req)) { } }
            catch (Exception e) { if (string.IsNullOrEmpty(_capSubject)) _capSubject = "请求异常:" + e.Message; }
        }
        Debug.Log($"{TAG} [{label}] {Verdict(_capSubject, _capIssuer, expect)}  ||  subj={_capSubject} | issuer={_capIssuer}");
    }
}
