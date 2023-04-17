using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WPF_TestPlayground
{
    public class MediaSessionModelTwo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string Information { get { return $"Session name: [{Id}]; ID: [{MediaSessionName}]; Artist: [{Artist}]; Song: [{SongName}]; Playback status: [{PlaybackStatus}]"; } }
        
        private int id;
        public int Id
        {
            get => id;
            set
            {
                id = value;
                OnPropertyChanged();
            }
        }

        private string mediaSessionName = "";
        public string MediaSessionName
        {
            get => mediaSessionName;
            set
            {
                mediaSessionName = value;
                OnPropertyChanged();
            }
        }

        private string artist = "";
        public string Artist
        {
            get => artist;
            set
            {
                artist = value;
                OnPropertyChanged();
            }
        }

        private string songName = "";
        public string SongName
        {
            get => songName;
            set
            {
                songName = value;
                OnPropertyChanged();
            }
        }

        private string playbackStatus = "";
        public string PlaybackStatus
        {
            get => playbackStatus;
            set
            {
                playbackStatus = value;
                OnPropertyChanged();
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
