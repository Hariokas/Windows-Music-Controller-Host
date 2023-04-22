using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_TestPlayground.EventClasses
{
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
}
