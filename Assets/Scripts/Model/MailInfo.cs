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
    public List<Attachments> attachments;
    public string content;
    [Newtonsoft.Json.JsonProperty("reference_id")]
    public string referenceId;
    [Newtonsoft.Json.JsonProperty("role_id")]
    public string roleId;
    public string sender;
    [Newtonsoft.Json.JsonProperty("server_id")]
    public string serverId;
    public string title;
}

public class Attachments
{
    [Newtonsoft.Json.JsonProperty("item_count")]
    public int itemCount;
    [Newtonsoft.Json.JsonProperty("item_id")]
    public string itemId;
}
