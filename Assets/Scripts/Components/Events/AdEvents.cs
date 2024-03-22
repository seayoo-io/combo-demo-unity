public class PreloadAdEvent : Event<PreloadAdEvent> {
    public string placementId;
}

public class ShowAdEvent : Event<ShowAdEvent> {
    public string placementId;
}