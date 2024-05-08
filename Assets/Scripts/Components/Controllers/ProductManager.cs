using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Combo;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ProductManager : MonoBehaviour
{
    public Transform parentTransform;
    public PlayerPanel playerPanel;
    public static ProductManager productManager;
    private string[] limitProducts;

    void Start()
    {
        EventSystem.Register(this);
        limitProducts = GameClient.GetLimitProduct();
        GameClient.GetListProduct(data =>
        {
            foreach (var productInfo in data)
            {
                AppendProductView(productInfo.productId,
                                productInfo.productName,
                                "￥" + (productInfo.price / 100).ToString(),
                                productInfo.iconUrl);
            }
        });
        productManager = this;
    }

    void OnDestroy()
    {
        EventSystem.UnRegister(this);
    }

    public void AppendProductView(
        string productId,
        string productName,
        string productPrice,
        string productImageUrl
    )
    {
        var view = ProductView.Instantiate();
        view.SetProductId(productId);
        view.SetProductName(productName);
        view.SetProductPrice(productPrice);
        view.gameObject.transform.SetParent(parentTransform, false);
        if(limitProducts != null && limitProducts.Contains(productId))
        {
            view.SetLimitProductText();
        }
        if (!string.IsNullOrEmpty(productImageUrl)) {
            GameClient.GetProductImg(
                productImageUrl,
                data => view.SetProductImage(data)
            );
        }
        view.Show();
    }

    [EventSystem.BindEvent]
    void HandlePurchaseEvent(PurchaseEvent evt)
    {
        OnOpenPurchaseView(evt);
    }

    public void OnOpenPurchaseView(PurchaseEvent evt)
    {
        if(ComboSDK.IsFeatureAvailable(Feature.PRODUCT_QUANTITY))
        {
            ShowProductQuantityView(evt.productId, evt.productName, evt.productPrice, evt.productImg);
        }
        else
        {
            OnPurchase(evt.productId, 1);
        }
    }

    public void OnPurchase(string productId, int quantity)
    {
        if (string.IsNullOrEmpty(productId))
        {
            Toast.Show("商品 Id 为空");
            return;
        }
        UIController.ShowLoading();
        GameClient.CreateOrder(
            productId,
            quantity,
            orderToken =>
            {
                var opts = new PurchaseOptions { orderToken = orderToken };
                ComboSDK.Purchase(
                    opts,
                    r =>
                    {
                        UIController.HideLoading();
                        if (r.IsSuccess)
                        {
                            var result = r.Data;
                            Toast.Show("购买完成：OrderId - " + result.orderId);
                            Log.I("购买完成：OrderId - " + result.orderId);
                            if(productId.Contains("limit_"))
                            {
                                PurchaseSuccessEvent.Invoke(new PurchaseSuccessEvent { productId = productId });
                            }
                            RequestUpdateCoinEvent.Invoke(new RequestUpdateCoinEvent());
                        }
                        else
                        {
                            var err = r.Error;
                            Toast.Show("购买失败：" + err.Message);
                            if (err.Code == ErrorCode.UserCancelled)
                            {
                                Log.I("用户取消购买：" + err.DetailMessage);
                            }
                            else
                            {
                                Log.E("购买失败：" + err.DetailMessage);
                            }
                        }
                    }
                );
            },
            () =>
            {
                UIController.HideLoading();
            }
        );
    }

    private void ShowProductQuantityView(
        string productId, 
        string productName, 
        string productPrice,
        Image productImage
    )
    {
        var productQuantityView = ProductQuantityView.Instantiate();
        productQuantityView.SetProductId(productId);
        productQuantityView.SetProductName(productName);
        productQuantityView.SetProductPrice(productPrice);
        productQuantityView.SetProductImage(productImage);
        productQuantityView.SetCurrentPrice(productPrice, 1);
        productQuantityView.SetCancelCallback(() => productQuantityView.Destroy());
        productQuantityView.SetQuitCallback(() => productQuantityView.Destroy());
        productQuantityView.Show();
    }
}
