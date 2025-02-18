using System;
using Combo;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityWebSocket;

public class WebSocketComponent : MonoBehaviour
{
    private IWebSocket webSocket;
    private float lastPingTime = 0f;
    private float lastPongTime = 0f;
    private string address ="";
    void Start()
    {
        var demoEndpoint = GameClient.GetClientEndPoint().Replace("https://", "");
        address = $"wss://{demoEndpoint}:443/{ComboSDK.GetGameId()}/ws/{ComboSDK.GetLoginInfo().comboId}";
        webSocket = new WebSocket(address);
        webSocket.OnOpen += OnOpen;
        webSocket.OnMessage += OnMessage;
        webSocket.OnClose += OnClose;
        webSocket.OnError += OnError;
        webSocket.ConnectAsync();
        InvokeRepeating("SendPingFrame", 1f, 2f);
        InvokeRepeating("CheckConnectionStatus", 2f, 2f);
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
        if (e.Data != "PONG")
        {
            MailListManager.Instance.SaveMail(ParseMail(e.Data));
            Message.Show(e.Data);
        }
        else
        {
            lastPongTime = Time.time;
        }
    }

    private void OnClose(object sender, CloseEventArgs e)
    {
        Log.I("WebSocket connection closed.");
    }

    private void OnError(object sender, ErrorEventArgs e)
    {

    }

    private void SendPingFrame()
    {
        webSocket.SendAsync("PING");
        lastPingTime = Time.time;
    }

    private void CheckConnectionStatus()
    {
        float timeSinceLastPing = Time.time - lastPingTime;
        float timeSinceLastPong = Time.time - lastPongTime;
        if (timeSinceLastPing > 3f || timeSinceLastPong > 3f)
        {
            Log.I("WebSocket connection lost, trying to reconnect...");
            webSocket = new WebSocket(address);
            webSocket.OnOpen += OnOpen;
            webSocket.OnMessage += OnMessage;
            webSocket.OnClose += OnClose;
            webSocket.OnError += OnError;
            webSocket.ConnectAsync();
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
}