namespace Media_Controller_Remote_Host.EventClasses;

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