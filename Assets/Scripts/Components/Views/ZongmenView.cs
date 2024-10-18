using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[ViewPrefab("Prefabs/ZongmenView")]
internal class ZongmenView : View<ZongmenView>
{
    public Text zongmenNameText;
    public Text levelText;
    public Text numberText;
    public Text patriarchText;
    private Zongmen zongmen = new Zongmen();
    void Awake()
    {
        EventSystem.Register(this);
    }

    public void OnOpenComplainView(){
        var view = ComplainView.Instantiate();
        view.ComplainType(RankType.Zongmen);
        view.SetZongmenName(zongmen.name);
        view.SetZongmenLevel(zongmen.level);
        view.SetZongmenNumber(zongmen.number);
        view.SetZongmenPatriarch(zongmen.patriarch);
    }

    void OnDestroy()
    {
        EventSystem.UnRegister(this);
    }

    public void SetZongmenInfo(string zongmenName, string level, string number, string patriarch) {
        zongmenNameText.text = zongmenName;
        levelText.text = $"LV.{level}";
        numberText.text = $"{number}/15";
        patriarchText.text = $"族长：{patriarch}";
        zongmen.name = zongmenName;
        zongmen.level = level;
        zongmen.number = number;
        zongmen.patriarch = patriarch;
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
