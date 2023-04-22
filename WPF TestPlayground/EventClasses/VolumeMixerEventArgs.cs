using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_TestPlayground.EventClasses
{
    public class VolumeMixerEventArgs : EventArgs
    {
        public VolumeMixerEvent VolumeMixerEvent { get; }

        public VolumeMixerEventArgs(VolumeMixerEvent volumeMixerEvent)
        {
            VolumeMixerEvent = volumeMixerEvent;
        }
    }
}
