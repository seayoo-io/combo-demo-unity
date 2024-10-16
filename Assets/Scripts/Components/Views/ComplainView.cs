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
    public Text zongmenName;
    public Text zongmenLevel;
    public Text zongmenNumber;
    public Text zongmenPatriarch;
    public Button complainBtn;
    public Button cancelBtn;
    private Action OnComplain;
    public GameObject CharacterPanel;
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
            ZongmenPanel.gameObject.SetActive(false);
        }
        else
        {
            this.rankType = RankType.Zongmen;
            ZongmenPanel.gameObject.SetActive(true);
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
        var opts = new ComplainOptions()
        {
            TargetType = GetTargetType(),
            TargetId = GetTargetId(),
            TargetName = GetTargetName(),
            ServerId = "1",
            RoleId = GetRoleId(),
            RoleName = "举报者名称",
            Width = 100,
            Height = 100
        };
        ComboSDK.Complain(opts, result =>{
            if(result.IsSuccess)
            {
                Destroy();
                Toast.Show("游戏内举报成功");
                Log.I("游戏内举报成功");
            }
            else
            {
                Toast.Show($"公告打开失败：{result.Error.Message}");
            }
        });
    }

    private string GetTargetType()
    {
        if(rankType == RankType.Character)
        {
            return "role";
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
