using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

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

        private static IEnumerator GetCoroutine(HttpRequestOptions options, Action<HttpResponse> callback)
        {
            var queryString = options.body == null ? "" : "?" + HttpUtils.ParseQueryString(options.body);
            var fullUrl = options.url + queryString;
            using (UnityWebRequest webRequest = UnityWebRequest.Get(fullUrl))
            {
                if (options.auth != null) options.headers["Authorization"] = options.auth.Get();
                foreach (var pair in options.headers)
                {
                    webRequest.SetRequestHeader(pair.Key, pair.Value);
                }
                webRequest.timeout = options.timeout;
                yield return webRequest.SendWebRequest();

                callback?.Invoke(HttpResponse.Create(webRequest, options.headers));
            }
        }
        private static IEnumerator PostCoroutine(HttpRequestOptions options, Action<HttpResponse> callback)
        {
            var bodyStr = options.body.ToJson();
            byte[] postData = System.Text.Encoding.UTF8.GetBytes(bodyStr);
#if UNITY_2022
            using (UnityWebRequest webRequest = UnityWebRequest.PostWwwForm(options.url, "POST"))
#else
            using (UnityWebRequest webRequest = UnityWebRequest.Post(options.url, "POST"))
#endif
            {
                options.headers["Content-Type"] = "application/json";
                options.headers["Accept"] = "application/json";
                if (options.auth != null) options.headers["Authorization"] = options.auth.Get();
                foreach (var pair in options.headers)
                {
                    webRequest.SetRequestHeader(pair.Key, pair.Value);
                }
                webRequest.timeout = options.timeout;

                webRequest.uploadHandler = new UploadHandlerRaw(postData);
                yield return webRequest.SendWebRequest();

                callback?.Invoke(HttpResponse.Create(webRequest, options.headers, bodyStr));
            }
        }
    }
}
