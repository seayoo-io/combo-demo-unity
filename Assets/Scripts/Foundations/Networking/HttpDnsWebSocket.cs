#if !UNITY_WEBGL || UNITY_EDITOR
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using Combo;

namespace Networking
{
    // wss with HTTPDNS and correct SNI, via a from-scratch RFC6455 client (SimpleWeb style).
    //
    //     TCP connect → <resolved-ip>          (HttpDns.ResolveSync, IPv6 → IPv4 → direct)
    //     TLS SNI     → original domain         (SslStream.AuthenticateAsClient(domain))
    //     Host header → original domain
    //
    // Why not the psygames UnityWebSocket lib: it wraps System.Net.WebSockets.ClientWebSocket,
    // which derives SNI from the URI host. Rewriting the URL host to an IP (the HTTP HTTPDNS
    // trick) then makes SNI = IP, and the server returns a cert for the domain → name mismatch
    // → handshake fails. ClientWebSocket exposes no hook to separate the connect target from
    // the SNI/Host. So, like HttpDnsWebRequest does for HTTPS, we drive TCP + TLS + the HTTP
    // upgrade handshake ourselves and keep the domain for SNI/Host while connecting to the IP.
    //
    // Candidate chain mirrors HttpDnsWebRequest: resolved IPv6 → resolved IPv4 → original
    // domain (system DNS, mandatory last step — Editor / resolution failure / fake-ip self-heal).
    // Only connection-level failures (connect/TLS/handshake) advance to the next candidate.
    //
    // Threading: a receive thread does resolve + connect + handshake + the read loop; a send
    // thread drains the send queue. All callbacks are marshalled to the main thread via an
    // event queue — call ProcessMessageQueue() once per frame (e.g. from MonoBehaviour.Update).
    // WebGL is unsupported (browser WebSocket can't set Host/SNI); this file compiles out there.
    public class HttpDnsWebSocket
    {
        public enum ReadyState { Connecting, Open, Closing, Closed }

        public event Action OnOpen;
        public event Action<string> OnMessage;      // text frames, UTF-8 decoded
        public event Action<string> OnError;
        public event Action<ushort, string> OnClose; // close code + reason

        public ReadyState State => _state;
        public string Via => _via;                   // which candidate connected: an IP, or "direct"

        const int ConnectTimeoutMs = 5000;
        const int ReadWriteTimeoutMs = 0;            // 0 = infinite; the ping/pong layer detects stalls
        const int MaxFrameSize = 64 * 1024;
        const string HandshakeGuid = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

        readonly string _url;
        readonly Uri _uri;

        volatile ReadyState _state = ReadyState.Closed;
        volatile string _via;

        Thread _receiveThread;
        Thread _sendThread;
        TcpClient _tcp;
        Stream _stream;

        readonly object _sendLock = new object();
        readonly Queue<byte[]> _sendQueue = new Queue<byte[]>();
        readonly AutoResetEvent _sendSignal = new AutoResetEvent(false);
        volatile bool _running;
        volatile bool _closeRequested;   // local Close(): send the close frame then tear down

        // event_type + payload, drained on the main thread by ProcessMessageQueue
        enum Evt { Open, Message, Error, Close }
        struct QueuedEvent { public Evt type; public string text; public ushort code; }
        readonly ConcurrentQueue<QueuedEvent> _events = new ConcurrentQueue<QueuedEvent>();

        public HttpDnsWebSocket(string url)
        {
            _url = url;
            _uri = new Uri(url);
        }

        // ===================================================================
        // Public API
        // ===================================================================

        public void Connect()
        {
            if (_state != ReadyState.Closed) return;
            _state = ReadyState.Connecting;
            _running = true;

            _receiveThread = new Thread(ConnectAndReceiveLoop)
            {
                IsBackground = true,
                Name = "HttpDnsWebSocket-recv",
            };
            _receiveThread.Start();
        }

