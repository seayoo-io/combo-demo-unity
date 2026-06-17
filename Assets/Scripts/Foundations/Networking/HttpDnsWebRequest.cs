using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Combo;
using UnityEngine;

namespace Networking
{
    // HTTPS with HTTPDNS and correct SNI, via C# HttpWebRequest:
    //
    //     URL      = https://<resolved-ip>/path   → TCP connects to the IP
    //     req.Host = "game.example.com"           → Host header + TLS SNI + cert validation
    //
    // Why HttpWebRequest and not UnityWebRequest: UnityWebRequest's native backends
    // (libcurl on Android, NSURLSession on iOS) derive SNI from the URL host, so
    // URL-rewriting to an IP breaks SNI-routed servers/CDNs, and they expose no DNS hook.
    // Mono's HttpWebRequest decouples the two: the TLS handshake host comes from the Host
    // property, not the URL. (See HttpDnsSniHostProbe.cs POC for the on-device evidence.)
    //
    // Policies:
    // - Candidate chain per hop: IPv6 IPs → IPv4 IPs → original domain URL (mandatory last
    //   step — covers Editor, resolution failure, and proxies that reject CONNECT-to-IP).
    // - Only connection-level failures (no server response) advance to the next candidate;
    //   any HTTP response, including 4xx/5xx, is final. TLS trust failures advance too.
    // - AllowAutoRedirect = false: redirects are followed manually, re-resolving each hop,
    //   so an explicitly-set Host/SNI never leaks to the redirect target.
    // - System proxies are intentionally ignored.
    //
    // Threading: the entire request — ResolveSync + blocking HttpWebRequest I/O — runs on
    // one ThreadPool worker per request, bounded by the timeout. The result is published
    // to the main thread via HttpRequestOperation (volatile + MemoryBarrier).
    public static class HttpDnsWebRequest
    {
        public enum ErrorType
        {
            AllAttemptsFailed,   // every candidate, including the direct domain attempt, failed at connection level
            ProtocolError,       // server responded with a non-2xx status
            TooManyRedirects,
            DataProcessingError, // unexpected failure preparing the request or reading the response
        }

        public class Response
        {
            public bool IsSuccess;
            public long StatusCode;
            public byte[] RawBody;

            // Error info (only set when IsSuccess == false)
            public ErrorType? Error;
            public string ErrorMessage;

            // Which attempt produced this response: a resolved IP, or "direct" (original
            // domain URL through system DNS). A rising "direct" share in metrics means
            // HTTPDNS results are failing to connect.
            public string SucceededVia;

            string _bodyText;
            public string Body =>
                _bodyText ?? (RawBody != null ? _bodyText = Encoding.UTF8.GetString(RawBody) : null);
        }

        const int DefaultTimeoutMs = 10000;
        const int MaxRedirects = 5;

        // HttpWebRequest restricted headers — must be set via property, not Headers.Add.
        static readonly HashSet<string> RestrictedHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Host", "Content-Type", "Content-Length", "Accept", "Connection",
            "User-Agent", "Referer", "Range", "If-Modified-Since", "Transfer-Encoding",
            "Proxy-Connection", "Date", "Expect",
        };

        static HttpDnsWebRequest()
        {
            // Each resolved IP gets its own ServicePoint (connection pool). Mono's default
            // of 2 concurrent connections per ServicePoint throttles parallel requests.
            ServicePointManager.DefaultConnectionLimit = 16;
        }

        // ===================================================================
        // Public API — operation style
        //   var op = HttpDnsWebRequest.Get(url);
        //   yield return op;
        //   var response = op.Response;
        // ===================================================================

        public static HttpRequestOperation Get(string url, Dictionary<string, string> headers = null, int timeoutMs = DefaultTimeoutMs)
        {
            return Send(url, "GET", null, null, headers, timeoutMs);
        }

        public static HttpRequestOperation Post(string url, byte[] body, string contentType, Dictionary<string, string> headers = null, int timeoutMs = DefaultTimeoutMs)
        {
            return Send(url, "POST", body, contentType, headers, timeoutMs);
        }

