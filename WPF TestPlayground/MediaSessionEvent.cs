namespace WPF_TestPlayground;

public enum EventType
{
    NewSession,
    CloseSession,
    SongChanged,
    PlaybackStatusChanged,
    SessionFocusChanged,
    Play,
    Pause,
    Previous,
    Next
}

public class MediaSessionEvent
{
    public EventType EventType { get; set; }
    public int MediaSessionId { get; set; }
    public string MediaSessionName { get; set; }
    public string Artist { get; set; }
    public string SongName { get; set; }
    public string PlaybackStatus { get; set; }
}