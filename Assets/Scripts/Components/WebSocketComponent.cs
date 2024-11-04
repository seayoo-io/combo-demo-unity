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
        if (e.Data == "PONG")
        {
            lastPongReceiveTime = Time.time;
        }
        else
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

    private MailBaseInfo ParseMail(string jsonString)
    {
        JObject jsonObject = JObject.Parse(jsonString);
        Log.I(jsonString);

        if (jsonObject.ContainsKey("items") && jsonObject.ContainsKey("present_ratio"))
        {
            RewardMailInfo rewardMailInfo = jsonObject.ToObject<RewardMailInfo>();
            rewardMailInfo.mailId = Guid.NewGuid();
            ReceivedRewardEvent.Invoke(new ReceivedRewardEvent{
                rewardMailInfo = rewardMailInfo
            });
            return rewardMailInfo;
        }
        else
        {
            MailInfo mailInfo = jsonObject.ToObject<MailInfo>();
            mailInfo.mailId = Guid.NewGuid();
            ReceivedMailEvent.Invoke(new ReceivedMailEvent{
                mailInfo = mailInfo
            });
            return mailInfo;
        }
    }
}