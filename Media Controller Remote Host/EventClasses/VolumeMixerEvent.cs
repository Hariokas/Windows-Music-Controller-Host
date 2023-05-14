using System.Collections.Generic;
using Media_Controller_Remote_Host.Models;

namespace Media_Controller_Remote_Host.EventClasses;

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