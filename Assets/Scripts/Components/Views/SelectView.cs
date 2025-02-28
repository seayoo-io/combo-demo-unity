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
    public Text btnText;
    public ServerButtonManager manager;
    public List<Sprite> images = new List<Sprite>();
    private Role currentRole;
    private List<SlotView> slotViews = new List<SlotView>();
    private Action CloseAction;

    void Start()
    {
        EventSystem.Register(this);
        deleleButton.interactable = false;
        enterGameBtn.interactable = false;
        GameManager.Instance.RoleDic.Clear();
        for (int i = 0; i < images.Count; i++)
        {
            GameManager.Instance.RoleDic.Add(i, images[i]);
        }
        ShowRoleList();
        closeBtn.onClick.AddListener(OnReturnBtnAction);
        deleleButton.onClick.AddListener(DeleteRole);
        enterGameBtn.onClick.AddListener(EnterGame);
    }

    void OnDestroy()
    {
        EventSystem.UnRegister(this);
        closeBtn.onClick.RemoveListener(OnReturnBtnAction);
        deleleButton.onClick.RemoveListener(DeleteRole);
        enterGameBtn.onClick.RemoveListener(EnterGame);
    }

    public void SetViewInfo(Action returnBtnAction)
    {
        CloseAction = returnBtnAction;
    }

    public void OnReturnBtnAction()
    {
        CloseAction?.Invoke();
        Destroy();
    }

    public void DeleteRole()
    {
        UIController.Alert(UIAlertType.Singleton, "提示", $"是否删除该账号 {currentRole.roleId}", "取消", "确认删除",
        () => {},
        () =>{
            deleleButton.interactable = false;
            enterGameBtn.interactable = false;
            GameClient.DeleteRole(currentRole.roleId, data => {
                ShowRoleList();
            }, (error) => {
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
        });  
    }

    private void EnterGame()
    {
        SceneManager.LoadScene("Game");
        var newPlayer = PlayerController.SpawnPlayer(currentRole);
        DontDestroyOnLoad(newPlayer);
        PlayerController.UpdateRole(PlayerController.GetPlayer(), currentRole);
        MailListManager.Instance.RefreshPath();
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

    [EventSystem.BindEvent]
    public void Close(CloseSeleteView e)
    {
        Destroy();
    }
    
    private void ShowRoleList()
    {   
        deleleButton.interactable = false;
        enterGameBtn.interactable = false;
        foreach (Transform child in parentTransform)
        {
            Destroy(child.gameObject);
        }
        slotViews.Clear();
        GameClient.GetRolesList(GameManager.Instance.ZoneId, GameManager.Instance.ServerId, datas => {
            if(datas == null)
            {
                LoadSlotView(number: 3);
                return;
            }
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
            deleleButton.interactable = true;
            enterGameBtn.interactable = true;
        }, (error) => {
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