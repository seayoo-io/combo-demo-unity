using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Combo;

[ViewPrefab("Prefabs/ComplainView")]
internal class ComplainView : View<ComplainView>
{
    private RankType rankType;
    public string playerId;
    public Text accountName;
    public Image playerImage;
    public Text wanjiaName;
    public Text wanjiaLevel;
    public Text wanjiaId;
    public Text zongmenName;
    public Text zongmenLevel;
    public Text zongmenNumber;
    public Text zongmenPatriarch;
    public Button complainBtn;
    public Button cancelBtn;
    private Action OnComplain;
    public GameObject CharacterPanel;
    public GameObject WanjiaPanel;
    public GameObject ZongmenPanel;

    void Awake()
    {
        complainBtn.onClick.AddListener(OnComplainConfigBtn);
        cancelBtn.onClick.AddListener(Destroy);
    }

    void OnDestroy()
    {
        complainBtn.onClick.RemoveListener(OnComplainConfigBtn);
        cancelBtn.onClick.RemoveListener(Destroy);
    }

    public void ComplainType(RankType rankType)
    {
        Log.I("ComplainType:" + rankType);
        if(rankType == RankType.Character)
        {
            this.rankType = RankType.Character;
            CharacterPanel.gameObject.SetActive(true);
            WanjiaPanel.gameObject.SetActive(false);
            ZongmenPanel.gameObject.SetActive(false);
        }
        else if(rankType == RankType.Wanjia)
        {
            this.rankType = RankType.Wanjia;
            WanjiaPanel.gameObject.SetActive(true);
            CharacterPanel.gameObject.SetActive(false);
            ZongmenPanel.gameObject.SetActive(false);
        }
        else if(rankType == RankType.Zongmen)
        {
            this.rankType = RankType.Zongmen;
            ZongmenPanel.gameObject.SetActive(true);
            WanjiaPanel.gameObject.SetActive(false);
            CharacterPanel.gameObject.SetActive(false);
        }
    }
    
    public void SetPlayerId(string id) {
        playerId = id;
    }

    public void SetAccountName(string name) {
        accountName.text = name;
    }
    public void SetPlayerImage(Sprite sprite) {
        playerImage.sprite = sprite;
    }

    public void SetWanjiaName(string name) {
        wanjiaName.text = name;
    }
    public void SetWanjiaLevel(string level) {
        wanjiaLevel.text = level;
    }

    public void SetWanjiaId(string id) {
        wanjiaId.text = id;
    }

    public void SetZongmenName(string name) {
        zongmenName.text = name;
    }
    public void SetZongmenLevel(string level) {
        zongmenLevel.text = level;
    }

    public void SetZongmenNumber(string number) {
        zongmenNumber.text = number;
    }
    public void SetZongmenPatriarch(string patriarch) {
        zongmenPatriarch.text = patriarch;
    }

    void OnComplainConfigBtn()
    {
        var role = PlayerController.GetRoleInfo(PlayerController.GetPlayer());
        ComplainEvent.Invoke(new ComplainEvent {
            targetType = GetTargetType(),
            targetId = GetTargetId(),
            targetName = GetTargetName(),
            serverId = role.serverId,
            roleId = GetRoleId() ,
            roleName = role.roleName,
            width = 100,
            height = 100
        });
        Destroy();
    }

    private string GetTargetType()
    {
        if(rankType == RankType.Character)
        {
            return "role";
        }
        else if(rankType == RankType.Wanjia)
        {
            return "wanjia";
        }
        else
        {
            return "zongmen";
        }
    }

    private string GetTargetId()
    {
        if(rankType == RankType.Character)
        {
            return playerId;
        }
        else if(rankType == RankType.Wanjia)
        {
            return wanjiaId.text;
        }
        else 
        {
            return zongmenNumber.text;
        }
    }

    private string GetTargetName()
    {
        if(rankType == RankType.Character)
        {
            return accountName.text;
        }
        else if(rankType == RankType.Wanjia)
        {
            return wanjiaName.text;
        }
        else 
        {
            return zongmenName.text;
        }
    }

    private string GetRoleId() 
    {
        string roleId;
        if (ComboSDK.IsFeatureAvailable(Feature.SEAYOO_ACCOUNT))
        {
            roleId = ComboSDK.SeayooAccount.UserId;
        }
        else
        {
            roleId = ComboSDK.GetLoginInfo().comboId;
        }
        return roleId;
    }

    public void SetOnComplainCallback(Action OnComplain)
    {
        this.OnComplain = OnComplain;
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
