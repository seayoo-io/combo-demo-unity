using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using Sentry;
using UnityEngine;
using UnityEngine.UI;

[ViewPrefab("Prefabs/RoleSelectView")]
internal class RoleSelectView : View<RoleSelectView>
{
    public Transform parentTransform;
    public InputField nameInputField;
    public Toggle[] toggles;
    public Button confirmBtn;
    public Button returnBtn;
    public Image roleImage;
    private int index = 0;
    private int gender = 0;
    void Start()
    {
        EventSystem.Register(this);
        for (int i = 0; i < toggles.Length; i++)
        {
            Toggle toggle = toggles[i];//循环遍历添加
            toggle.onValueChanged.AddListener((bool value) => OnValueChange(toggle));
        }
        var dic = GameManager.Instance.RoleDic;
        foreach (KeyValuePair<int, Sprite> entry in dic)
        {
            Sprite sprite = entry.Value;
            if (sprite != null)
            {
                var view = RoleView.Instantiate();
                view.SetInfo(entry.Key, sprite);
                view.gameObject.transform.SetParent(parentTransform, false);
            }
        }
        confirmBtn.onClick.AddListener(OnConfirm);
        returnBtn.onClick.AddListener(() => { CloseSeleteRoleEvent.Invoke(new CloseSeleteRoleEvent{ isFinish = false }); Destroy();});
    }

    void OnDestroy()
    {
        EventSystem.UnRegister(this);
        confirmBtn.onClick.RemoveListener(OnConfirm);
        returnBtn.onClick.RemoveListener(Destroy);
    }

    public void OnConfirm()
    {
        if(string.IsNullOrEmpty(nameInputField.text))
        {
            Toast.Show("请输入角色名称");
            return;
        }
        GameClient.CreateRole(nameInputField.text, gender, index, GameManager.Instance.ZoneId, GameManager.Instance.ServerId, (roleId) => {
            Destroy();
            CloseSeleteRoleEvent.Invoke(new CloseSeleteRoleEvent{ isFinish = true });
        }); 
    }

    [EventSystem.BindEvent]
    public void ClickRole(ClickRoleEvent e)
    {
        var dic = GameManager.Instance.RoleDic;
        index = e.roleIndex;
        dic.TryGetValue(index, out Sprite sprite);
        roleImage.sprite = sprite;
    }

    private void OnValueChange(Toggle toggle)
    {
        if (toggle.isOn)
        {
            switch (toggle.name)
            {
                case "manToggle":
                    gender = 0;
                    break;
                case "womanToggle":
                    gender = 1;
                    break;
            }
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