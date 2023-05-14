using System;

namespace Media_Controller_Remote_Host.EventClasses;

public class VolumeMixerEventArgs : EventArgs
{
    public VolumeMixerEventArgs(VolumeMixerEvent volumeMixerEvent)
    {
        VolumeMixerEvent = volumeMixerEvent;
    }

    public VolumeMixerEvent VolumeMixerEvent { get; }
}