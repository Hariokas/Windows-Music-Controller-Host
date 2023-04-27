using System.Collections.Generic;
using WPF_TestPlayground.Models;

namespace WPF_TestPlayground.EventClasses;

public enum VolumeMixerEventType
{
    GetApplicationVolumes,
    SetApplicationVolume
}

public class VolumeMixerEvent : BaseEvent
{
    public VolumeMixerEvent()
    {
        EventType = BaseEventType.VolumeMixerEvent;
    }

    public VolumeMixerEventType VolumeMixerEventType { get; set; }

    public List<ApplicationVolume> ApplicationVolumes { get; set; }

    public ApplicationVolume ApplicationVolume { get; set; }
}