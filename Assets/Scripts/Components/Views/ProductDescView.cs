using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ViewPrefab("Prefabs/ProductDescView")]
internal class ProductDescView : View<ProductDescView>
{
    public Button cancelBtn;
    public Button confirmBtn;
    public InputField producrDescText;
    public Action<string> confirmAction;
    void Awake()
    {
        EventSystem.Register(this);
        cancelBtn.onClick.AddListener(OnCancelConfigBtn);
        confirmBtn.onClick.AddListener(OnConfirmPurchaseEvent);
    }

    void OnDestroy()
    {
        EventSystem.UnRegister(this);
        cancelBtn.onClick.RemoveListener(OnCancelConfigBtn);
        confirmBtn.onClick.RemoveListener(OnConfirmPurchaseEvent);
    }


    void OnCancelConfigBtn()
    {
        Destroy();
    }

    public void OnConfirmPurchaseEvent(){
        confirmAction.Invoke(producrDescText.text);
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
