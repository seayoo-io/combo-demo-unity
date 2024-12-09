using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Select : MonoBehaviour
{
    public Transform parentTransform;
    public Role[] roleInfos;
    public Button deleleButton;
    public Button enterGameBtn;
    public Button closeBtn;
    public ServerButtonManager manager;
    public List<Sprite> images = new List<Sprite>();
    private string currentRoleId;
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
        closeBtn.onClick.AddListener(() => {SceneManager.LoadScene("Login");});
        deleleButton.onClick.AddListener(DeleteRole);
    }

    void OnDestroy()
    {
        EventSystem.UnRegister(this);
        closeBtn.onClick.RemoveListener(() => {SceneManager.LoadScene("Login");});
        deleleButton.onClick.RemoveListener(DeleteRole);
    }

    public void DeleteRole()
    {
        GameClient.DeleteRole(currentRoleId);
    }

    public void EnterGame()
    {
        SceneManager.LoadScene("Game");
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
        currentRoleId = e.roleId;
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
                LoadSlotView(number: 3);
                return;
            }
            foreach(var data in datas)
            {
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

    private void LoadSlotView(GetRolesListResponse data = null, int number = 1)
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
                slotView.SetInfo(SlotType.ROLE, data.roleId, data.roleName, data.gender, data.type);
                slotViews.Add(slotView);
            }
            slotView.gameObject.transform.SetParent(parentTransform, false);
        }
        if(slotViews.Count() <= 0)
        {
            return;
        }
        manager.OnButtonViewSelected(slotViews[0]);
        currentRoleId = slotViews[0].roleId.text;
    }
}