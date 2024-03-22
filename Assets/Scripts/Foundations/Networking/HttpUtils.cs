using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Types;
using UnityEngine.Networking;

namespace Networking
{
    public class HttpUtils
    {
        public static string ParseQueryString(Serializable obj)
        {
            var jObject = JObject.Parse(obj.ToJson());
            var keyValuePairs = new List<string>();

            foreach (var key in jObject.Properties())
            {
                var value = jObject[key.Name];
                keyValuePairs.Add(key.Name + "=" + UrlEncode(value.ToString()));
            }

            return string.Join("&", keyValuePairs);
        }

        public static string UrlEncode(string url)
        {
            return UnityWebRequest.EscapeURL(url);
        }
    }
}
