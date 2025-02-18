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
    public Text mailItemNumber;
    public Button readButton;
    public Button readAllButton;
    public Button closeBtn;
    public Transform parentTransform;
    public RectTransform scrollViewPanel;
    public GameObject mailPanel;
    public GameObject contentPanel;
    public GameObject mailItemPanel;
    private MailInfo currentMail;
    private GameObject currentMailObject;
    private List<MailInfo> list;

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
            var mailInfo = mail as MailInfo;
            AppendMailView(MailType.System, mailInfo);
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
        var mailInfo = currentMail;
        var number = GetItemNumber(mailInfo);
        if(number > 0)
        {
            RequestUpdateCoinEvent.Invoke(new RequestUpdateCoinEvent{
                coinOffset = number
            });
            Toast.Show($"获得 {number} 金币");
        }
        MailListManager.Instance.DeleteMail(mailInfo.referenceId);
        Destroy(currentMailObject);
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
            number += GetItemNumber(mail);
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
        list = new List<MailInfo>();
    }

    
    [EventSystem.BindEvent]
    void ReceiveMail(SendMailEvent evt)
    {
        currentMail = evt.mailInfo;
        currentMailObject = evt.gameObject;
        SetMailInfo(evt.mailInfo);
    }

    [EventSystem.BindEvent]
    void UpdateMail(ReceivedMailEvent evt)
    {
        contentPanel.SetActive(true);
        readAllButton.gameObject.SetActive(true);
        if(string.IsNullOrEmpty(evt.mailInfo.sender))
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

    private void SetMailInfo(MailInfo mailInfo)
    {
        mailPanel.gameObject.SetActive(true);
        mailTitle.text = mailInfo.title;
        mailContent.text = mailInfo.content;
        if(mailInfo.attachments != null && mailInfo.attachments.Count > 0 && GetItemNumber(mailInfo) > 0)
        {
            readBtnText.text = "领取";
            mailItemNumber.text = GetItemNumber(mailInfo).ToString();
            mailItemPanel.SetActive(true);
            scrollViewPanel.offsetMin = new Vector2(0, -125);
        }
        else
        {
            readBtnText.text = "删除";
            mailItemPanel.SetActive(false);
            scrollViewPanel.offsetMin = new Vector2(0, -165);
        }
        if(string.IsNullOrEmpty(mailInfo.sender))
        {
            typeText.text = "系统";
        }
        else
        {
            mailTitle.text = mailInfo.title + $": 来自好友 {mailInfo.sender}";
            typeText.text = "好友";
        }
    }

    private void SetFristMail(List<MailInfo> list)
    {
        var firstMail = list[0];
        currentMail = firstMail;
        currentMailObject = parentTransform.GetChild(0).gameObject;
        SetMailInfo(firstMail);
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
