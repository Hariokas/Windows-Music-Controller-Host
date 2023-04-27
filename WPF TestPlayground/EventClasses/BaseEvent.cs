namespace WPF_TestPlayground.EventClasses;

public enum BaseEventType
{
    MasterVolumeEvent,
    MediaSessionEvent,
    VolumeMixerEvent
}

public class BaseEvent
{
    public BaseEventType EventType { get; set; }
}