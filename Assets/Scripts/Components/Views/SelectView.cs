using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[ViewPrefab("Prefabs/SelectView")]
internal class SelectView : View<SelectView>
{
    public Transform parentTransform;
    public Dictionary<string, Role> roleInfos = new Dictionary<string, Role>();
    public Button deleleButton;
    public Button enterGameBtn;
    public Button closeBtn;
    public ServerButtonManager manager;
    public List<Sprite> images = new List<Sprite>();
    private Role currentRole;
    private List<SlotView> slotViews = new List<SlotView>();

    void Start()
    {
        EventSystem.Register(this);
        GameManager.Instance.RoleDic.Clear();
        for (int i = 0; i < images.Count; i++)
        {
            GameManager.Instance.RoleDic.Add(i, images[i]);
        }
        ShowRoleList();
        closeBtn.onClick.AddListener(Destroy);
        deleleButton.onClick.AddListener(DeleteRole);
        enterGameBtn.onClick.AddListener(EnterGame);
    }

    void OnDestroy()
    {
        EventSystem.UnRegister(this);
        closeBtn.onClick.RemoveListener(Destroy);
        deleleButton.onClick.RemoveListener(DeleteRole);
        enterGameBtn.onClick.RemoveListener(EnterGame);
    }

    public void DeleteRole()
    {
        GameClient.DeleteRole(currentRole.roleId, data => {
            ShowRoleList();
        });
        
    }

    public void EnterGame()
    {  
        SceneManager.LoadScene("Game");
        var newPlayer = PlayerController.SpawnPlayer(currentRole);
        DontDestroyOnLoad(newPlayer);
        PlayerController.UpdateRole(PlayerController.GetPlayer(), currentRole);
    }

    [EventSystem.BindEvent]
    public void Refresh(CloseSeleteRoleEvent e)
    {
        if(!e.isFinish)
        {
            return;
        }
        ShowRoleList();
    }

    [EventSystem.BindEvent]
    public void SelectRole(ClickSlotViewEvent e)
    {
        if (e.role != null)
        {
            currentRole = e.role;
        };
    }

    private void ShowRoleList()
    {   
        foreach (Transform child in parentTransform)
        {
            Destroy(child.gameObject);
        }
        GameClient.GetRolesList(GameManager.Instance.ZoneId, GameManager.Instance.ServerId, datas => {
            if(datas == null)
            {
                deleleButton.interactable = false;
                enterGameBtn.interactable = false;
                LoadSlotView(number: 3);
                return;
            }
            deleleButton.interactable = true;
            enterGameBtn.interactable = true;
            foreach(var data in datas)
            {
                roleInfos[data.roleId] = data;
                LoadSlotView(data);
            }
            if(datas.Count() < 3)
            {
                LoadSlotView(number: 3 - datas.Count());
            }
            else
            {
                LoadSlotView();
            }
        });
    }

    private void LoadSlotView(Role data = null, int number = 1)
    {
        for (int i = 0; i < number; i++)
        {
            var slotView = SlotView.Instantiate();
            if(data == null)
            {
                slotView.SetInfo(SlotType.ADD);
            }
            else
            {
                slotView.SetInfo(SlotType.ROLE, r: data);
                slotViews.Add(slotView);
            }
            slotView.gameObject.transform.SetParent(parentTransform, false);
        }
        if(slotViews.Count() <= 0)
        {
            return;
        }
        manager.OnButtonViewSelected(slotViews[0]);
        currentRole = roleInfos[slotViews[0].roleId.text];
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