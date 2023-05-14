using System;
using NAudio.CoreAudioApi;

namespace Media_Controller_Remote_Host.Controllers;

public class MasterVolumeController
{
    public float GetMasterVolume()
    {
        using var deviceEnumerator = new MMDeviceEnumerator();
        using var device = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        return device.AudioEndpointVolume.MasterVolumeLevelScalar;
    }

    public void SetMasterVolume(float volume)
    {
        if (volume is < 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(volume), "Volume must be in the range of 0 to 1.");

        using var deviceEnumerator = new MMDeviceEnumerator();
        using var device = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        device.AudioEndpointVolume.MasterVolumeLevelScalar = volume;
    }

    public bool IsMuted()
    {
        using var deviceEnumerator = new MMDeviceEnumerator();
        using var device = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        return device.AudioEndpointVolume.Mute;
    }

    public void SetMute(bool mute)
    {
        using var deviceEnumerator = new MMDeviceEnumerator();
        using var device = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        device.AudioEndpointVolume.Mute = mute;
    }
}