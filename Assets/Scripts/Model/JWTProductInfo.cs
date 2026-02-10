// JWTProductInfo 对象
[System.Serializable]
public class JWTProductInfo
{
    public string iss { get; set; }
    public string sub { get; set; }
    public string aud { get; set; }
    public long exp { get; set; }
    public long iat { get; set; }
    public string scope { get; set; }
    [Newtonsoft.Json.JsonProperty("order_id")]
    public string orderId { get; set; }
    public int amount { get; set; }
    public string currency { get; set; }
    [Newtonsoft.Json.JsonProperty("product_id")]
    public string productId { get; set; }
    public int quantity { get; set; }
}