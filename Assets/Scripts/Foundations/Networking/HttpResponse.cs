using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Networking
{
    public class HttpResponseBody {
        private UnityWebRequest requestHandler;
        public Texture2D ToImage() {
            Texture2D texture = new Texture2D(10, 10);
            texture.LoadImage(ToRaw());
            return texture;
        }
        public byte[] ToRaw() => requestHandler.downloadHandler.data;
        public string ToText() => requestHandler.result == UnityWebRequest.Result.ConnectionError ? requestHandler.error : requestHandler.downloadHandler.text;
        public T ToJson<T>() {
            var text = requestHandler.downloadHandler.text;
            return JsonConvert.DeserializeObject<T>(text);
        }
        public HttpResponseBody(UnityWebRequest requestHandler) {
            this.requestHandler = requestHandler;
        }
    }
    public class HttpResponse
    {
        public int StatusCode => (int)requestHandler.responseCode;
        public Dictionary<string, string> Headers => requestHandler.GetResponseHeaders();
        public Dictionary<string, string> RequestHeaders { get; private set; }
        public bool IsNetworkError => requestHandler.result == UnityWebRequest.Result.ConnectionError;
        public bool IsHttpError => requestHandler.result == UnityWebRequest.Result.ProtocolError;
        public bool IsSuccess => !IsNetworkError && !IsHttpError;
        public HttpResponseBody Body { get; private set; }
        private UnityWebRequest requestHandler;
        private string requestBody;

        internal static HttpResponse Create(UnityWebRequest webRequest, Dictionary<string, string> requestHeaders, string requestBody = "")
        {
            return new HttpResponse
            {
                RequestHeaders = requestHeaders,
                requestHandler = webRequest,
                requestBody = requestBody,
                Body = new HttpResponseBody(webRequest),
            };
        }

        public override string ToString() {
            string reqHeaderString = RequestHeaders == null 
            ? "" 
            : string.Join("\n", RequestHeaders.Select(kvp => $"{kvp.Key}: {kvp.Value ?? ""}"));
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
{(requestHandler.result == UnityWebRequest.Result.ConnectionError ? requestHandler.error: requestHandler.downloadHandler?.text)}
";
        }
    }
}
