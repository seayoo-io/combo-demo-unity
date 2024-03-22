public class UpdateGameFinishedEvent : Event<UpdateGameFinishedEvent> {
    public bool forceUpdate;
    public bool success;
}