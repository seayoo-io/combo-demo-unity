using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum SlotType
{
    ROLE,
    ADD
}

public interface ISelectableView
{
    void Select();
    void Deselect();
}

[ViewPrefab("Prefabs/SlotView")]
internal class SlotView : View<SlotView>, ISelectableView
{
    public Button clickBtn;
    public Text roleId;
    public Text gender;
    public Text roleName;
    public Image image;
    public Image isSelectImage;
    public GameObject rolePanel;
    public GameObject addPanel;
    public SlotType slotType;
    private ServerButtonManager manager;
    private int roleType;
    private Role role;

    void Start()
    {
        clickBtn.onClick.AddListener(OnClick);
    }

    void OnDestroy()
    {
        clickBtn.onClick.RemoveListener(OnClick);
    }

    private void OnClick()
    {
        if(slotType == SlotType.ROLE)
        {
            manager?.OnButtonViewSelected(this);
            ClickSlotViewEvent.Invoke(new ClickSlotViewEvent{ role = role });
        }
        else
        {
            RoleSelectView.Instantiate();
        }
    }

    public void SetInfo(SlotType type, Role r = null)
    {
        slotType = type;
        if(type == SlotType.ROLE)
        {
            role = r;
            rolePanel.gameObject.SetActive(true);
            addPanel.gameObject.SetActive(false);
            roleId.text = r.roleId;
            roleName.text = r.roleName;
            roleType = r.type;
            gender.text = r.gender == 0 ? "男" : "女";
            Sprite sprite;
            GameManager.Instance.RoleDic.TryGetValue((int)r.type, out sprite);
            image.sprite = sprite;
            role.serverId = GameManager.Instance.ServerId;
            role.serverName = GameManager.Instance.ServerName;

            manager = FindObjectOfType<ServerButtonManager>();
            if (manager != null)
            {
                manager.RegisterButtonView(this);
            }
        }
        else
        {
            rolePanel.gameObject.SetActive(false);
            addPanel.gameObject.SetActive(true);
        }
        
    }

    public void Select()
    {
        if (isSelectImage != null)
        {
            isSelectImage.gameObject.SetActive(true);
        }
    }

    public void Deselect()
    {
        if (isSelectImage != null)
        {
            isSelectImage.gameObject.SetActive(false);
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