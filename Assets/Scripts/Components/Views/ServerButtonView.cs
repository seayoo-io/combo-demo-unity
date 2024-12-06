using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[ViewPrefab("Prefabs/ServerButtonView")]
internal class ServerButtonView : View<ServerButtonView>, ISelectableView
{
    public Button button;
    public Text serverName;
    public Text serverType;
    public Image image;
    private int serverId;

    private ServerButtonManager manager;

    private void Awake()
    {
        manager = FindObjectOfType<ServerButtonManager>();
        if (manager != null)
        {
            manager.RegisterButtonView(this);
        }
        
        button.onClick.AddListener(OnButtonClicked);
    }

    public void SetInfo(string text, int serverType, int serverId)
    {
        serverName.text = text;
        this.serverType.text = GetServerType(serverType);
        this.serverId = serverId;
    }

    private string GetServerType(int serverType)
    {
        switch (serverType)
        {
            case 1:
                return "正常";
            case 2:
                return "维护状态";
            case 3:
                return "推荐";
            case 4:
                return "新服";
            case 5:
                return "繁忙";
            case 6:
                return "爆满";
            default:
                return "未知";
        }
    }

    private void OnButtonClicked()
    {
        manager?.OnButtonViewSelected(this);
        ClickServerEvent.Invoke(new ClickServerEvent{ serverId = serverId} );
    }

    public void Select()
    {
        if (image != null)
        {
            image.gameObject.SetActive(true);
        }
    }

    public void Deselect()
    {
        if (image != null)
        {
            image.gameObject.SetActive(false);
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