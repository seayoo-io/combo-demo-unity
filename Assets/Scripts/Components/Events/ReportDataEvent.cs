using System.Collections.Generic;

public class PromoPseudoPurchaseEvent : Event<PromoPseudoPurchaseEvent> {
    public string amount;
}
public class LoginEvent : Event<LoginEvent> {
}
public class ActiveValueEvent : Event<ActiveValueEvent>
{
    public string value;
}

public class RoundEndEvent : Event<RoundEndEvent>
{
    public string roomHostId;
    public string matchType;
    public List<string> queueRoleIdList;
}

public class BattleEndEvent : Event<BattleEndEvent>
{
    public string stageId;
    public string stageType;
    public string battleType;
}