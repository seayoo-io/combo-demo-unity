public class ComplainEvent : Event<ComplainEvent>
{
    public string targetType;
    public string targetId;
    public string targetName;
    public string serverId;
    public string roleId;
    public string roleName;
    public int width;
    public int height;
}