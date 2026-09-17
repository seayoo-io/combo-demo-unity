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
    public Text limitProductTxt;
    public Image productImg;

    private const int MaxTextLines = 2;
    // 商品 ID/名称的原始值，购买、限购匹配等业务逻辑用这两个字段，不能用可能被截断的 Text 显示内容
    private string productId;
    private string productName;

    void Awake()
    {
        EventSystem.Register(this);
    }

    void Start()
    {
        ButtonManager.SetButtonEnabledByType(purchaseBtn, ButtonType.PurchaseButton);
    }

    void OnDestroy()
    {
        EventSystem.UnRegister(this);
    }

    public void OnPurchase(){
        PurchaseEvent.Invoke(new PurchaseEvent {
            productId = productId,
            productName = productName,
            productPrice = productPriceTxt.text,
            productImg = productImg
        });
    }

    [EventSystem.BindEvent]
    public void ShowLimitProductText(PurchaseSuccessEvent action)
    {
        if(action.productId == productId)
        {
            limitProductTxt.gameObject.SetActive(true);
        }
    }

    public void SetProductId(string productId) {
        this.productId = productId;
        productIdTxt.text = TruncateToLines(productIdTxt, productId, MaxTextLines);
    }

    public void SetProductName(string productName) {
        this.productName = productName;
        productNameTxt.text = TruncateToLines(productNameTxt, productName, MaxTextLines);
    }

    // 文本按显示区域宽度换行后如果超过 maxLines 行，逐字符收缩并补上省略号，避免横向溢出到相邻格子
    // 高度用 float.MaxValue 探测真实换行行数，不依赖 Text 挂载的 ContentSizeFitter 尚未按内容重算完成的当前高度
    private string TruncateToLines(Text label, string text, int maxLines)
    {
        var generator = label.cachedTextGenerator;
        var settings = label.GetGenerationSettings(new Vector2(label.rectTransform.rect.width, float.MaxValue));
        generator.Populate(text, settings);
        if (generator.lineCount <= maxLines)
        {
            return text;
        }
        for (var length = text.Length - 1; length > 0; length--)
        {
            var candidate = text.Substring(0, length) + "...";
            generator.Populate(candidate, settings);
            if (generator.lineCount <= maxLines)
            {
                return candidate;
            }
        }
        return "...";
    }

    public void SetProductPrice(string productPrice) {
        productPriceTxt.text = productPrice;
    }

    public void SetLimitProductText()
    {
        limitProductTxt.gameObject.SetActive(true);
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
