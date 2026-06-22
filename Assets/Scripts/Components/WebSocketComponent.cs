using System;
using Combo;
using Networking;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class WebSocketComponent : MonoBehaviour
{
// HTTPDNS wss runs over raw sockets/TLS, which WebGL's runtime doesn't provide. ws on this
// platform is out of scope (see HttpDnsWebSocket), so the component is inert on WebGL while
// staying a valid MonoBehaviour so the scene reference survives.
#if !UNITY_WEBGL || UNITY_EDITOR
    private HttpDnsWebSocket webSocket;
    private float lastPingTime;
    private float lastPongTime;
    private string address = "";
    private bool reconnecting;

    void Start()
    {
        var demoEndpoint = GameClient.GetClientEndPoint().Replace("https://", "");
        address = $"wss://{demoEndpoint}:443/{ComboSDK.GetGameId()}/ws/{ComboSDK.GetLoginInfo().comboId}";
        OpenSocket();
        InvokeRepeating("SendPingFrame", 1f, 2f);
        InvokeRepeating("CheckConnectionStatus", 2f, 2f);
    }

    void Update()
    {
        // HttpDnsWebSocket runs on background threads; pump its callbacks onto the main thread.
        webSocket?.ProcessMessageQueue();
    }

    void OnDestroy()
    {
        CloseSocket();
    }

    private void OpenSocket()
    {
        // Reset the heartbeat clock so CheckConnectionStatus doesn't fire a false "lost"
        // before the first PONG arrives (lastPongTime used to start at 0 → instant reconnect).
        float now = Time.time;
        lastPingTime = now;
        lastPongTime = now;

        webSocket = new HttpDnsWebSocket(address);
        webSocket.OnOpen += OnOpen;
        webSocket.OnMessage += OnMessage;
        webSocket.OnClose += OnClose;
        webSocket.OnError += OnError;
        webSocket.Connect();
    }

    private void CloseSocket()
    {
        if (webSocket == null) return;
        // Unsubscribe before closing so the old instance's late callbacks can't drive
        // reconnect logic against a socket we're discarding.
        webSocket.OnOpen -= OnOpen;
        webSocket.OnMessage -= OnMessage;
        webSocket.OnClose -= OnClose;
        webSocket.OnError -= OnError;
        webSocket.Close();
        webSocket = null;
    }

    private void OnOpen()
    {
        reconnecting = false;
        lastPongTime = Time.time;
        Log.I($"WebSocket connection opened. via={webSocket?.Via}");
    }

    private void OnMessage(string data)
    {
        if (data != "PONG")
        {
            MailListManager.Instance.SaveMail(ParseMail(data));
            Message.Show(data);
        }
        else
        {
            lastPongTime = Time.time;
        }
    }

    private void OnClose(ushort code, string reason)
    {
        Log.I($"WebSocket connection closed. code={code}, reason={reason}");
    }

    private void OnError(string error)
    {
        Log.E($"WebSocket error: {error}");
    }

    private void SendPingFrame()
    {
        if (webSocket == null || webSocket.State != HttpDnsWebSocket.ReadyState.Open) return;
        webSocket.Send("PING");
        lastPingTime = Time.time;
    }

    private void CheckConnectionStatus()
    {
        float timeSinceLastPing = Time.time - lastPingTime;
        float timeSinceLastPong = Time.time - lastPongTime;
        if (reconnecting) return;
        if (timeSinceLastPing > 3f || timeSinceLastPong > 3f)
        {
            Log.I("WebSocket connection lost, trying to reconnect...");
            reconnecting = true;
            CloseSocket();   // fully tear down (unsubscribe + close) before reopening
            OpenSocket();
        }
    }

    private MailInfo ParseMail(string jsonString)
    {
        JObject jsonObject = JObject.Parse(jsonString);

        MailInfo mailInfo = jsonObject.ToObject<MailInfo>();
        ReceivedMailEvent.Invoke(new ReceivedMailEvent{
            mailInfo = mailInfo
        });
        return mailInfo;
    }
#endif
}
