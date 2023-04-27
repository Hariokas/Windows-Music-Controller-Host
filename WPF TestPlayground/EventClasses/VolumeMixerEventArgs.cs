using System;

namespace WPF_TestPlayground.EventClasses;

public class VolumeMixerEventArgs : EventArgs
{
    public VolumeMixerEventArgs(VolumeMixerEvent volumeMixerEvent)
    {
        VolumeMixerEvent = volumeMixerEvent;
    }

    public VolumeMixerEvent VolumeMixerEvent { get; }
}