        public void Send(string text)
        {
            if (_state != ReadyState.Open) return;
            byte[] frame = EncodeFrame(Opcode.Text, Encoding.UTF8.GetBytes(text));
            lock (_sendLock)
            {
                _sendQueue.Enqueue(frame);
            }
            _sendSignal.Set();
        }

        public void Close()
        {
            var prev = _state;
            if (prev == ReadyState.Closed || prev == ReadyState.Closing) return;
            _state = ReadyState.Closing;

            if (prev != ReadyState.Open)
            {
                // Connecting but not yet open — just tear everything down.
                Shutdown();
                return;
            }

            // Single-writer teardown: queue a normal close frame and flag the send thread to
            // dispose the socket once it's written. Disposing there (not waiting for the server
            // to echo a close) unblocks the receive thread's Read so it can't leak.
            _closeRequested = true;
            byte[] frame = EncodeFrame(Opcode.Close, EncodeCloseBody(1000, "Normal Closure"));
            lock (_sendLock)
            {
                _sendQueue.Enqueue(frame);
            }
            _sendSignal.Set();
        }

        // Drains queued callbacks onto the calling (main) thread. Call once per frame.
        public void ProcessMessageQueue()
        {
            while (_events.TryDequeue(out var e))
            {
                switch (e.type)
                {
                    case Evt.Open: OnOpen?.Invoke(); break;
                    case Evt.Message: OnMessage?.Invoke(e.text); break;
                    case Evt.Error: OnError?.Invoke(e.text); break;
                    case Evt.Close: OnClose?.Invoke(e.code, e.text); break;
                }
            }
        }

        // ===================================================================
        // Connect (HTTPDNS candidate chain) + receive loop — on the receive thread
        // ===================================================================

        void ConnectAndReceiveLoop()
        {
            string lastError = null;
            foreach (var ip in BuildCandidates())
            {
                if (!_running) return;

                string connectHost = ip ?? _uri.Host;   // IP for resolved candidates, domain for direct
                string via = ip ?? "direct";
                try
                {
                    OpenConnection(connectHost);          // SNI/Host always use _uri.Host (the domain)
                    _via = via;
                    break;
                }
                catch (Exception e)
                {
                    lastError = $"[{via}] {e.Message}";
                    CloseSocketQuietly();
                    // try next candidate
                }
            }

            if (_stream == null)
            {
                _state = ReadyState.Closed;
                _running = false;
                Enqueue(new QueuedEvent { type = Evt.Error, text = $"All attempts failed for {_uri.Host}. Last: {lastError}" });
                Enqueue(new QueuedEvent { type = Evt.Close, code = 1006, text = "connect failed" });
                return;
            }

            _state = ReadyState.Open;
            Enqueue(new QueuedEvent { type = Evt.Open });

            // start the send thread now that the stream exists
            _sendThread = new Thread(SendLoop) { IsBackground = true, Name = "HttpDnsWebSocket-send" };
            _sendThread.Start();

            ushort closeCode = 1006;
            string closeReason = "";
            try
            {
                ReceiveLoop(ref closeCode, ref closeReason);
            }
            catch (Exception e)
            {
                // A local Close() disposes the socket from the send thread, which surfaces here
                // as a read exception — that's expected teardown, not an error to report.
                if (!_closeRequested)
                    Enqueue(new QueuedEvent { type = Evt.Error, text = e.Message });
                closeCode = _closeRequested ? (ushort)1000 : (ushort)1006;
                closeReason = _closeRequested ? "Normal Closure" : e.Message;
            }
            finally
            {
                _state = ReadyState.Closed;
                _running = false;
                _sendSignal.Set();   // wake the send thread so it can exit
                CloseSocketQuietly();
                Enqueue(new QueuedEvent { type = Evt.Close, code = closeCode, text = closeReason });
            }
        }

