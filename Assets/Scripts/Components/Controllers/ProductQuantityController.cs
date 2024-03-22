using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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
        productQuantityText.onValidateInput += delegate(string input, int charIndex, char addedChar) 
        {
            return ValidateNumberInput(input, addedChar);
        };
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
            productQuantity = numInt;
        }
        else
        {
            productQuantity = 1;
        }
        ModifyProductQuantity();
    }

    char ValidateNumberInput(string input, char addedChar)
    {
        if (!char.IsDigit(addedChar))
        {
            return '\0';
        }

        var proposedInput = input + addedChar;
        var proposedValue = int.Parse(proposedInput);

        if (proposedValue <= 0 ||proposedValue > 100)
        {
            return '\0';
        }

        return addedChar;
    }
}