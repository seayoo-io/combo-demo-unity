using UnityEngine;

public class ReceivedMailEvent : Event<ReceivedMailEvent>
{
    public MailInfo mailInfo;
}

public class ReceivedRewardEvent: Event<ReceivedRewardEvent>
{
    public RewardMailInfo rewardMailInfo;
}

public class SendMailEvent : Event<SendMailEvent>
{
    public MailInfo mailInfo;
    public GameObject gameObject;
}

public class SendRewardEvent : Event<SendRewardEvent>
{
    public RewardMailInfo rewardMailInfo;
    public GameObject gameObject;
}