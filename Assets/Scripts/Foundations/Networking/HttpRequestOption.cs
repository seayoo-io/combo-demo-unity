// {Leslie}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Types;

namespace Networking
{
    public class HttpRequestOptions
    {
        public string url = "";
        public Serializable body = null;
        public Dictionary<string, string> headers = new Dictionary<string, string>();
        public HttpAuth auth = null;
        public Dictionary<string, string> cookies = new Dictionary<string, string>();
        public int timeout = 0;
    }

    public abstract class HttpAuth
    {
        public abstract string Get();
    }

    public class HttpBasicAuth : HttpAuth
    {
        private string authMethod = "Basic";
        private string username;
        private string password;

        public HttpBasicAuth(string username, string password)
        {
            this.username = username;
            this.password = password;
        }

        override public string Get()
        {
            var authKey = Crypto.Base64Encode($"{username}:{password}");
            return $"{authMethod} {authKey}";
        }

    }
}
