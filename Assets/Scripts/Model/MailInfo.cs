using System;
using System.Collections.Generic;

public enum MailType
{
    System,
    Friend
}

[System.Serializable]
public class MailList
{
    public List<MailInfo> mails = new List<MailInfo>();
}

[System.Serializable]
public class MailInfo
{
    [Newtonsoft.Json.JsonProperty("combo_id")]
    public string comboId;
    public List<Attachments> attachments;
    public string content;
    public string description;
    public string from;
    [Newtonsoft.Json.JsonProperty("reference_id")]
    public string referenceId;
    [Newtonsoft.Json.JsonProperty("server_id")]
    public string serverId;
    public string title;
    public string to;
}

public class Attachments
{
    [Newtonsoft.Json.JsonProperty("item_count")]
    public int itemCount;
    [Newtonsoft.Json.JsonProperty("item_id")]
    public string itemId;
}
