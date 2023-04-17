using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Windows.Media.Control;
using WindowsMediaController;
using static WindowsMediaController.MediaManager;

namespace WPF_TestPlayground
{

    public class MediaSessionHandler
    {
        private ObservableCollection<MediaSessionModel> MediaSessionModelList;

        public MediaSessionHandler(ObservableCollection<MediaSessionModel> MediaSessionModelList)
        {
            this.MediaSessionModelList = MediaSessionModelList;
        }

        public void AddSession(MediaSession mediaSession)
        {
            if (mediaSession == null)
                return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                MediaSessionModelList.Add(new MediaSessionModel
                {
                    Id = mediaSession.GetHashCode(),
                    MediaSessionName = mediaSession.Id
                });
            });

        }

        public void RemoveMediaSession(MediaSession mediaSession)
        {
            if (mediaSession == null)
                return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                for (int i = 0; i < MediaSessionModelList.Count; i++)
                {
                    if (MediaSessionModelList[i].Id == mediaSession.GetHashCode())
                    {
                        MediaSessionModelList.Remove(MediaSessionModelList[i]);
                    }
                }
            });

        }

        public void UpdatePlaybackState(MediaSession mediaSession, GlobalSystemMediaTransportControlsSessionPlaybackInfo args)
        {
            if (mediaSession == null || args == null)
                return;

            var _mediaSession = FindMediaSession(mediaSession);

            if (_mediaSession == null)
                return;

            _mediaSession.PlaybackStatus = args.PlaybackStatus.ToString();

        }

        public void UpdateMediaProperty(MediaSession mediaSession, GlobalSystemMediaTransportControlsSessionMediaProperties args)
        {
            if (mediaSession == null || args == null) return;

            var _mediaSession = FindMediaSession(mediaSession);

            if (_mediaSession == null)
                return;

            _mediaSession.Artist = args.Artist;
            _mediaSession.SongName = args.Title;

        }

        private MediaSessionModel? FindMediaSession(MediaSession _mediaSession)
        {
            return MediaSessionModelList.Where(x => x.Id == _mediaSession.GetHashCode()).FirstOrDefault() ?? null;
        }

    }
}