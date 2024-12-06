using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Select : MonoBehaviour
{
    public Transform parentTransform;
    public Role[] roleInfos;
    public Button deleleButton;
    public Button enterGameBtn;
    private string currentRoleId;

    void Start()
    {
        GameClient.GetRolesList(GameManager.Instance.ZoneId, GameManager.Instance.ServerId, datas => {
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

    public void DeleteRole()
    {
        GameClient.DeleteRole(currentRoleId);
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
                slotView.SetInfo(SlotType.ROLE, data.roleId, data.roleName, data.type);
            }
            slotView.gameObject.transform.SetParent(parentTransform, false);
        }
    }
}