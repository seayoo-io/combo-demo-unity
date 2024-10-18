using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[ViewPrefab("Prefabs/WanjiaView")]
internal class WanjiaView : View<WanjiaView>
{
    public Text wanjiaNameText;
    public Text levelText;
    public Text wanjiaIdText;
    private Wanjia wanjia = new Wanjia();
    void Awake()
    {
        EventSystem.Register(this);
    }

    public void OnOpenComplainView(){
        var view = ComplainView.Instantiate();
        view.ComplainType(RankType.Wanjia);
        view.SetWanjiaName(wanjia.wanjiaName);
        view.SetWanjiaLevel(wanjia.level);
        view.SetWanjiaId(wanjia.wanjiaId);
    }

    void OnDestroy()
    {
        EventSystem.UnRegister(this);
    }

    public void SetWanjiaInfo(Wanjia wanjia) {
        wanjiaNameText.text = wanjia.wanjiaName;
        levelText.text = $"LV.{wanjia.level}";
        wanjiaIdText.text = $"Id: {wanjia.wanjiaId}";
        this.wanjia.wanjiaName = wanjia.wanjiaName;
        this.wanjia.level = wanjia.level;
        this.wanjia.wanjiaId = wanjia.wanjiaId;
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
