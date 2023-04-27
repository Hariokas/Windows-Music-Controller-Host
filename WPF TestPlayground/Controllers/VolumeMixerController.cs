using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Radios;
using NAudio.CoreAudioApi;
using NAudio;
using System.Collections.Generic;
using System.Data;
using WPF_TestPlayground.Models;

namespace WPF_TestPlayground.Controllers
{
    public class VolumeMixerController
    {
        public List<ApplicationVolume> GetApplicationVolumes()
        {
            var appVolumes = new List<ApplicationVolume>();

            var deviceEnumerator = new MMDeviceEnumerator();
            var device = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

            using var sessionManager = device.AudioEndpointVolume;
            var sessions = device.AudioSessionManager.Sessions;

            for (int i = 0; i < sessions.Count; i++)
            {
                using (var session = sessions[i])
                {
                    var control = session;
                    using (var simpleVolume = session.SimpleAudioVolume)
                    {
                        appVolumes.Add(new ApplicationVolume
                        {
                            ProcessID = (int)control.GetProcessID,
                            DisplayName = control.DisplayName,
                            Volume = simpleVolume.Volume
                        });
                    }
                }
            }

            return appVolumes;
        }

        public void SetApplicationVolume(int processId, float volume)
        {
            if (volume < 0 || volume > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(volume), "Volume must be in the range of 0 to 1.");
            }

            var deviceEnumerator = new MMDeviceEnumerator();
            var device = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

            using var sessionManager = device.AudioEndpointVolume;
            var sessions = device.AudioSessionManager.Sessions;

            for (int i = 0; i < sessions.Count; i++)
            {
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
    }
}
