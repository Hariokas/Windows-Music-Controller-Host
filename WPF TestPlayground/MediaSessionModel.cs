using System.ComponentModel;
using System.Runtime.CompilerServices;
using WPF___Online_Arduino_Music_Player;

namespace WPF_TestPlayground
{
    public class MediaSessionModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private SerialCommunicator serialCommunicator = SerialCommunicator.Instance;
        string previousOutput = string.Empty;
        public string Information
        {
            get
            {
                //return $"Session name: [{Id}]; ID: [{MediaSessionName}]; Artist: [{Artist}]; Song: [{SongName}]; Playback status: [{PlaybackStatus}]";
                NotifyArduino();
                return $"Status of [{SongName}] by [{Artist}] on [{MediaSessionName}], [{Id}] - [{PlaybackStatus}]";
            }
        }

        private int id;
        public int Id
        {
            get => id;
            set
            {
                id = value;
                OnPropertyChanged(nameof(Information));
            }
        }

        private string mediaSessionName = "";
        public string MediaSessionName
        {
            get => mediaSessionName;
            set
            {
                if (string.IsNullOrEmpty(value)) return;
                mediaSessionName = value;
                OnPropertyChanged(nameof(Information));
            }
        }

        private string artist = "";
        public string Artist
        {
            get => artist;
            set
            {
                if (string.IsNullOrEmpty(value)) return;
                artist = value;
                OnPropertyChanged(nameof(Information));
            }
        }

        private string songName = "";
        public string SongName
        {
            get => songName;
            set
            {
                if (string.IsNullOrEmpty(value)) return;
                songName = value;
                OnPropertyChanged(nameof(Information));
            }
        }

        private string playbackStatus = "";
        public string PlaybackStatus
        {
            get => playbackStatus;
            set
            {
                if (string.IsNullOrEmpty(value)) return;
                playbackStatus = value;
                OnPropertyChanged(nameof(Information));
            }
        }

        private void NotifyArduino()
        {
            if ((!string.IsNullOrEmpty(SongName) && !string.IsNullOrEmpty(Artist))
                || (!string.IsNullOrEmpty(MediaSessionName) && !string.IsNullOrEmpty(PlaybackStatus)))
            {
                serialCommunicator.UpdateArduino(this);
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}