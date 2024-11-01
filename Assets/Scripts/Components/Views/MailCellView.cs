using System.Collections;
using UnityEditor.IMGUI.Controls;
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
    public bool isRead;
    private MailType mailType;
    private MailBaseInfo mail;
    void Awake()
    {
        clickBtn.onClick.AddListener(OnClickButton);
    }

    void OnDestroy()
    {
        clickBtn.onClick.RemoveListener(OnClickButton);
    }

    public void SetMailInfo(MailType mailType, RewardMailInfo rewardMailInfo = null, MailInfo mailInfo = null)
    {
        this.mailType = mailType;
        if(mailType == MailType.System)
        {
            mail = mailInfo;
            mailTypeText.text = "系统邮件";
            mailTitleText.text = mailInfo.title;
        }
        else if(mailType == MailType.Friend)
        {
            mail = mailInfo;
            mailTypeText.text = "好友邮件";
            mailTitleText.text = mailInfo.title;
        }
        else
        {
            mail = rewardMailInfo;
            mailTypeText.text = "活动奖励";
            mailTitleText.text = "";
        }
    }

    public void OnClickButton()
    {
        if(mailType == MailType.Reward)
        {
            SendRewardEvent.Invoke(new SendRewardEvent{
                rewardMailInfo = (RewardMailInfo)mail,
                gameObject = this.gameObject
            });
        }
        else
        {
            SendMailEvent.Invoke(new SendMailEvent{
                mailInfo = (MailInfo)mail,
                gameObject = this.gameObject
            });
        }
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
