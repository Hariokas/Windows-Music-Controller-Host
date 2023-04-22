using System;

namespace WPF_TestPlayground.EventClasses;

public class MasterVolumeEventArgs : EventArgs
{
    public MasterVolumeEventArgs(MasterVolumeEvent masterVolumeEvent)
    {
        MasterVolumeEvent = masterVolumeEvent;
    }

    public MasterVolumeEvent MasterVolumeEvent { get; }
}