using System;
using System.Diagnostics;
using Media_Controller_Remote_Host.Controllers;
using Media_Controller_Remote_Host.EventClasses;

namespace Media_Controller_Remote_Host.Handlers
{
    public class VolumeMixerHandler
    {
        private readonly VolumeMixerController _controller = new();
        public event EventHandler<VolumeMixerEventArgs> SendMessageRequested;

        public async void VolumeMixerCommandReceived(object sender, VolumeMixerEventArgs args)
        {
            try
            {
                switch (args.VolumeMixerEvent.VolumeMixerEventType)
                {
                    case VolumeMixerEventType.GetApplicationVolumes:

                        var appVolumesList = _controller.GetApplicationVolumes();
                        SendMessageRequested?.Invoke(this, new VolumeMixerEventArgs(new VolumeMixerEvent
                        {
                            VolumeMixerEventType = VolumeMixerEventType.GetApplicationVolumes,
                            ApplicationVolumes = appVolumesList
                        }));
                        break;

                    case VolumeMixerEventType.SetApplicationVolume:

                        var application = args.VolumeMixerEvent.ApplicationVolume;
                        if (application == null || application.Volume == null || application.ProcessID == null) return;

                        _controller.SetApplicationVolume(application.ProcessID, (float)application.Volume);
                        break;

                    default:
                        Trace.WriteLine($"Unknown event type: {args.VolumeMixerEvent.VolumeMixerEventType}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[VolumeMixerCommandReceived]: Error: {ex}");
            }
        }

    }
}
