// 公告打开事件，OpenAnnouncements 成功后由 WebViewParameterView 发出
public class OpenAnnouncementsEvent : Event<OpenAnnouncementsEvent> {}

// 公告列表项选中事件，携带被选中的公告数据，由 AnnouncementCellView 发出，AnnouncementView 监听
public class SelectAnnouncementEvent : Event<SelectAnnouncementEvent>
{
    // 被选中的公告数据
    public AnnouncementData Data;
}
