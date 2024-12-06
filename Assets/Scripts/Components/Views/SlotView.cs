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
    public GameObject rolePanel;
    public GameObject addPanel;
    public SlotType slotType;
    public List<Sprite> images = new List<Sprite>();
    private ServerButtonManager manager;
    private int roleType;

    void Start()
    {
        clickBtn.onClick.AddListener(OnClick);
        for (int i = 0; i < images.Count; i++)
        {
            GameManager.Instance.RoleDic.Add(i, images[i]);
        }
    }

    void OnDestroy()
    {
        clickBtn.onClick.RemoveListener(OnClick);
    }

    private void OnClick()
    {
        if(slotType == SlotType.ROLE)
        {
            
        }
        else
        {
            var view = RoleSelectView.Instantiate();
        }
    }

    public void SetInfo(SlotType type, string roleId = "", string roleName = "", int? roleType = null)
    {
        slotType = type;
        if(type == SlotType.ROLE)
        {
            rolePanel.gameObject.SetActive(true);
            addPanel.gameObject.SetActive(false);
            this.roleId.text = roleId;
            this.roleName.text = roleName;
            this.roleType = (int)roleType;
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