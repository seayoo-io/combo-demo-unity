using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ViewPrefab("Prefabs/ProductQuantityView")]
internal class ProductQuantityView : View<ProductQuantityView>
{
    public Button cancelBtn;
    public Button quitBtn;
    public Text productIdTxt;
    public Text productNameTxt;
    public Text productPriceTxt;
    public Text CurrentPriceTxt;
    public Image productImg;
    private Action OnQuit;
    private Action OnCancel;
    private string productPrice;

    void Awake()
    {
        EventSystem.Register(this);
        cancelBtn.onClick.AddListener(OnCancelConfigBtn);
        quitBtn.onClick.AddListener(OnQuitConfigBtn);
    }

    void OnDestroy()
    {
        EventSystem.UnRegister(this);
        cancelBtn.onClick.RemoveListener(OnCancelConfigBtn);
        quitBtn.onClick.RemoveListener(OnQuitConfigBtn);
    }

    [EventSystem.BindEvent]
    void HandleModifyProductQuantityEvent(ModifyProductQuantityEvent evt)
    {
        SetCurrentPrice(productPrice, evt.quantity);
    }

    void OnQuitConfigBtn()
    {
        OnQuit.Invoke();
    }

    void OnCancelConfigBtn()
    {
        OnCancel.Invoke();
    }

    public void OnConfirmPurchaseEvent(){
        ConfirmPurchaseEvent.Invoke(new ConfirmPurchaseEvent {
            productId = productIdTxt.text,
        });
    }

    public void SetQuitCallback(Action OnQuit)
    {
        this.OnQuit = OnQuit;
    }

    public void SetCancelCallback(Action OnCancel)
    {
        this.OnCancel = OnCancel;
    }

    public void SetProductId(string productId) {
        productIdTxt.text = productId;
    }

    public void SetProductName(string productName) {
        productNameTxt.text = productName;
    }

    public void SetProductPrice(string productPrice) {
        this.productPrice = productPrice;
        productPriceTxt.text = productPrice;
    }

    public void SetProductImage(Image productImage) {
        Texture2D texture = SpriteToTexture2D(productImage.sprite);
        productImg.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }

    public void SetCurrentPrice(string price, int quantity) {
        var currentPrice = quantity * decimal.Parse(price.Replace("￥", ""));
        CurrentPriceTxt.text = currentPrice.ToString();
    }

    protected override IEnumerator OnHide()
    {
        yield return null;
    }

    protected override IEnumerator OnShow()
    {
        yield return null;
    }

    private Texture2D SpriteToTexture2D(Sprite sprite)
    {
        Texture2D texture = new Texture2D((int)sprite.textureRect.width, (int)sprite.textureRect.height);
        var pixels = sprite.texture.GetPixels((int)sprite.textureRect.x, (int)sprite.textureRect.y, (int)sprite.textureRect.width, (int)sprite.textureRect.height);
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }
}
