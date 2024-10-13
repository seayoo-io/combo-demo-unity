using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[ViewPrefab("Prefabs/ComplainView")]
internal class ComplainView : View<ComplainView>
{
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
        if(rankType == RankType.Character)
        {
            CharacterPanel.gameObject.SetActive(true);
            ZongmenPanel.gameObject.SetActive(false);
        }
        else
        {
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
        OnComplain.Invoke();
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
