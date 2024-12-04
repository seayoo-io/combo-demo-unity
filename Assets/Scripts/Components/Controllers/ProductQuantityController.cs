using UnityEngine;
using UnityEngine.UI;

public class ProductQuantityController : MonoBehaviour
{
    public Button addBtn;
    public Button subBtn;
    public InputField productQuantityText;
    private int productQuantity = 1;

    void Start()
    {
        EventSystem.Register(this);
    }

    void OnDestroy()
    {
        EventSystem.UnRegister(this);
    }

    public void ModifyProductQuantity()
    {
        ModifyProductQuantityEvent.Invoke(new ModifyProductQuantityEvent
        {
            quantity = productQuantity,
        });
    }

    [EventSystem.BindEvent]
    void HandlePurchaseEvent(ConfirmPurchaseEvent evt)
    {
        OnPurchase(evt.productId);
    }

    public void OnPurchase(string productId)
    {
        ProductManager.productManager.OnPurchase(productId, productQuantity);
    }

    public void OnAddProductQuantity()
    {
        if(productQuantity >= 100)
        {
            return;
        }
        productQuantity++;
        productQuantityText.text = $"{productQuantity}";
    }

    public void OnSubProductQuantity()
    {
        if(productQuantity <= 1)
        {
            return;
        }
        productQuantity--;
        productQuantityText.text = $"{productQuantity}";
    }

    public void SynchronizeProductQuantity()
    {
        int numInt;
        if (int.TryParse(productQuantityText.text, out numInt))
        {
            productQuantity = numInt == 0 ? 1 : numInt;
        }
        else
        {
            productQuantity = 1;
        }
        productQuantityText.text = productQuantity.ToString();
        ModifyProductQuantity();
    }
}