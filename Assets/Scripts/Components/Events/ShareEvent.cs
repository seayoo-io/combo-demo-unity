using Combo;

public class ImgShareEvent : Event<ImgShareEvent> {
    public string title;
    public string text;
    public string imageUrl;
    public string hashtag;
}

public class VideoShareEvent : Event<VideoShareEvent> {
    public string title;
    public string text;
    public string videoUrl;
    public string videoCoverUrl;
    public string hashtag;
}

public class LinkShareEvent : Event<LinkShareEvent> {
    public string title;
    public string text;
    public string linkUrl;
    public string linkCoverUrl;
}