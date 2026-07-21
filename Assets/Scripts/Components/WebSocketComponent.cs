using System;
using Combo;
using Networking;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityWebSocket;

public class WebSocketComponent : MonoBehaviour
{
    private const float PingIntervalSeconds = 2f;
    private const float PongTimeoutSeconds = 6f;
    private const float ConnectTimeoutSeconds = 10f;
    private const float ReconnectDelaySeconds = 3f;

    private IWebSocket webSocket;
    private float connectStartedAt;
    private float lastPongAt;
    private float nextReconnectAt;
    private string address = "";
    private bool isShuttingDown;
    private bool heartbeatConfirmed;

    private void Start()
    {
        WebSocketCompatibility.Prepare();

        if (!TryCreateAddress(out address))
        {
            enabled = false;
            return;
        }

        Connect();
        InvokeRepeating(nameof(SendPingFrame), PingIntervalSeconds, PingIntervalSeconds);
        InvokeRepeating(nameof(CheckConnectionStatus), 1f, 1f);
    }

    private void OnDestroy()
    {
        isShuttingDown = true;
        CancelInvoke();
        ReleaseSocket(webSocket, true);
    }

    private bool TryCreateAddress(out string webSocketAddress)
    {
        webSocketAddress = "";

        var loginInfo = ComboSDK.GetLoginInfo();
        var gameId = ComboSDK.GetGameId();
        var endpoint = GameClient.GetClientEndPoint();
        if (loginInfo == null || string.IsNullOrEmpty(loginInfo.comboId))
        {
            Log.E("Cannot connect player WebSocket: comboId is empty.");
            return false;
        }

        if (string.IsNullOrEmpty(gameId))
        {
            Log.E("Cannot connect player WebSocket: gameId is empty.");
            return false;
        }

        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var endpointUri))
        {
            Log.E($"Cannot connect player WebSocket: invalid demo endpoint '{endpoint}'.");
            return false;
        }

        try
        {
            webSocketAddress = endpointUri.ToPlayerWebSocketUri(gameId, loginInfo.comboId).AbsoluteUri;
            return true;
        }
        catch (ArgumentException exception)
        {
            Log.E($"Cannot connect player WebSocket: {exception.Message}");
            return false;
        }
    }

    private void Connect()
    {
        if (isShuttingDown)
        {
            return;
        }

        if (webSocket != null)
        {
            var state = webSocket.ReadyState;
            if (state == WebSocketState.Connecting || state == WebSocketState.Open)
            {
                return;
            }

            ReleaseSocket(webSocket, false);
        }

        var socket = new WebSocket(address);
        webSocket = socket;
        socket.OnOpen += OnOpen;
        socket.OnMessage += OnMessage;
        socket.OnClose += OnClose;
        socket.OnError += OnError;
        connectStartedAt = Time.realtimeSinceStartup;

        Log.I($"Connecting player WebSocket: {address}");
        try
        {
            socket.ConnectAsync();
        }
        catch (Exception exception)
        {
            Log.E($"Player WebSocket connect failed: {exception.Message}");
            ReleaseSocket(socket, true);
            ScheduleReconnect();
        }
    }

    private void OnOpen(object sender, OpenEventArgs e)
    {
        if (!ReferenceEquals(sender, webSocket))
        {
            return;
        }

        heartbeatConfirmed = false;
        lastPongAt = Time.realtimeSinceStartup;
        Log.I($"Player WebSocket handshake completed: {address}");
    }

    private void OnMessage(object sender, MessageEventArgs e)
    {
        if (!ReferenceEquals(sender, webSocket))
        {
            return;
        }

        if (e.Data == "PONG")
        {
            lastPongAt = Time.realtimeSinceStartup;
            if (!heartbeatConfirmed)
            {
                heartbeatConfirmed = true;
                Log.I("Player WebSocket heartbeat confirmed.");
            }
            return;
        }

        try
        {
            MailListManager.Instance.SaveMail(ParseMail(e.Data));
            Message.Show(e.Data);
        }
        catch (Exception exception)
        {
            Log.E($"Failed to parse player WebSocket message: {exception.Message}");
        }
    }

    private void OnClose(object sender, CloseEventArgs e)
    {
        if (!ReferenceEquals(sender, webSocket))
        {
            return;
        }

        Log.W($"Player WebSocket closed: code={e.Code}, reason={e.Reason}");
        ReleaseSocket((IWebSocket)sender, false);
        ScheduleReconnect();
    }

    private void OnError(object sender, ErrorEventArgs e)
    {
        if (!ReferenceEquals(sender, webSocket))
        {
            return;
        }

        Log.E($"Player WebSocket error: {e.Message}");
    }

    private void SendPingFrame()
    {
        var socket = webSocket;
        if (socket == null || socket.ReadyState != WebSocketState.Open)
        {
            return;
        }

        try
        {
            socket.SendAsync("PING");
        }
        catch (Exception exception)
        {
            RestartConnection($"failed to send PING: {exception.Message}");
        }
    }

    private void CheckConnectionStatus()
    {
        if (isShuttingDown)
        {
            return;
        }

        var now = Time.realtimeSinceStartup;
        var socket = webSocket;
        if (socket == null)
        {
            if (now >= nextReconnectAt)
            {
                Connect();
            }
            return;
        }

        switch (socket.ReadyState)
        {
            case WebSocketState.Connecting:
                if (now - connectStartedAt >= ConnectTimeoutSeconds)
                {
                    RestartConnection("connect timeout");
                }
                break;
            case WebSocketState.Open:
                if (now - lastPongAt >= PongTimeoutSeconds)
                {
                    RestartConnection("PONG timeout");
                }
                break;
            case WebSocketState.Closing:
                break;
            case WebSocketState.Closed:
                ReleaseSocket(socket, false);
                ScheduleReconnect();
                break;
            default:
                RestartConnection($"invalid state: {socket.ReadyState}");
                break;
        }
    }

    private void RestartConnection(string reason)
    {
        Log.W($"Player WebSocket disconnected ({reason}), reconnecting in {ReconnectDelaySeconds} seconds.");
        ReleaseSocket(webSocket, true);
        ScheduleReconnect();
    }

    private void ScheduleReconnect()
    {
        if (!isShuttingDown)
        {
            nextReconnectAt = Time.realtimeSinceStartup + ReconnectDelaySeconds;
        }
    }

    private void ReleaseSocket(IWebSocket socket, bool close)
    {
        if (socket == null)
        {
            return;
        }

        socket.OnOpen -= OnOpen;
        socket.OnMessage -= OnMessage;
        socket.OnClose -= OnClose;
        socket.OnError -= OnError;

        if (close && socket.ReadyState != WebSocketState.Closed && socket.ReadyState != WebSocketState.Closing)
        {
            try
            {
                socket.CloseAsync();
            }
            catch (Exception exception)
            {
                Log.W($"Failed to close player WebSocket: {exception.Message}");
            }
        }

        if (ReferenceEquals(socket, webSocket))
        {
            webSocket = null;
        }
    }

    private MailInfo ParseMail(string jsonString)
    {
        var jsonObject = JObject.Parse(jsonString);
        var mailInfo = jsonObject.ToObject<MailInfo>();
        ReceivedMailEvent.Invoke(new ReceivedMailEvent
        {
            mailInfo = mailInfo
        });
        return mailInfo;
    }
}
