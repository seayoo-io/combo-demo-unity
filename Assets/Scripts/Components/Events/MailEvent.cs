using UnityEngine;

public class ReceivedMailEvent : Event<ReceivedMailEvent>
{
    public MailInfo mailInfo;
}

public class SendMailEvent : Event<SendMailEvent>
{
    public MailInfo mailInfo;
    public GameObject gameObject;
}