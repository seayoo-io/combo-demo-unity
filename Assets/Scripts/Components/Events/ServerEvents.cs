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

// 用于更新角色创建时间，通知给 Text 使用
public class UpdateRoleCreateTimeEvent : Event<UpdateRoleCreateTimeEvent>
{
    public DateTime changeTime;
}

// 用于改变角色创建时间，通知的是最终更改的角色创建时间
public class ChangeRoleCreateTimeEvent : Event<ChangeRoleCreateTimeEvent>
{
    public long unixTime;
}