        // resolved IPv6 → resolved IPv4 → direct(null). Same policy as HttpDnsWebRequest.
        List<string> BuildCandidates()
        {
            var candidates = new List<string>();
            if (Uri.CheckHostName(_uri.Host) == UriHostNameType.Dns)
            {
                HttpDnsResult dns = null;
                try { dns = HttpDns.ResolveSync(_uri.Host); } catch { /* fall through to direct */ }
                if (dns?.IPv6 != null) candidates.AddRange(dns.IPv6);
                if (dns?.IPv4 != null) candidates.AddRange(dns.IPv4);
            }
            candidates.Add(null); // mandatory direct attempt (original domain, system DNS)
            return candidates;
        }

        // TCP connect to connectHost, TLS with SNI = domain, then the HTTP upgrade handshake.
        void OpenConnection(string connectHost)
        {
            _tcp = new TcpClient { NoDelay = true };

            IAsyncResult ar = _tcp.BeginConnect(connectHost, _uri.Port, null, null);
            if (!ar.AsyncWaitHandle.WaitOne(ConnectTimeoutMs))
            {
                _tcp.Close();
                throw new TimeoutException($"TCP connect to {connectHost}:{_uri.Port} timed out");
            }
            _tcp.EndConnect(ar);

            Stream stream = _tcp.GetStream();
            if (ReadWriteTimeoutMs > 0)
            {
                stream.ReadTimeout = ReadWriteTimeoutMs;
                stream.WriteTimeout = ReadWriteTimeoutMs;
            }

            bool secure = _uri.Scheme == "wss" || _uri.Scheme == "https";
            if (secure)
            {
                var ssl = new SslStream(stream, false, ValidateServerCertificate);
                // SNI = the original domain (NOT connectHost, which may be an IP). This is the
                // whole point: TLS authenticates against the domain while TCP rode on the IP.
                ssl.AuthenticateAsClient(_uri.Host);
                stream = ssl;
            }

            Handshake(stream);
            _stream = stream;
        }

        // Strict: only trust a fully valid chain. Because SNI = domain, the server's domain
        // cert validates normally even though we connected to an IP.
        static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return sslPolicyErrors == SslPolicyErrors.None;
        }

        void Handshake(Stream stream)
        {
            byte[] keyBytes = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
                rng.GetBytes(keyBytes);
            string key = Convert.ToBase64String(keyBytes);
            string expectedAccept = Convert.ToBase64String(
                SHA1.Create().ComputeHash(Encoding.ASCII.GetBytes(key + HandshakeGuid)));

            // Host header carries the domain (with non-default port appended), never the IP.
            string hostHeader = _uri.IsDefaultPort ? _uri.Host : $"{_uri.Host}:{_uri.Port}";
            string request =
                $"GET {_uri.PathAndQuery} HTTP/1.1\r\n" +
                $"Host: {hostHeader}\r\n" +
                "Upgrade: websocket\r\n" +
                "Connection: Upgrade\r\n" +
                $"Sec-WebSocket-Key: {key}\r\n" +
                "Sec-WebSocket-Version: 13\r\n" +
                "\r\n";
            byte[] reqBytes = Encoding.ASCII.GetBytes(request);
            stream.Write(reqBytes, 0, reqBytes.Length);
            stream.Flush();

            string response = ReadHttpResponseHeader(stream);
            if (response.IndexOf(" 101", StringComparison.Ordinal) < 0)
                throw new Exception($"Handshake failed, no 101 status:\n{response}");

            const string acceptHeader = "sec-websocket-accept:";
            int idx = response.ToLowerInvariant().IndexOf(acceptHeader, StringComparison.Ordinal);
            if (idx < 0)
                throw new Exception("Handshake failed, missing Sec-WebSocket-Accept");
            idx += acceptHeader.Length;
            int end = response.IndexOf("\r\n", idx, StringComparison.Ordinal);
            string accept = response.Substring(idx, end - idx).Trim();
            if (accept != expectedAccept)
                throw new Exception("Handshake failed, Sec-WebSocket-Accept mismatch");
        }

