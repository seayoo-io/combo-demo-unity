public class PreloadAdEvent : Event<PreloadAdEvent> {
    public string placementId;
    public string scenarioId;
}

public class ShowAdEvent : Event<ShowAdEvent> {
    public string placementId;
    public string scenarioId;
}