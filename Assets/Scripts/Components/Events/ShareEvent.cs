using Combo;

public class ImgShareEvent : Event<ImgShareEvent> {
    public string title;
    public string contents;
    public string imageUrl;
    public string hashtag;
}

public class VideoShareEvent : Event<VideoShareEvent> {
    public string title;
    public string contents;
    public string videoUrl;
    public string videoCoverUrl;
    public string hashtag;
}