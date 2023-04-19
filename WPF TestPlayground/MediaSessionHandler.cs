using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Windows.Media.Control;
using static WindowsMediaController.MediaManager;

namespace WPF_TestPlayground;

public class MediaSessionHandler
{
    private readonly ObservableCollection<MediaSessionModel> _mediaSessionModelList;

    public MediaSessionHandler(ObservableCollection<MediaSessionModel> mediaSessionModelList)
    {
        _mediaSessionModelList = mediaSessionModelList;
    }

    //public void GetCurrentSession
    //return mediaSession

    public void AddSession(MediaSession mediaSession)
    {
        if (mediaSession == null)
            return;

        Application.Current.Dispatcher.Invoke(() =>
        {
            _mediaSessionModelList.Add(new MediaSessionModel
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
            for (var i = 0; i < _mediaSessionModelList.Count; i++)
                if (_mediaSessionModelList[i].Id == mediaSession.GetHashCode())
                    _mediaSessionModelList.Remove(_mediaSessionModelList[i]);
        });
    }

    public void UpdatePlaybackState(MediaSession mediaSession,
        GlobalSystemMediaTransportControlsSessionPlaybackInfo args)
    {
        if (mediaSession == null || args == null)
            return;

        var _mediaSession = FindMediaSession(mediaSession);

        if (_mediaSession == null)
            return;

        _mediaSession.PlaybackStatus = args.PlaybackStatus.ToString();
    }

    public void UpdateMediaProperty(MediaSession mediaSession,
        GlobalSystemMediaTransportControlsSessionMediaProperties args)
    {
        if (mediaSession == null || args == null) return;

        var _mediaSession = FindMediaSession(mediaSession);

        if (_mediaSession == null)
            return;

        _mediaSession.Artist = args.Artist;
        _mediaSession.SongName = args.Title;
    }

    private MediaSessionModel? FindMediaSession(MediaSession mediaSession)
    {
        return _mediaSessionModelList?.Where(x => x.Id == mediaSession.GetHashCode()).FirstOrDefault() ?? null;
    }
}