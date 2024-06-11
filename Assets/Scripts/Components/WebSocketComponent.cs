using Combo;
using UnityEngine;
using UnityWebSocket;

public class WebSocketComponent : MonoBehaviour
{
    private IWebSocket webSocket;
    private float lastPongReceiveTime = 0f;
    void Start()
    {
        string address = $"wss://combo-demo.dev.seayoo.com:443/{ComboSDK.GetGameId()}/ws/{ComboSDK.GetLoginInfo().comboId}";
        webSocket = new WebSocket(address);
        webSocket.OnOpen += OnOpen;
        webSocket.OnMessage += OnMessage;
        webSocket.OnClose += OnClose;
        webSocket.OnError += OnError;
        webSocket.ConnectAsync();
        InvokeRepeating("SendPingFrame", 0f, 2f);
    }

    void Update()
    {
        CheckConnectionStatus();
    }

    void OnDestroy()
    {
        webSocket.OnOpen -= OnOpen;
        webSocket.OnMessage -= OnMessage;
        webSocket.OnClose -= OnClose;
        webSocket.OnError -= OnError;
        webSocket.CloseAsync();
    }

    private void OnOpen(object sender, OpenEventArgs e)
    {
        Log.I("WebSocket connection opened.");
    }

    private void OnMessage(object sender, MessageEventArgs e)
    {
        if (e.Data == "PONG")
        {
            lastPongReceiveTime = Time.time;
        }
        else
        {
            Message.Show(e.Data);
        }
    }

    private void OnClose(object sender, CloseEventArgs e)
    {
        Log.I("WebSocket connection closed.");
    }

    private void OnError(object sender, ErrorEventArgs e)
    {

    }

    private void CheckConnectionStatus()
    {
        if (webSocket.ReadyState == WebSocketState.Open)
        {
            float timeSinceLastPong = Time.time - lastPongReceiveTime;
            if (timeSinceLastPong > 10f)
            {
                Log.I("WebSocket connection lost, trying to reconnect...");
                webSocket.CloseAsync();
                webSocket.ConnectAsync();
            }
        }
    }

    private void SendPingFrame()
    {
        webSocket.SendAsync("PING");
    }
}