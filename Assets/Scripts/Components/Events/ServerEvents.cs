using System;

public class ClickServerEvent : Event<ClickServerEvent> 
{
    public int serverId;
    public string serverName;
}

public class ClickRoleEvent : Event<ClickRoleEvent> 
{
    public int roleIndex;
}

public class CloseSeleteRoleEvent : Event<CloseSeleteRoleEvent> 
{
    public bool isFinish;
}

public class CloseSeleteView : Event<CloseSeleteView> {}

public class ClickSlotViewEvent : Event<ClickSlotViewEvent>
{
    public Role role;
}

public class ChangeRoleEvent : Event<ChangeRoleEvent>
{
    public Role role;
}

// 用于更新时间，通知给 Text 使用
public class UpdateTimeEvent : Event<UpdateTimeEvent>
{
    public DateTime changeTime;
}

// 用于改变时间，通知的是最终时间
public class ChangeTimeEvent : Event<ChangeTimeEvent>
{
    public long unixTime;
}