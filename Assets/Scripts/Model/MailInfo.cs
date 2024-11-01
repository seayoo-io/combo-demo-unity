using System;
using System.Collections.Generic;

public enum MailType
{
    System,
    Friend,
    Reward
}

[System.Serializable]
public class MailList
{
    public List<MailBaseInfo> mails = new List<MailBaseInfo>();
}

[System.Serializable]
public abstract class MailBaseInfo
{
    public abstract string Type { get; }
    [Newtonsoft.Json.JsonProperty("combo_id")]
    public string comboId;
    public Guid mailId;
    public bool isRead = false;
}


[System.Serializable]
public class MailInfo : MailBaseInfo
{
    public override string Type => "MailInfo";
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

[System.Serializable]
public class RewardMailInfo : MailBaseInfo
{
    public override string Type => "RewardMailInfo";
    public bool immediately;
    public List<RewardItem> items;
    [Newtonsoft.Json.JsonProperty("present_ratio")]
    public int presentRatio;
    [Newtonsoft.Json.JsonProperty("role_id")]
    public string roleId;
    [Newtonsoft.Json.JsonProperty("server_id")]
    public int serverId;
}

[System.Serializable]
public class RewardItem
{
    public string id;
    public string name;
    public int type;
}
