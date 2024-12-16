using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[ViewPrefab("Prefabs/MailCellView")]
internal class MailCellView : View<MailCellView>
{
    public Text mailTypeText;
    public Text mailTitleText;
    public GameObject unrealImage;
    public GameObject realImage;
    public Button clickBtn;
    private MailType mailType;
    private MailInfo mail;
    void Awake()
    {
        clickBtn.onClick.AddListener(OnClickButton);
    }

    void OnDestroy()
    {
        clickBtn.onClick.RemoveListener(OnClickButton);
    }

    public void SetMailInfo(MailInfo mailInfo)
    {
        if(string.IsNullOrEmpty(mailInfo.sender))
        {
            mailType = MailType.System;
            mail = mailInfo;
            mailTypeText.text = "系统邮件";
            mailTitleText.text = mailInfo.title;
        }
        else
        {
            mailType = MailType.Friend;
            mail = mailInfo;
            mailTypeText.text = "好友邮件";
            mailTitleText.text = mailInfo.title;
        }
    }

    public void OnClickButton()
    {
        SendMailEvent.Invoke(new SendMailEvent{
            mailInfo = mail,
            gameObject = gameObject
        });
    }

    protected override IEnumerator OnHide()
    {
        yield return null;
    }

    protected override IEnumerator OnShow()
    {
        yield return null;
    }
}
