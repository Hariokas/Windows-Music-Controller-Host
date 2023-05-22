using System;
using System.Collections.Generic;
using System.Diagnostics;
using Media_Controller_Remote_Host.Models;
using NAudio.CoreAudioApi;

namespace Media_Controller_Remote_Host.Controllers;

public class VolumeMixerController
{
    public List<ApplicationVolume> GetApplicationVolumes()
    {
        var appVolumes = new List<ApplicationVolume>();

        var deviceEnumerator = new MMDeviceEnumerator();
        var device = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

        using var sessionManager = device.AudioEndpointVolume;
        var sessions = device.AudioSessionManager.Sessions;

        for (var i = 0; i < sessions.Count; i++)
            using (var session = sessions[i])
            {
                var control = session;
                var processId = (int)control.GetProcessID;
                var process = Process.GetProcessById(processId);
                var appName = string.IsNullOrEmpty(process.MainWindowTitle)
                    ? process.ProcessName
                    : process.MainWindowTitle;

                if (appName == "ShellExperienceHost") appName = "System sounds";

                using (var simpleVolume = session.SimpleAudioVolume)
                {
                    appVolumes.Add(new ApplicationVolume
                    {
                        ProcessID = processId,
                        DisplayName = appName,
                        Volume = simpleVolume.Volume
                    });
                }
            }

        return appVolumes;
    }

    public void SetApplicationVolume(int processId, float volume)
    {
        if (volume < 0 || volume > 1)
            throw new ArgumentOutOfRangeException(nameof(volume), "Volume must be in the range of 0 to 1.");

        var deviceEnumerator = new MMDeviceEnumerator();
        var device = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

        using var sessionManager = device.AudioEndpointVolume;
        var sessions = device.AudioSessionManager.Sessions;

        for (var i = 0; i < sessions.Count; i++)
            using (var session = sessions[i])
            {
                var control = session;

                if ((int)control.GetProcessID != processId) continue;

                using (var simpleVolume = session.SimpleAudioVolume)
                {
                    simpleVolume.Volume = volume;
                    break;
                }
            }
    }
}