using UnityEngine.UI;

public class PurchaseEvent : Event<PurchaseEvent> {
    public string productId;
    public string productName;
    public string productPrice;
    public Image productImg;
}

public class CoinUpdatedEvent : Event<CoinUpdatedEvent> {
    public int coin;
}

public class RequestUpdateCoinEvent : Event<RequestUpdateCoinEvent> {
    public int coinOffset = 0;
}

public class ConfirmPurchaseEvent : Event<ConfirmPurchaseEvent> {
    public string productId;
}

public class ModifyProductQuantityEvent : Event<ModifyProductQuantityEvent> {
    public int quantity;
}

public class PurchaseSuccessEvent : Event<PurchaseSuccessEvent> {
    public string productId;
}