        // Reads bytes one at a time until the CRLFCRLF header terminator. The handshake is a
        // one-shot, low-volume read, so byte-at-a-time is fine and avoids over-reading into
        // the first WebSocket frame.
        static string ReadHttpResponseHeader(Stream stream)
        {
            var sb = new StringBuilder();
            int matched = 0;
            byte[] one = new byte[1];
            while (matched < 4)
            {
                int n = stream.Read(one, 0, 1);
                if (n <= 0) throw new Exception("Connection closed during handshake");
                char c = (char)one[0];
                sb.Append(c);
                // match \r\n\r\n
                if ((matched == 0 && c == '\r') || (matched == 2 && c == '\r')) matched++;
                else if ((matched == 1 && c == '\n') || (matched == 3 && c == '\n')) matched++;
                else matched = (c == '\r') ? 1 : 0;
                if (sb.Length > 8192) throw new Exception("Handshake response too large");
            }
            return sb.ToString();
        }

        // ===================================================================
        // Receive loop — RFC6455 frame decode + reassembly
        // ===================================================================

        enum Opcode { Continuation = 0x0, Text = 0x1, Binary = 0x2, Close = 0x8, Ping = 0x9, Pong = 0xA }

        void ReceiveLoop(ref ushort closeCode, ref string closeReason)
        {
            var fragment = new MemoryStream();
            Opcode fragmentOpcode = Opcode.Continuation;
            byte[] header = new byte[2];
            byte[] lenBuf = new byte[8];

            while (_running)
            {
                ReadExact(header, 2);
                bool fin = (header[0] & 0x80) != 0;
                var opcode = (Opcode)(header[0] & 0x0F);
                bool masked = (header[1] & 0x80) != 0;   // server frames must NOT be masked
                long len = header[1] & 0x7F;

                if (len == 126)
                {
                    ReadExact(lenBuf, 2);
                    len = (lenBuf[0] << 8) | lenBuf[1];
                }
                else if (len == 127)
                {
                    ReadExact(lenBuf, 8);
                    len = 0;
                    for (int i = 0; i < 8; i++) len = (len << 8) | lenBuf[i];
                }

                if (len > MaxFrameSize)
                    throw new Exception($"Frame too large: {len} > {MaxFrameSize}");

                byte[] maskKey = null;
                if (masked)
                {
                    maskKey = new byte[4];
                    ReadExact(maskKey, 4);
                }

                byte[] payload = new byte[len];
                if (len > 0) ReadExact(payload, (int)len);
                if (maskKey != null)
                    for (int i = 0; i < payload.Length; i++) payload[i] ^= maskKey[i & 3];

                switch (opcode)
                {
                    case Opcode.Ping:
                        EnqueueRaw(EncodeFrame(Opcode.Pong, payload)); // protocol-level pong
                        break;

                    case Opcode.Pong:
                        break; // app-level PONG arrives as a Text frame in this demo; ignore WS pong

                    case Opcode.Close:
                        if (payload.Length >= 2)
                        {
                            closeCode = (ushort)((payload[0] << 8) | payload[1]);
                            closeReason = payload.Length > 2 ? Encoding.UTF8.GetString(payload, 2, payload.Length - 2) : "";
                        }
                        else { closeCode = 1000; }
                        // echo a close and stop
                        EnqueueRaw(EncodeFrame(Opcode.Close, EncodeCloseBody(closeCode, "")));
                        _running = false;
                        return;

                    case Opcode.Text:
                    case Opcode.Binary:
                        if (fin)
                        {
                            DeliverMessage(opcode, payload, 0, payload.Length);
                        }
                        else
                        {
                            fragment.SetLength(0);
                            fragmentOpcode = opcode;
                            fragment.Write(payload, 0, payload.Length);
                        }
                        break;

                    case Opcode.Continuation:
                        fragment.Write(payload, 0, payload.Length);
                        if (fin)
                        {
                            byte[] full = fragment.ToArray();
                            DeliverMessage(fragmentOpcode, full, 0, full.Length);
                            fragment.SetLength(0);
                        }
                        break;
                }
            }
        }

        void DeliverMessage(Opcode opcode, byte[] data, int offset, int count)
        {
            // The demo speaks text (app-level "PONG" and mail JSON). Binary is decoded as
            // UTF-8 too so callers get a single string channel.
            string text = Encoding.UTF8.GetString(data, offset, count);
            Enqueue(new QueuedEvent { type = Evt.Message, text = text });
        }

