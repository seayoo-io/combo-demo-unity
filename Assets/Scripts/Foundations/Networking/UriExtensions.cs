using System;

namespace Networking
{
    public static class UriExtensions
    {
        public static Uri ToPlayerWebSocketUri(this Uri endpoint, string gameId, string comboId)
        {
            if (endpoint == null)
            {
                throw new ArgumentNullException(nameof(endpoint));
            }

            if (endpoint.Scheme != Uri.UriSchemeHttp && endpoint.Scheme != Uri.UriSchemeHttps)
            {
                throw new ArgumentException($"unsupported demo endpoint scheme '{endpoint.Scheme}'.", nameof(endpoint));
            }

            if (string.IsNullOrEmpty(gameId))
            {
                throw new ArgumentException("gameId is empty.", nameof(gameId));
            }

            if (string.IsNullOrEmpty(comboId))
            {
                throw new ArgumentException("comboId is empty.", nameof(comboId));
            }

            var builder = new UriBuilder(endpoint)
            {
                Scheme = endpoint.Scheme == Uri.UriSchemeHttps ? "wss" : "ws",
                Query = "",
                Fragment = ""
            };
            if (endpoint.IsDefaultPort)
            {
                builder.Port = -1;
            }

            var basePath = builder.Path.Trim('/');
            var playerPath = $"{Uri.EscapeDataString(gameId)}/ws/{Uri.EscapeDataString(comboId)}";
            builder.Path = string.IsNullOrEmpty(basePath) ? playerPath : $"{basePath}/{playerPath}";
            return builder.Uri;
        }
    }
}
