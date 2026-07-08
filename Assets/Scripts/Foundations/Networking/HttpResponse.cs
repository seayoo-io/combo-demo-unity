using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Networking
{
    public class HttpResponseBody {
        // Source A: UnityWebRequest（原路径，保留以防其他调用方）。
        private UnityWebRequest requestHandler;
        // Source B: HttpDnsWebRequest 返回的原始字节（HTTPDNS 路径）。
        private byte[] rawBytes;
        private readonly bool fromBytes;

        public HttpResponseBody(UnityWebRequest requestHandler) {
            this.requestHandler = requestHandler;
            this.fromBytes = false;
        }

        public HttpResponseBody(byte[] raw) {
            this.rawBytes = raw;
            this.fromBytes = true;
        }

        public Texture2D ToImage() {
            Texture2D texture = new Texture2D(10, 10);
            texture.LoadImage(ToRaw());
            return texture;
        }

        public byte[] ToRaw() => fromBytes ? (rawBytes ?? new byte[0]) : requestHandler.downloadHandler.data;

        public string ToText() {
            if (fromBytes) {
                return rawBytes == null ? "" : Encoding.UTF8.GetString(rawBytes);
            }
            return requestHandler.isNetworkError ? requestHandler.error : requestHandler.downloadHandler.text;
        }

        public T ToJson<T>() {
            var text = ToText();
            return JsonConvert.DeserializeObject<T>(text);
        }
    }

    public class HttpResponse
    {
        // 两种来源统一对外暴露 IsSuccess / StatusCode / Headers / Body。
        private UnityWebRequest requestHandler; // Source A，可能为 null（HTTPDNS 路径）
        private bool fromBytes;                 // true = Source B（HttpDnsWebRequest）

        // Source B 缓存的字段（原型 Response 不持有 UnityWebRequest）。
        private bool dnsIsSuccess;
        private int dnsStatusCode;
        private string dnsSucceededVia;
        private string dnsErrorMessage;

        private string requestBody;

        public int StatusCode => fromBytes ? dnsStatusCode : (int)requestHandler.responseCode;
        public Dictionary<string, string> Headers =>
            fromBytes ? new Dictionary<string, string>() : requestHandler.GetResponseHeaders();
        public Dictionary<string, string> RequestHeaders { get; private set; }
        public bool IsNetworkError => fromBytes ? !dnsIsSuccess && dnsStatusCode == 0 : requestHandler.isNetworkError;
        public bool IsHttpError => fromBytes ? !dnsIsSuccess && dnsStatusCode != 0 : requestHandler.isHttpError;
        public bool IsSuccess => fromBytes ? dnsIsSuccess : (!IsNetworkError && !IsHttpError);

        // HTTPDNS 命中打点：成功请求经哪个 IP（或 "direct"）完成。Source A 为 null。
        public string SucceededVia => fromBytes ? dnsSucceededVia : null;

        public HttpResponseBody Body { get; private set; }

        internal static HttpResponse Create(UnityWebRequest webRequest, Dictionary<string, string> requestHeaders, string requestBody = "")
        {
            return new HttpResponse
            {
                fromBytes = false,
                RequestHeaders = requestHeaders,
                requestHandler = webRequest,
                requestBody = requestBody,
                Body = new HttpResponseBody(webRequest),
            };
        }

        // 从 HttpDnsWebRequest 的 Response 构造（HTTPDNS 路径）。
        internal static HttpResponse Create(HttpDnsWebRequest.Response response, Dictionary<string, string> requestHeaders, string requestBody = "")
        {
            return new HttpResponse
            {
                fromBytes = true,
                RequestHeaders = requestHeaders,
                requestBody = requestBody,
                dnsIsSuccess = response.IsSuccess,
                dnsStatusCode = (int)response.StatusCode,
                dnsSucceededVia = response.SucceededVia,
                dnsErrorMessage = response.ErrorMessage,
                Body = new HttpResponseBody(response.RawBody),
            };
        }

        public override string ToString() {
            string reqHeaderString = RequestHeaders == null
            ? ""
            : string.Join("\n", RequestHeaders.Select(kvp => $"{kvp.Key}: {kvp.Value ?? ""}"));

            if (fromBytes) {
                return $@"REQUEST (via {dnsSucceededVia ?? "?"})
--headers:
{reqHeaderString}
--body:
{requestBody}

RESPONSE [{dnsStatusCode}]

--body:
{(dnsIsSuccess ? Body.ToText() : dnsErrorMessage)}
";
            }

            string resHeaderString = Headers == null
            ? ""
            : string.Join("\n", Headers.Select(kvp => $"{kvp.Key}: {kvp.Value ?? ""}"));
            return $@"REQUEST: {requestHandler?.url}
--headers:
{reqHeaderString}
--body:
{requestBody}

RESPONSE

--headers:
{resHeaderString}
--body:
{(requestHandler.isNetworkError ? requestHandler.error: requestHandler.downloadHandler?.text)}
";
        }
    }
}
