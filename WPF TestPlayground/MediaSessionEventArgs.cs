using System;

namespace WPF_TestPlayground;

public class MediaSessionEventArgs : EventArgs
{
    public MediaSessionEventArgs(MediaSessionEvent mediaSessionEvent)
    {
        MediaSessionEvent = mediaSessionEvent;
    }

    public MediaSessionEvent MediaSessionEvent { get; }
}