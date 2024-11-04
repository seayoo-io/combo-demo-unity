using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ViewPrefab("Prefabs/MailView")]
internal class MailView : View<MailView>
{
    public Text mailTitle;
    public Text mailContent;
    public Text typeText;
    public Text readBtnText;
    public Text rewardItemNumber;
    public Text mailItemNumber;
    public Button readButton;
    public Button readAllButton;
    public Button closeBtn;
    public Transform parentTransform;
    public RectTransform scrollViewPanel;
    public GameObject mailPanel;
    public GameObject rewardPanel;
    public GameObject contentPanel;
    public GameObject mailItemPanel;
    private MailBaseInfo currentMail;
    private GameObject currentMailObject;
    private List<MailBaseInfo> list;

    void Awake()
    {
        EventSystem.Register(this);
        foreach (Transform child in parentTransform)
        {
            Destroy(child.gameObject);
        }
        list = MailListManager.Instance.LoadMails();
        foreach (var mail in list)
        {
            if(mail.GetType() == typeof(RewardMailInfo))
            {
                var mailInfo = mail as RewardMailInfo;
                AppendRewardView(MailType.Reward, mailInfo);
            }
            else
            {
                var mailInfo = mail as MailInfo;
                AppendMailView(MailType.System, mailInfo);
            }
        }
        if(list.Count == 0)
        {
            contentPanel.SetActive(false);
            readAllButton.gameObject.SetActive(false);
            return;
        }
        contentPanel.SetActive(true);
        readAllButton.gameObject.SetActive(true);
        SetFristMail(list);
    }

    void Start()
    {
        closeBtn.onClick.AddListener(Destroy);
        readButton.onClick.AddListener(ReadMail);
        readAllButton.onClick.AddListener(ReadAllMail);
    }

    void OnDestroy()
    {
        EventSystem.UnRegister(this);
        closeBtn.onClick.RemoveListener(Destroy);
        readButton.onClick.RemoveListener(ReadMail);
        readAllButton.onClick.RemoveListener(ReadAllMail);
    }

    public void ReadMail()
    {
        if(currentMail.GetType() == typeof(RewardMailInfo))
        {
            var mailInfo = currentMail as RewardMailInfo;
            RequestUpdateCoinEvent.Invoke(new RequestUpdateCoinEvent{
                coinOffset = mailInfo.presentRatio
            });
            Toast.Show($"获得 {mailInfo.presentRatio} 金币");
            MailListManager.Instance.DeleteMail(mailInfo.mailId);
            Destroy(currentMailObject);
        }
        else
        {
            var mailInfo = currentMail as MailInfo;
            var number = GetItemNumber(mailInfo);
            RequestUpdateCoinEvent.Invoke(new RequestUpdateCoinEvent{
                coinOffset = number
            });
            Toast.Show($"获得 {number} 金币");
            MailListManager.Instance.DeleteMail(mailInfo.mailId);
            Destroy(currentMailObject);
        }
        StartCoroutine(UpdateMailListNextFrame());
    }
    private IEnumerator UpdateMailListNextFrame()
    {
        yield return null;  // 等待下一帧
        list = MailListManager.Instance.LoadMails();
        if (list.Count == 0)
        {
            contentPanel.SetActive(false);
            readAllButton.gameObject.SetActive(false);
        }
        else
        {
            SetFristMail(list);
        }
    }

    public void ReadAllMail()
    {
        list = MailListManager.Instance.LoadMails();
        int number = 0;
        foreach (var mail in list)
        {
            if(mail.GetType() == typeof(RewardMailInfo))
            {
                var mailInfo = mail as RewardMailInfo;
                number += mailInfo.presentRatio;
            }
            else
            {
                var mailInfo = mail as MailInfo;
                number += GetItemNumber(mailInfo);
            }
        }
        if(number > 0)
        {
            RequestUpdateCoinEvent.Invoke(new RequestUpdateCoinEvent{
                coinOffset = number
            });
            Toast.Show($"总共获得 {number} 金币");
        }
        
        MailListManager.Instance.DeleteAllMail();
        foreach (Transform child in parentTransform)
        {
            Destroy(child.gameObject);
        }
        contentPanel.SetActive(false);
        readAllButton.gameObject.SetActive(false);
        list = new List<MailBaseInfo>();
    }

    
    [EventSystem.BindEvent]
    void ReceiveMail(SendMailEvent evt)
    {
        currentMail = evt.mailInfo;
        currentMailObject = evt.gameObject;
        SetMailInfo(evt.mailInfo);
    }

