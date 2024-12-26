using System;
using Combo;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityWebSocket;

public class WebSocketComponent : MonoBehaviour
{
    private IWebSocket webSocket;
    private float lastPongReceiveTime = 0f;
    private string address ="";
    void Start()
    {
        Log.I("WebSocketComponent Start");
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
        Log.I("WebSocketComponent Destroy");
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

    void OnApplicationPause(bool pauseStatus)
    {
        Log.I($"Application pause: {pauseStatus}");
    }

    void OnApplicationFocus(bool hasFocus)
    {
        Log.I($"Application focus: {hasFocus}");
    }


    private void OnMessage(object sender, MessageEventArgs e)
    {
        if (e.Data != "PONG")
        {
            MailListManager.Instance.SaveMail(ParseMail(e.Data));
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

    private void SendPingFrame()
    {
        webSocket.SendAsync("PING");
        lastPongReceiveTime = Time.time;
        Log.I(Time.time + "    send ping");
    }

    private void CheckConnectionStatus()
    {
        float timeSinceLastPong = Time.time - lastPongReceiveTime;
        if (timeSinceLastPong > 3f)
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