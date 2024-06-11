using JetBrains.Annotations;
using Networking;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using Types;
using UnityEngine;
using UnityEngine.Networking;

class ErrorResponse : Serializable
{
    [JsonProperty("error")]
    public string error;

    [JsonProperty("message")]
    public string message;
}

[System.Serializable]
class LoginRequest : Serializable
{
    public string token;
}

[System.Serializable]
class LoginResponse : Serializable
{
    [JsonProperty("session_token")]
    public string sessionToken;
    [JsonProperty("granted_limit_products")]
    public string[] grantedLimitProduct;
}

[System.Serializable]
public class CreateOrderRequest : Serializable
{
    [JsonProperty("product_id")]
    public string productId;
    public string platform;
    [JsonProperty("quantity")]
    public int quantity;
}

[System.Serializable]
public class CreateOrderResponse : Serializable
{
    [JsonProperty("order_token")]
    public string orderToken;
}

[System.Serializable]
class GetPlayerItemsRequest : Serializable
{
}

[System.Serializable]
public class PlayerItems : Serializable
{
    [JsonProperty("item_id")]
    public string itemId;
    [JsonProperty("item_count")]
    public int itemCount;
}

[System.Serializable]
public class ListProduct : Serializable
{

    [JsonProperty("product_id")]
    public string productId;
    [JsonProperty("product_name")]
    public string productName;
    [JsonProperty("price")]
    public double price;
    [JsonProperty("icon_url")]
    public string iconUrl;
}


public static class GameClient
{
    private static string session = "";
    private static string gameId = "";
    private static string endpoint = "";
    private static string[] limitProduct = {};

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Setup()
    {
        var demoEndpoint = BuildParams.Load().demoEndpoint ?? "https://combo-demo.dev.seayoo.com";

        GameClient.gameId = Combo.ComboSDK.GetGameId();
        GameClient.endpoint = demoEndpoint;
    }
    
    public static void Login(string token, Action<bool> action)
    {
        HttpRequest.Post(new HttpRequestOptions
        {
            url = $"{endpoint}/{gameId}/login",
            body = new LoginRequest { token = token },
            headers = Headers()
        }, resp =>
        {
            if (resp.IsSuccess)
            {
                try
                {
                    var data = resp.Body.ToJson<LoginResponse>();
                    session = data.sessionToken;
                    limitProduct = data.grantedLimitProduct;
                    action.Invoke(true);
                }
                catch (Exception e)
                {
                    LogError("Login - ParseDataFailed: " + e.ToString());
                }
            } else
            {
                try {
                    var body = resp.Body.ToJson<ErrorResponse>();
                    LogErrorWithToast(body);
                } catch (Exception)
                {
                    LogErrorWithToast(resp.Body.ToText());
                }
                action.Invoke(false);
            }
        });
    }

    public static void Logout()
    {
        session = "";
    }

    public static void CreateOrder(string productId, int quantity, Action<string> onOrderToken, Action onError)
    {
        if (string.IsNullOrEmpty(session))
        {
            LogErrorWithToast("用户未登录");
            onError.Invoke();
            return;
        }
        HttpRequest.Post(new HttpRequestOptions
        {
            url = $"{endpoint}/{gameId}/create-order",
            body = new CreateOrderRequest { 
                productId = productId,
                quantity = quantity,
#if UNITY_STANDALONE
                platform = "windows",
#elif UNITY_ANDROID
                platform = "android",
#elif UNITY_IOS
                platform = "ios",
#endif
            },
            headers = Headers()
        }, resp =>
        {
            Log.D(resp.ToString());
            if (resp.IsSuccess)
            {
                try
                {
                    var data = resp.Body.ToJson<CreateOrderResponse>();
                    onOrderToken.Invoke(data.orderToken);
                } catch (Exception e)
                {
                    LogError("CreateOrder ParseDataFailed: " + e.ToString());
                }
            } else
            {
                try
                {
                    LogErrorWithToast(resp.Body.ToJson<ErrorResponse>());
                } catch (Exception)
                {
                    LogErrorWithToast(resp.Body.ToText());
                }
                onError.Invoke();
            }
        });
    }

    public static void GetPlayerItems(Action<PlayerItems[]> action)
    {
        HttpRequest.Get(new HttpRequestOptions
        {
            url = $"{endpoint}/{gameId}/get-player-items",
            body = new GetPlayerItemsRequest { },
            headers = Headers()
        }, resp =>
        {
            Log.D(resp.ToString());
            try
            {
                var data = resp.Body.ToJson<PlayerItems[]>();
                if (resp.IsSuccess)
                {
                    action.Invoke(data);
                }
                else
                {
                    LogErrorWithToast(resp.Body.ToText());
                    action.Invoke(new PlayerItems[0]);
                }
            } catch (Exception)
            {
                action.Invoke(new PlayerItems[0]);
            }
        });
    }

    public static void GetListProduct(Action<ListProduct[]> action) {
        HttpRequest.Get(new HttpRequestOptions{
            url = $"{endpoint}/{gameId}/list-products",
            headers = Headers()
        }, resp =>
        {
            Log.D(resp.ToString());
            try
            {
                var data = resp.Body.ToJson<ListProduct[]>();
                if(resp.IsSuccess)
                {
                    action.Invoke(data);
                }
                else{
                    LogErrorWithToast(resp.Body.ToText());
                }
            } catch(Exception)
            {
                
            }
        });
    }

    public static void GetProductImg(string url, Action<Texture2D> action){
        HttpRequest.Get(new HttpRequestOptions {
            url = url,
            headers = Headers()
        }, resp => {
            action.Invoke(resp.Body.ToImage());
        });
    }

    public static string[] GetLimitProduct()
    {
        return limitProduct;
    }

    private static Dictionary<string, string> Headers()
    {
        return new Dictionary<string, string> { {
            "x-session-token", session
            } };
    }

    public static string GetClientEndPoint()
    {
        return endpoint;
    }

    private static void LogErrorWithToast(ErrorResponse resp)
    {
        if(resp.error == "invalid headers")
        {
            Toast.Show("登陆状态失效，请重新登陆");
        }
        else
        {
            Toast.Show(resp.message);
        }
        Log.E($"DemoServer: {resp.message}");
    }

    private static void LogErrorWithToast(object message)
    {
        Toast.Show(message);
        Log.E($"DemoServer: {message}");
    }
    private static void LogError(object message)
    {
        Log.E($"DemoServer: {message}");
    }
}