using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ViewPrefab("Prefabs/StatusView")]
internal class StatusView : View<StatusView>
{

    public Button apiStatusBtn;
    public Button closeBtn;
    private Action OnAPIStatus;
    // Start is called before the first frame update
    void Start()
    {
        apiStatusBtn.onClick.AddListener(OnAPITestBtn);
        closeBtn.onClick.AddListener(Destroy);
    }

    // Update is called once per frame
    void OnDestroy()
    {
        apiStatusBtn.onClick.RemoveListener(OnAPITestBtn);
        closeBtn.onClick.RemoveListener(Destroy);
    }

    void OnAPITestBtn()
    {
        this.OnAPIStatus.Invoke();
    }

    public void SetAPIStatusAction(Action OnAPIStatus)
    {
        this.OnAPIStatus = OnAPIStatus;
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
