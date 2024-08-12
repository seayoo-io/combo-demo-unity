using System.Collections.Generic;
using Combo;
using UnityEngine;

public class ReportDataManager : MonoBehaviour
{
    void Start()
    {
        EventSystem.Register(this);
    }

    void OnDestroy()
    {
        EventSystem.UnRegister(this);
    }

    [EventSystem.BindEvent]
    void HandlePromoPseudoPurchaseEvent(PromoPseudoPurchaseEvent evt)
    {
        string amount = evt.amount;
        if (string.IsNullOrEmpty(evt.amount))
        {
            amount = "0";
        }
        OnPromoPseudoPurchase(int.Parse(amount));
    }

    public void OnPromoPseudoPurchase(int amount)
    {
        PromoPseudoPurchaseOptions opts = new PromoPseudoPurchaseOptions()
        {
            Amount = amount,
        };
        ComboSDK.PromoPseudoPurchase(opts);
    }
}
