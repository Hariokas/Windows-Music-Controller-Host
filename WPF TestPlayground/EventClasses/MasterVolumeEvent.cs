namespace WPF_TestPlayground.EventClasses;

public enum MasterVolumeEventType
{
    GetMasterVolume,
    SetMasterVolume,
    GetIsMuted,
    SetMute
}

public class MasterVolumeEvent : BaseEvent
{
    public MasterVolumeEvent()
    {
        EventType = BaseEventType.MasterVolumeEvent;
    }

    public MasterVolumeEventType MasterVolumeEventType { get; set; }

    public bool IsMuted { get; set; }

    public float MasterVolume { get; set; }

}