        public static HttpRequestOperation Send(string url, string method, byte[] body, string contentType, Dictionary<string, string> headers, int timeoutMs)
        {
            var op = new HttpRequestOperation();

            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    op.SetResponse(Execute(url, method, body, contentType, headers, timeoutMs));
                }
                catch (Exception e)
                {
                    op.SetResponse(new Response
                    {
                        IsSuccess = false,
                        Error = ErrorType.DataProcessingError,
                        ErrorMessage = e.Message,
                    });
                }
            });

            return op;
        }

        // ===================================================================
        // Public API — coroutine + callback style
        //   StartCoroutine(HttpDnsWebRequest.Get(url, response => { ... }));
        // ===================================================================

        public static IEnumerator Get(string url, Action<Response> callback, Dictionary<string, string> headers = null, int timeoutMs = DefaultTimeoutMs)
        {
            var op = Get(url, headers, timeoutMs);
            yield return op;
            callback?.Invoke(op.Response);
        }

        public static IEnumerator Post(string url, byte[] body, string contentType, Action<Response> callback, Dictionary<string, string> headers = null, int timeoutMs = DefaultTimeoutMs)
        {
            var op = Post(url, body, contentType, headers, timeoutMs);
            yield return op;
            callback?.Invoke(op.Response);
        }

        // ===================================================================
        // Worker — everything below runs on a ThreadPool thread
        // ===================================================================

        sealed class Outcome
        {
            public Response Response;     // terminal result, or
            public Uri RedirectTo;        // next hop (3xx with a Location header)
            public int RedirectStatus;
        }

        static Response Execute(string url, string method, byte[] body, string contentType, Dictionary<string, string> headers, int timeoutMs)
        {
            var uri = new Uri(url);

            for (int redirects = 0; ; redirects++)
            {
                if (redirects > MaxRedirects)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Error = ErrorType.TooManyRedirects,
                        ErrorMessage = $"More than {MaxRedirects} redirects starting from {url}",
                    };
                }

                var outcome = ExecuteHop(uri, method, body, contentType, headers, timeoutMs);
                if (outcome.Response != null)
                    return outcome.Response;

                uri = outcome.RedirectTo;

                // 303 always downgrades to GET; 301/302 downgrade POST to GET (de facto
                // standard); 307/308 preserve method and body.
                int status = outcome.RedirectStatus;
                if (status == 303 || ((status == 301 || status == 302) && method != "GET"))
                {
                    method = "GET";
                    body = null;
                    contentType = null;
                }
            }
        }

        // One redirect hop: resolve the host, try every candidate, return the first
        // server response (or AllAttemptsFailed).
        static Outcome ExecuteHop(Uri uri, string method, byte[] body, string contentType, Dictionary<string, string> headers, int timeoutMs)
        {
            var candidates = new List<string>();

            // Only resolve real domains — a hop to an IP-literal URL goes direct.
            if (Uri.CheckHostName(uri.Host) == UriHostNameType.Dns)
            {
                var dns = HttpDns.ResolveSync(uri.Host);
                if (dns?.IPv6 != null) candidates.AddRange(dns.IPv6);
                if (dns?.IPv4 != null) candidates.AddRange(dns.IPv4);
            }

            // Mandatory last attempt: original URL, system DNS. This is the Editor path,
            // the HTTPDNS-failure path, and the proxy-ACL self-heal path.
            candidates.Add(null);

            string lastError = null;

            foreach (var ip in candidates)
            {
                Uri attemptUri = ip == null ? uri : ReplaceHost(uri, ip);
                string hostOverride = ip == null ? null : uri.Host;
                string via = ip ?? "direct";

                try
                {
                    return DoRequest(attemptUri, hostOverride, method, body, contentType, headers, timeoutMs, via);
                }
                catch (Exception e)
                {
                    // No server response (connect/TLS/read failure) — try the next candidate.
                    lastError = $"[{via}] {e.Message}";
                }
            }

            return new Outcome
            {
                Response = new Response
                {
                    IsSuccess = false,
                    Error = ErrorType.AllAttemptsFailed,
                    ErrorMessage = $"All attempts failed for {uri.Host}. Last: {lastError}",
                },
            };
        }

        // Sends one request. Returns an Outcome for any server response (2xx, 3xx
        // redirect, 4xx/5xx). Throws only on connection-level failures, which the caller
        // treats as "advance to next candidate".
        static Outcome DoRequest(Uri attemptUri, string hostOverride, string method, byte[] body, string contentType, Dictionary<string, string> headers, int timeoutMs, string via)
        {
            var req = (HttpWebRequest)WebRequest.Create(attemptUri);

            // Host header + TLS SNI + certificate validation all follow this property.
            // Must be set before the request is sent. Non-default ports go in the Host
            // value ("host:port") for the wire header; TLS strips the port for SNI.
            if (hostOverride != null)
                req.Host = attemptUri.IsDefaultPort
                    ? hostOverride
                    : hostOverride + ":" + attemptUri.Port;

            req.Method = method;
            req.Timeout = timeoutMs;
            req.ReadWriteTimeout = timeoutMs;
            req.AllowAutoRedirect = false;
            req.KeepAlive = true;
            req.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            ApplyHeaders(req, headers);

            UnityEngine.Debug.Log($"[HTTPDNS-DIAG] {method} {attemptUri.AbsolutePath} via={via} headers applied, body={body?.Length ?? 0}B");

            if (body != null && body.Length > 0)
            {
                if (contentType != null)
                    req.ContentType = contentType;
                req.ContentLength = body.Length;
                UnityEngine.Debug.Log($"[HTTPDNS-DIAG] {method} {attemptUri.AbsolutePath} via={via} before GetRequestStream");
                using (var rs = req.GetRequestStream())
                    rs.Write(body, 0, body.Length);
                UnityEngine.Debug.Log($"[HTTPDNS-DIAG] {method} {attemptUri.AbsolutePath} via={via} after GetRequestStream (body written)");
            }
            else if (method != "GET")
            {
                // Bodyless POST: send an explicit zero length so servers don't 411.
                req.ContentLength = 0;
            }

            UnityEngine.Debug.Log($"[HTTPDNS-DIAG] {method} {attemptUri.AbsolutePath} via={via} before GetResponse");

            HttpWebResponse resp;
            try
            {
                resp = (HttpWebResponse)req.GetResponse();
                UnityEngine.Debug.Log($"[HTTPDNS-DIAG] {method} {attemptUri.AbsolutePath} via={via} after GetResponse, status={(int)resp.StatusCode}");
            }
            catch (WebException we) when (we.Response is HttpWebResponse errorResponse)
            {
                // The server responded (4xx/5xx) — final answer, not a reason to try
                // another IP. WebExceptions without a response propagate to the loop.
                resp = errorResponse;
            }

            using (resp)
            {
                int status = (int)resp.StatusCode;

                if (status >= 300 && status < 400)
                {
                    string location = resp.Headers["Location"];
                    if (!string.IsNullOrEmpty(location))
                    {
                        // attemptUri may hold an IP — resolve relative Locations against
                        // the original-host URI so the next hop re-resolves via HTTPDNS.
                        Uri baseUri = hostOverride == null
                            ? attemptUri
                            : new UriBuilder(attemptUri) { Host = hostOverride }.Uri;
                        return new Outcome { RedirectTo = new Uri(baseUri, location), RedirectStatus = status };
                    }
                }

                byte[] raw = ReadAll(resp);
                bool ok = status >= 200 && status < 300;

                return new Outcome
                {
                    Response = new Response
                    {
                        IsSuccess = ok,
                        StatusCode = status,
                        RawBody = raw,
                        Error = ok ? (ErrorType?)null : ErrorType.ProtocolError,
                        ErrorMessage = ok ? null : $"HTTP {status}",
                        SucceededVia = via,
                    },
                };
            }
        }

        // Apply caller headers. Restricted headers (Host/Content-Type/...) cannot go
        // through Headers.Add — they must be set via the dedicated property, or skipped
        // here when already handled (Host, Content-Type, Content-Length).
        static void ApplyHeaders(HttpWebRequest req, Dictionary<string, string> headers)
        {
            if (headers == null) return;

            foreach (var pair in headers)
            {
                if (string.IsNullOrEmpty(pair.Key)) continue;

                if (RestrictedHeaders.Contains(pair.Key))
                {
                    // Host / Content-Type / Content-Length are owned by DoRequest itself.
                    // Map the few that callers commonly set; ignore the rest to avoid
                    // throwing on restricted-header assignment.
                    if (string.Equals(pair.Key, "Accept", StringComparison.OrdinalIgnoreCase))
                        req.Accept = pair.Value;
                    else if (string.Equals(pair.Key, "User-Agent", StringComparison.OrdinalIgnoreCase))
                        req.UserAgent = pair.Value;
                    else if (string.Equals(pair.Key, "Content-Type", StringComparison.OrdinalIgnoreCase))
                        req.ContentType = pair.Value;
                    // Host/Content-Length/Connection/etc. intentionally skipped.
                    continue;
                }

                req.Headers[pair.Key] = pair.Value;
            }
        }

        static byte[] ReadAll(HttpWebResponse resp)
        {
            using (var stream = resp.GetResponseStream())
            {
                if (stream == null)
                    return null;

                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }

        static Uri ReplaceHost(Uri originalUri, string ip)
        {
            // UriBuilder adds brackets to IPv6 literals automatically.
            var builder = new UriBuilder(originalUri) { Host = ip };

            // UriBuilder makes the default port explicit (e.g. :443 for https) — strip it
            // to keep URLs clean.
            if (originalUri.IsDefaultPort)
                builder.Port = -1;

            return builder.Uri;
        }
    }

    // Cross-thread handoff for HTTP responses — same lock-free publication pattern as
    // Combo.HttpDnsResolveOperation: the worker writes _response, the memory barrier
    // commits it, the volatile _isDone write publishes it; the main thread polls
    // keepWaiting once per frame and reads Response only after observing _isDone.
    public class HttpRequestOperation : CustomYieldInstruction
    {
        private HttpDnsWebRequest.Response _response;
        private volatile bool _isDone;

        public HttpDnsWebRequest.Response Response => _response;
        public bool IsDone => _isDone;
        public override bool keepWaiting => !_isDone;

        internal void SetResponse(HttpDnsWebRequest.Response response)
        {
            _response = response;
            Thread.MemoryBarrier();
            _isDone = true;
        }
    }
}
