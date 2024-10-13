using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[ViewPrefab("Prefabs/RankView")]
internal class RankView : View<RankView>
{
   public Button homeBtn;
    public Action OnGoHome;

    void Awake()
    {
        homeBtn.onClick.AddListener(OnGoHomeConfigBtn);
    }

    void OnDestroy()
    {
        homeBtn.onClick.RemoveListener(OnGoHomeConfigBtn);
    }

    void OnGoHomeConfigBtn()
    {
        OnGoHome.Invoke();
    }

    public void SetGoHomeCallback(Action OnGoHome)
    {
        this.OnGoHome = OnGoHome;
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
