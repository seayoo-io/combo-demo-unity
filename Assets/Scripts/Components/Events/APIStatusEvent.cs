public class APIClickEvent: Event<APIClickEvent>
{
    public string apiName;
    public string api;
}

public class APITestResultEvent: Event<APITestResultEvent>
{
    public string api;
    public APIInfo.Status status;
}