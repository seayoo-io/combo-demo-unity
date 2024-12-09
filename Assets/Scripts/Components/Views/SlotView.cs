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
            ClickSlotViewEvent.Invoke(new ClickSlotViewEvent{ roleId = roleId.text });
        }
        else
        {
            RoleSelectView.Instantiate();
        }
    }

    public void SetInfo(SlotType type, string roleId = "", string roleName = "", int gender = 0, int? roleType = null)
    {
        slotType = type;
        if(type == SlotType.ROLE)
        {
            rolePanel.gameObject.SetActive(true);
            addPanel.gameObject.SetActive(false);
            this.roleId.text = roleId;
            this.roleName.text = roleName;
            this.roleType = (int)roleType;
            this.gender.text = gender == 0 ? "男" : "女";
            Sprite sprite;
            GameManager.Instance.RoleDic.TryGetValue((int)roleType, out sprite);
            image.sprite = sprite;

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