public class ClickServerEvent : Event<ClickServerEvent> 
{
    public int serverId;
}

public class ClickRoleEvent : Event<ClickRoleEvent> 
{
    public int roleIndex;
}

public class CloseSeleteRoleEvent : Event<CloseSeleteRoleEvent> 
{
    public bool isFinish;
}

public class ClickSlotViewEvent : Event<ClickSlotViewEvent>
{
    public Role role;
}