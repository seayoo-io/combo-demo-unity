using System.Collections;
using System.Collections.Generic;
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
        closeBtn.onClick.AddListener(Destroy);
        enterCreateRoleBtn.onClick.AddListener(() => { SceneManager.LoadScene("Select"); });
        GameClient.GetServerList((GameData[] datas) =>
        {
            StartCoroutine(InitializeButtonsAsync(datas));
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
        closeBtn.onClick.RemoveListener(Destroy);
        enterCreateRoleBtn.onClick.RemoveListener(() => { SceneManager.LoadScene("Select"); });
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
            var buttonView = ServerButtonView.Instantiate();
            buttonView.SetInfo(data.serverName, data.status);
            buttonView.gameObject.transform.SetParent(serverParentTransform, false);
            serverButtonViews.Add(buttonView);
        }
        manager.OnButtonViewSelected(serverButtonViews[0]);
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