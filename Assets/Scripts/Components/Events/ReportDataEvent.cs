public class PromoPseudoPurchaseEvent : Event<PromoPseudoPurchaseEvent> {
    public string amount;
}
public class LoginEvent : Event<LoginEvent> {
}
public class ActiveValueEvent : Event<ActiveValueEvent> {
    public string value;
}