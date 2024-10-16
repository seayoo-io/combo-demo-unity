using Combo;
using UnityEngine;
using UnityEngine.UI;

public enum RankType
{
    Character,
    Zongmen
}

[System.Serializable]
public class Character
{
    public RankType rankType = RankType.Character;
    public string rank;
    public string roleName;
    public string score;
    public string roleId;
    public Sprite characterImage;
}

[System.Serializable]
public class Zongmen
{
    public RankType rankType = RankType.Zongmen;
    public string name;
    public string level;
    public string number;
    public string patriarch;
}

public class RankManager : MonoBehaviour
{
    public Transform parentTransform;
    public Button characterBtn;
    public Button zongmenBtn;
    public Character[] characters;
    public Zongmen[] zongmens;
    public static RankManager rankManager;
    private Button selectedBtn;

    public void Start()
    {
        EventSystem.Register(this);
        characterBtn.Select();
        foreach(Character character in characters)
        {
            AppendCharacter(character);
        }
        rankManager = this;
        selectedBtn = characterBtn;
    }

    public void Destroy()
    {
        EventSystem.UnRegister(this);
    }

    public void Update()
    {
        if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == null)
        {
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(selectedBtn.gameObject);
        }
    }


    public void OnCharacterBtn()
    {
        selectedBtn = characterBtn;
        Clear();
        foreach(Character character in characters)
        {
            AppendCharacter(character);
        }
    }

    public void OnZongmenBtn()
    {
        selectedBtn = zongmenBtn;
        Clear();
        foreach(Zongmen zongmen in zongmens)
        {
            AppendZongmen(zongmen);
        }
    }

    private void Clear()
    {
        foreach (Transform child in parentTransform)
        {
            Destroy(child.gameObject);
        }
    }

    private void AppendCharacter(Character character)
    {
        var view = CharacterView.Instantiate();
        view.gameObject.transform.SetParent(parentTransform, false);
        // view.gameObject.transform.localScale = Vector3.one;
        view.SetCharacter(character);
        view.Show();
    }

    private void AppendZongmen(Zongmen zongmen)
    {
        var view = ZongmenView.Instantiate();
        view.gameObject.transform.SetParent(parentTransform, false);
        // view.gameObject.transform.localScale = Vector3.one;
        view.SetZongmenInfo(zongmen.name, zongmen.level, zongmen.number, zongmen.patriarch);
        view.Show();
    }

    [EventSystem.BindEvent]
    public void Complain(ComplainEvent evt)
    {
        var opts = new ComplainOptions()
        {
            TargetType = evt.targetType,
            TargetId = evt.targetId,
            TargetName = evt.targetName,
            ServerId = evt.serverId,
            RoleId = evt.roleId,
            RoleName = evt.roleName,
            Width = evt.width,
            Height = evt.height
        };
        ComboSDK.Complain(opts, result =>{
            if(result.IsSuccess)
            {
                Toast.Show("游戏内举报成功");
                Log.I("游戏内举报成功");
            }
            else
            {
                Toast.Show($"举报界面打开失败：{result.Error.Message}");
            }
        });
    }
}