    [EventSystem.BindEvent]
    void ReceiveReward(SendRewardEvent evt)
    {
        currentMail = evt.rewardMailInfo;
        currentMailObject = evt.gameObject;
        SetRewardInfo(evt.rewardMailInfo);
    }

    [EventSystem.BindEvent]
    void UpdateMail(ReceivedMailEvent evt)
    {
        contentPanel.SetActive(true);
        readAllButton.gameObject.SetActive(true);
        if(string.IsNullOrEmpty(evt.mailInfo.from))
        {
            AppendMailView(MailType.System, evt.mailInfo);
        }
        else
        {
            AppendMailView(MailType.Friend, evt.mailInfo);
        }
        list = MailListManager.Instance.LoadMails();
        if(list.Count == 0)
        {
            currentMail = evt.mailInfo;
            currentMailObject = parentTransform.GetChild(0).gameObject;
            SetMailInfo(evt.mailInfo);
        }
    }

    [EventSystem.BindEvent]
    void UpdateReward(ReceivedRewardEvent evt)
    {
        contentPanel.SetActive(true);
        readAllButton.gameObject.SetActive(true);
        AppendRewardView(MailType.Reward, evt.rewardMailInfo);
        list = MailListManager.Instance.LoadMails();
        if(list.Count == 0)
        {
            currentMail = evt.rewardMailInfo;
            currentMailObject = parentTransform.GetChild(0).gameObject;
            SetRewardInfo(evt.rewardMailInfo);
        }
    }

    private void AppendMailView(
        MailType mailType,
        MailInfo mailInfo
    )
    {
        var view = MailCellView.Instantiate();
        view.gameObject.transform.localScale = Vector3.one;
        view.SetMailInfo(mailInfo);
        view.gameObject.transform.SetParent(parentTransform, false);
        view.Show();
    }

    private void AppendRewardView(
        MailType mailType,
        RewardMailInfo rewardMailInfo
    )
    {
        var view = MailCellView.Instantiate();
        view.gameObject.transform.localScale = Vector3.one;
        view.SetRewardInfo(rewardMailInfo);
        view.gameObject.transform.SetParent(parentTransform, false);
        view.Show();
    }


    private void SetMailInfo(MailInfo mailInfo)
    {
        mailPanel.gameObject.SetActive(true);
        rewardPanel.gameObject.SetActive(false);
        mailTitle.text = mailInfo.title;
        mailContent.text = mailInfo.content;
        if(mailInfo.attachments != null && mailInfo.attachments.Count > 0)
        {
            readBtnText.text = "领取";
            if(GetItemNumber(mailInfo) != 0)
            {
                mailItemNumber.text = GetItemNumber(mailInfo).ToString();
                mailItemPanel.SetActive(true);
                scrollViewPanel.offsetMin = new Vector2(0, -145);
            }
        }
        else
        {
            readBtnText.text = "删除";
            mailItemPanel.SetActive(false);
            scrollViewPanel.offsetMin = new Vector2(0, -195);
        }
        if(string.IsNullOrEmpty(mailInfo.from))
        {
            typeText.text = "系统";
        }
        else
        {
            mailTitle.text = mailInfo.title + $": 来自好友 {mailInfo.from}";
            typeText.text = "好友";
        }
    }

    private void SetRewardInfo(RewardMailInfo rewardMailInfo)
    {
        rewardPanel.gameObject.SetActive(true);
        mailPanel.gameObject.SetActive(false);
        mailItemPanel.SetActive(false);
        mailTitle.text = "活动奖励";
        rewardItemNumber.text = rewardMailInfo.presentRatio.ToString();
        readBtnText.text = "领取";
        typeText.text = "奖励";
    }

    private void SetFristMail(List<MailBaseInfo> list)
    {
        var firstMail = list[0];
        currentMail = firstMail;
        currentMailObject = parentTransform.GetChild(0).gameObject;
        if(firstMail.GetType() == typeof(RewardMailInfo))
        {
            var mailInfo = firstMail as RewardMailInfo;
            SetRewardInfo(mailInfo);
        }
        else
        {
            var mailInfo = firstMail as MailInfo;
            SetMailInfo(mailInfo);
        }
    }

    private int GetItemNumber(MailInfo mailInfo)
    {
        var itemCount = 0;
        if(mailInfo.attachments != null && mailInfo.attachments.Count > 0)
        {
            foreach (var attachment in mailInfo.attachments)
            {
                itemCount += attachment.itemCount;
            }
        }
        return itemCount;
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
