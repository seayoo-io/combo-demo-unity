using System;
using System.Collections;
using System.Collections.Generic;
using Combo;
using UnityEngine;
using UnityEngine.UI;

[ViewPrefab("Prefabs/ProductView")]
internal class ProductView : View<ProductView>
{
    public Button purchaseBtn;
    public Text productIdTxt;
    public Text productNameTxt;
    public Text productPriceTxt;
    public Image productImg;

    void Start()
    {
        ButtonManager.SetButtonEnabledByType(purchaseBtn, ButtonType.PurchaseButton);
    }

    public void OnPurchase(){
        PurchaseEvent.Invoke(new PurchaseEvent {
            productId = productIdTxt.text,
            productName = productNameTxt.text,
            productPrice = productPriceTxt.text,
            productImg = productImg
        });
    }
    
    public void SetProductId(string productId) {
        productIdTxt.text = productId;
    }

    public void SetProductName(string productName) {
        productNameTxt.text = productName;
    }

    public void SetProductPrice(string productPrice) {
        productPriceTxt.text = productPrice;
    }

    public void SetProductImage(Texture2D productImage) {
        productImg.sprite = Sprite.Create(productImage, new Rect(0, 0, productImage.width, productImage.height), new Vector2(0.5f, 0.5f));
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
