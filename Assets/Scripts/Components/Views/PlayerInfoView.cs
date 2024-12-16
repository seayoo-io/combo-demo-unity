using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ViewPrefab("Prefabs/PlayerInfoView")]
internal class PlayerInfoView : View<PlayerInfoView>
{
    public Text playerId;
    public Text accountName;
    public Button copyBtn;
    public Button cancelBtn;
    public Button manageAccountBtn;
    public Button changePasswordBtn;
    public Button deleteAccountBtn;
    public Button contactSupportBtn;
    public Image iconImg;
    public Text roleId;
    public Text serverName;
    public Text gender;
    public Text roleName;
    private Action OnCopy;
    private Action OnCancel;
    private Action OnManageAccount;
    private Action OnChangePassword;
    private Action OnDeleteAccount;
    private Action OnContactSupport;

    void Awake()
    {
        copyBtn.onClick.AddListener(OnCopyConfigBtn);
        cancelBtn.onClick.AddListener(OnCancelConfigBtn);
        manageAccountBtn.onClick.AddListener(OnManageAccountConfigBtn);
        changePasswordBtn.onClick.AddListener(OnChangePasswordConfigBtn);
        deleteAccountBtn.onClick.AddListener(OnDeleteAccountConfigBtn);
        contactSupportBtn.onClick.AddListener(OnContactSupportConfigBtn);
        SetIcon();
    }

    void OnDestroy()
    {
        copyBtn.onClick.RemoveListener(OnCopyConfigBtn);
        cancelBtn.onClick.RemoveListener(OnCancelConfigBtn);
        manageAccountBtn.onClick.RemoveListener(OnManageAccountConfigBtn);
        changePasswordBtn.onClick.RemoveListener(OnChangePasswordConfigBtn);
        deleteAccountBtn.onClick.RemoveListener(OnDeleteAccountConfigBtn);
        contactSupportBtn.onClick.RemoveListener(OnContactSupportConfigBtn);
    }
    
    public void SetPlayerId(string id) {
        playerId.text = id;
    }

    public void SetAccountName(string name) {
        accountName.text = name;
    }

    public void SetRole(Role r)
    {
        roleId.text = "角色 ID：" + r.roleId;
        var str = r.gender == 0 ? "男" : "女";
        gender.text = "性别：" + str;
        roleName.text = "角色名：" + r.roleName;
    }

    public void SetServer(string z, string s)
    {
        serverName.text = z + s;
    }

    void OnCopyConfigBtn()
    {
        OnCopy.Invoke();
    }
    void OnCancelConfigBtn()
    {
        OnCancel.Invoke();
    }
    void OnManageAccountConfigBtn()
    {
        OnManageAccount.Invoke();
    }
    void OnChangePasswordConfigBtn()
    {
        OnChangePassword.Invoke();
    }
    void OnDeleteAccountConfigBtn()
    {
        OnDeleteAccount.Invoke();
    }
    void OnContactSupportConfigBtn()
    {
        OnContactSupport.Invoke();
    }
    public void SetCopyCallback(Action OnCopy)
    {
        this.OnCopy = OnCopy;
    }
    public void SetCancelCallback(Action OnCancel)
    {
        this.OnCancel = OnCancel;
    }
    public void SetManageAccountCallback(Action OnManageAccount)
    {
        this.OnManageAccount = OnManageAccount;
    }
    public void SetChangePasswordCallback(Action OnChangePassword)
    {
        this.OnChangePassword = OnChangePassword;
    }
    public void SetDeleteAccountCallback(Action OnDeleteAccount)
    {
        this.OnDeleteAccount = OnDeleteAccount;
    }
    public void SetOnContactSupportCallback(Action OnContactSupport)
    {
        this.OnContactSupport = OnContactSupport;
    }
    protected override IEnumerator OnHide()
    {
        yield return null;
    }

    protected override IEnumerator OnShow()
    {
        yield return null;
    }

    // 设置头像
    private void SetIcon()
    {
        var role = PlayerController.GetRoleInfo(PlayerController.GetPlayer());
        Sprite sprite;
        GameManager.Instance.RoleDic.TryGetValue((int)role.type, out sprite);
        iconImg.sprite = sprite;
    }
}
