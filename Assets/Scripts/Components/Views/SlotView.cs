using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public enum SlotType
{
    ROLE,
    ADD
}

[ViewPrefab("Prefabs/SlotView")]
internal class SlotView : View<SlotView>
{
    public Button clickBtn;
    public Text roleId;
    public Text gender;
    public Text roleName;
    public Image sprite;
    public GameObject rolePanel;
    public GameObject addPanel;
    public 

    void Start()
    {

    }

    void OnDestroy()
    {

    }

    public void SetInfo(SlotType type, Role role = null)
    {
        if(type == SlotType.ROLE)
        {
            rolePanel.gameObject.SetActive(true);
            addPanel.gameObject.SetActive(false);
            roleId.text = role.roleId;
            roleName.text = role.roleName;
        }
        else
        {
            rolePanel.gameObject.SetActive(false);
            addPanel.gameObject.SetActive(true);
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