        void ReadExact(byte[] buffer, int count)
        {
            int read = 0;
            while (read < count)
            {
                int n = _stream.Read(buffer, read, count - read);
                if (n <= 0) throw new IOException("Connection closed by remote");
                read += n;
            }
        }

        // ===================================================================
        // Send loop
        // ===================================================================

        void SendLoop()
        {
            try
            {
                while (_running || _sendQueue.Count > 0)
                {
                    _sendSignal.WaitOne(1000);
                    while (true)
                    {
                        byte[] frame;
                        lock (_sendLock)
                        {
                            if (_sendQueue.Count == 0) break;
                            frame = _sendQueue.Dequeue();
                        }
                        _stream.Write(frame, 0, frame.Length);
                        _stream.Flush();
                    }
                    if (_closeRequested)
                    {
                        // Close frame has been flushed — tear the socket down so the receive
                        // thread's blocking Read returns instead of waiting on a server echo.
                        _running = false;
                        CloseSocketQuietly();
                        break;
                    }
                    if (!_running) break;
                }
            }
            catch (Exception e)
            {
                if (!_closeRequested)
                    Enqueue(new QueuedEvent { type = Evt.Error, text = "send: " + e.Message });
                _running = false;
            }
        }

        void EnqueueRaw(byte[] frame)
        {
            lock (_sendLock) { _sendQueue.Enqueue(frame); }
            _sendSignal.Set();
        }

        // ===================================================================
        // Frame encoding (client → server is always masked)
        // ===================================================================

        static byte[] EncodeFrame(Opcode opcode, byte[] payload)
        {
            int len = payload?.Length ?? 0;
            int headerLen = 2 + (len <= 125 ? 0 : len <= ushort.MaxValue ? 2 : 8) + 4; // +4 mask key
            byte[] frame = new byte[headerLen + len];

            frame[0] = (byte)(0x80 | (byte)opcode); // FIN + opcode
            int pos;
            if (len <= 125)
            {
                frame[1] = (byte)(0x80 | len);       // MASK + len
                pos = 2;
            }
            else if (len <= ushort.MaxValue)
            {
                frame[1] = 0x80 | 126;
                frame[2] = (byte)(len >> 8);
                frame[3] = (byte)len;
                pos = 4;
            }
            else
            {
                frame[1] = 0x80 | 127;
                long l = len;
                for (int i = 7; i >= 0; i--) { frame[2 + i] = (byte)(l & 0xFF); l >>= 8; }
                pos = 10;
            }

            byte[] maskKey = new byte[4];
            using (var rng = new RNGCryptoServiceProvider())
                rng.GetBytes(maskKey);
            Buffer.BlockCopy(maskKey, 0, frame, pos, 4);
            pos += 4;

            for (int i = 0; i < len; i++)
                frame[pos + i] = (byte)(payload[i] ^ maskKey[i & 3]);

            return frame;
        }

        static byte[] EncodeCloseBody(ushort code, string reason)
        {
            byte[] reasonBytes = string.IsNullOrEmpty(reason) ? Array.Empty<byte>() : Encoding.UTF8.GetBytes(reason);
            byte[] body = new byte[2 + reasonBytes.Length];
            body[0] = (byte)(code >> 8);
            body[1] = (byte)code;
            Buffer.BlockCopy(reasonBytes, 0, body, 2, reasonBytes.Length);
            return body;
        }

        // ===================================================================
        // Teardown
        // ===================================================================

        void Shutdown()
        {
            _running = false;
            _sendSignal.Set();
            CloseSocketQuietly();
            _state = ReadyState.Closed;
        }

        void CloseSocketQuietly()
        {
            try { _stream?.Dispose(); } catch { }
            try { _tcp?.Close(); } catch { }
            _stream = null;
            _tcp = null;
        }

        void Enqueue(QueuedEvent e) => _events.Enqueue(e);
    }
}
#endif
