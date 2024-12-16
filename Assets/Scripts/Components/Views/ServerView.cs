using System.Collections;
using System.Collections.Generic;
using Combo;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[ViewPrefab("Prefabs/ServerView")]
internal class ServerView : View<ServerView>
{
    public Transform zoneParentTransform;
    public Transform serverParentTransform;
    public TabGroup tabGroup;
    public GameObject buttonPrefab;
    public Button closeBtn;
    public Button enterCreateRoleBtn;
    public ServerButtonManager manager;
    private List<ServerButtonView> serverButtonViews = new List<ServerButtonView>();
    
    void Start()
    {
        EventSystem.Register(this);
        closeBtn.onClick.AddListener(Destroy);
        enterCreateRoleBtn.onClick.AddListener(EnterRoleView);
        GameClient.GetServerList((GameData[] datas) =>
        {
            StartCoroutine(InitializeButtonsAsync(datas));
        }, (error) => {
            enterCreateRoleBtn.interactable = false;
            if(error != "invalid headers") return;
            UIController.Alert(UIAlertType.Singleton, "提示", "登陆状态失效，请重新登陆", "切换账号", () => {
                Login login = FindObjectOfType<Login>();
                if(login != null)
                {
                    login.OnSwitchAccount();
                    Destroy();
                }
            });
        });
    }
    
    IEnumerator InitializeButtonsAsync(GameData[] datas)
    {
        foreach (var data in datas)
        {
            var buttonView = TabButtonView.Instantiate();
            buttonView.SetInfo(data.zone.zoneName);
            buttonView.gameObject.transform.SetParent(zoneParentTransform, false);
            tabGroup.actions.Add(() => { OnClickZone(data); });
            tabGroup.tabButtons.Add(buttonView.button);
            buttonView.button.tabGroup = tabGroup;

            yield return null;
        }

        if (tabGroup.tabButtons.Count > 0)
        {
            tabGroup.OnTabSelected(tabGroup.tabButtons[0]);
        }
    }

    void OnDestroy()
    {
        EventSystem.UnRegister(this);
        closeBtn.onClick.RemoveListener(Destroy);
        enterCreateRoleBtn.onClick.RemoveListener(EnterRoleView);
    }

    public void EnterRoleView()
    {
        SelectView.Instantiate();
        Destroy();
    }

    public void OnClickZone(GameData gameData)
    {
        foreach (Transform child in serverParentTransform)
        {
            Destroy(child.gameObject);
        }
        serverButtonViews.Clear();
        foreach(var data in gameData.servers)
        {
            if(data.visibility == 2)
            {
                return;
            }
            var buttonView = ServerButtonView.Instantiate();
            buttonView.SetInfo(data.serverName, data.status, data.serverId);
            buttonView.gameObject.transform.SetParent(serverParentTransform, false);
            serverButtonViews.Add(buttonView);
        }
        manager.OnButtonViewSelected(serverButtonViews[0]);
        GameManager.Instance.ZoneId = gameData.zone.zoneId;
        GameManager.Instance.ServerId = gameData.servers[0].serverId;
        GameManager.Instance.ServerName = gameData.servers[0].serverName;
        GameManager.Instance.ZoneName = gameData.zone.zoneName;
    }

    [EventSystem.BindEvent]
    public void ClickServer(ClickServerEvent e)
    {
        GameManager.Instance.ServerId = e.serverId;
        GameManager.Instance.ServerName = e.serverName;
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