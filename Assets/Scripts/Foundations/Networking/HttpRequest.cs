using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Networking
{
    class HttpRequestCaller : MonoBehaviour
    {
        private static HttpRequestCaller _instance;
        private const string GameObjectName = "HttpHandler";

        public static HttpRequestCaller Instance
        {
            get
            {
                if (_instance != null) return _instance;

                _instance = FindObjectOfType<HttpRequestCaller>();
                if (_instance != null) return _instance;

                _instance = new GameObject(GameObjectName).AddComponent<HttpRequestCaller>();
                _instance.gameObject.hideFlags = HideFlags.HideInHierarchy;
                DontDestroyOnLoad(_instance.gameObject);

                return _instance;
            }
        }
    }


    public class HttpRequest
    {
        public static void Get(HttpRequestOptions options, Action<HttpResponse> callback)
        {
            HttpRequestCaller.Instance.StartCoroutine(GetCoroutine(options, callback));
        }

        public static void Post(HttpRequestOptions options, Action<HttpResponse> callback)
        {
            HttpRequestCaller.Instance.StartCoroutine(PostCoroutine(options, callback));
        }

        // options.timeout 单位是秒（沿用原 UnityWebRequest.timeout 语义）；
        // 0 表示未设置 → 用原型默认超时（传 0 给原型会被当 0ms，故此处转默认）。
        private const int DefaultTimeoutMs = 10000;
        private static int TimeoutMs(HttpRequestOptions options) =>
            options.timeout > 0 ? options.timeout * 1000 : DefaultTimeoutMs;

        private static IEnumerator GetCoroutine(HttpRequestOptions options, Action<HttpResponse> callback)
        {
            var queryString = options.body == null ? "" : "?" + HttpUtils.ParseQueryString(options.body);
            var fullUrl = options.url + queryString;

            if (options.auth != null) options.headers["Authorization"] = options.auth.Get();

            var op = HttpDnsWebRequest.Get(fullUrl, options.headers, TimeoutMs(options));
            yield return op;

            LogVia("GET", fullUrl, op.Response);
            callback?.Invoke(HttpResponse.Create(op.Response, options.headers));
        }

        private static IEnumerator PostCoroutine(HttpRequestOptions options, Action<HttpResponse> callback)
        {
            var bodyStr = options.body.ToJson();
            byte[] postData = System.Text.Encoding.UTF8.GetBytes(bodyStr);

            options.headers["Accept"] = "application/json";
            if (options.auth != null) options.headers["Authorization"] = options.auth.Get();

            // Content-Type 由原型按 body 设置（受限 header，走属性而非 Headers.Add）。
            var op = HttpDnsWebRequest.Post(options.url, postData,
                "application/json", options.headers, TimeoutMs(options));
            yield return op;

            LogVia("POST", options.url, op.Response);
            callback?.Invoke(HttpResponse.Create(op.Response, options.headers, bodyStr));
        }

        // 调试日志：标注本次请求经哪条路完成——
        //   via = 解析到的 IP  → 走了 HTTPDNS（命中加速）
        //   via = "direct"     → 回退原域名 + 系统 DNS（HTTPDNS 未解析到/失败/Editor）
        //   via = null         → 所有候选都失败（连接级错误）
        private const string TAG = "[HTTPDNS-NET]";
        private static void LogVia(string method, string url, HttpDnsWebRequest.Response resp)
        {
            if (resp == null)
            {
                Debug.LogWarning($"{TAG} {method} {url} => no response");
                return;
            }

            string via = resp.SucceededVia;
            string route = string.IsNullOrEmpty(via)
                ? "FAILED（所有候选失败）"
                : (via == "direct" ? "回退系统 DNS (direct)" : $"HTTPDNS 命中 (via {via})");

            string msg = $"{TAG} {method} {url} => {route}, status={resp.StatusCode}, success={resp.IsSuccess}";
            if (resp.IsSuccess)
                Debug.Log(msg);
            else
                Debug.LogWarning($"{msg}, error={resp.ErrorMessage}");
        }
    }
}
