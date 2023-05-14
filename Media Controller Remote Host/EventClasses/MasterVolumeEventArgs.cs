using System;

namespace Media_Controller_Remote_Host.EventClasses;

public class MasterVolumeEventArgs : EventArgs
{
    public MasterVolumeEventArgs(MasterVolumeEvent masterVolumeEvent)
    {
        MasterVolumeEvent = masterVolumeEvent;
    }

    public MasterVolumeEvent MasterVolumeEvent { get; }
}