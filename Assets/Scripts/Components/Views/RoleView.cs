using System.Collections;
using System.ComponentModel.Design.Serialization;
using UnityEngine;
using UnityEngine.UI;

[ViewPrefab("Prefabs/RoleView")]
internal class RoleView : View<RoleView>
{
    public Button clickBtn;
    public Image image;
    private int index;
    void Start()
    {
       clickBtn.onClick.AddListener(OnClick);
    }

    void OnDestroy()
    {
        clickBtn.onClick.RemoveListener(OnClick);
    }

    public void SetInfo(int index, Sprite sprite)
    {
        this.index = index;
        image.sprite = sprite;
    }

    public void OnClick()
    {
        ClickRoleEvent.Invoke(new ClickRoleEvent{ roleIndex = index });
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