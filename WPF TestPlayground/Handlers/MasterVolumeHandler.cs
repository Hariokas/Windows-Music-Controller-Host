using System;
using System.Diagnostics;
using WPF_TestPlayground.Controllers;
using WPF_TestPlayground.EventClasses;

namespace WPF_TestPlayground.Handlers;

public class MasterVolumeHandler
{
    private readonly MasterVolumeController _controller = new();
    public event EventHandler<MasterVolumeEventArgs> SendMessageRequested;

    public async void MasterVolumeCommandReceived(object sender, MasterVolumeEventArgs args)
    {
        try
        {
            switch (args.MasterVolumeEvent.MasterVolumeEventType)
            {
                case MasterVolumeEventType.GetIsMuted:
                    var isMuted = _controller.IsMuted();
                    SendMessageRequested?.Invoke(this, new MasterVolumeEventArgs(
                        new MasterVolumeEvent
                        {
                            MasterVolumeEventType = MasterVolumeEventType.GetIsMuted,
                            IsMuted = isMuted 

                        }));
                    break;

                case MasterVolumeEventType.SetMute:
                    bool? muteStatus = args.MasterVolumeEvent.IsMuted;
                    if (muteStatus is null) return;
                    _controller.SetMute(muteStatus.Value);
                    break;

                case MasterVolumeEventType.GetMasterVolume:
                    var volumeLevel = _controller.GetMasterVolume();
                    SendMessageRequested?.Invoke(this, new MasterVolumeEventArgs(
                        new MasterVolumeEvent
                        {
                            MasterVolumeEventType = MasterVolumeEventType.GetMasterVolume,
                            MasterVolume = volumeLevel
                        }));
                    break;

                case MasterVolumeEventType.SetMasterVolume:
                    float? setVolumeLevel = args.MasterVolumeEvent.MasterVolume;
                    if (setVolumeLevel is null) return;
                    _controller.SetMasterVolume(setVolumeLevel.Value);
                    break;

                default:
                    Trace.WriteLine($"Unknown event type: {args.MasterVolumeEvent.MasterVolumeEventType}");
                    break;
            }
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"[MasterVolumeCommandReceived]: Error: {ex}");
        }
    }
}