using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[ViewPrefab("Prefabs/CharacterView")]
internal class CharacterView : View<CharacterView>
{
    public Text rankText;
    public Text characterNameText;
    public Text scoreText;
    public Image characterImg;
    private string roleId;
    void Awake()
    {
        EventSystem.Register(this);
    }

    void OnDestroy()
    {
        EventSystem.UnRegister(this);
    }

    public void OnOpenComplainView(){
        var view = ComplainView.Instantiate();
        view.ComplainType(RankType.Character);
        view.SetAccountName(characterNameText.text);
        view.SetPlayerId(roleId);
        view.SetPlayerImage(characterImg.sprite);
    }
    
    public void SetCharacter(Character character) {
        rankText.text = character.rank;
        characterNameText.text = character.roleName;
        scoreText.text = character.score;
        characterImg.sprite = character.characterImage;
        roleId = character.roleId;
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
