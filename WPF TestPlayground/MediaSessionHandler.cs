using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.Media.Control;
using WindowsMediaController;
using static WindowsMediaController.MediaManager;

namespace WPF_TestPlayground;

public class MediaSessionHandler
{
    private readonly MediaManager MediaManager;
    private static MediaSession? _currentMediaSession;

    private int _currentSessionId;

    public MediaSessionHandler(MediaManager mediaManager)
    {
        MediaManager = mediaManager;
        MediaSessionInfos = new Dictionary<int, MediaInfo>();

        MediaManager.OnAnySessionOpened += MediaManager_OnAnySessionOpened;
        MediaManager.OnAnySessionClosed += MediaManager_OnAnySessionClosed;
        MediaManager.OnFocusedSessionChanged += MediaManager_OnFocusedSessionChanged;
        MediaManager.OnAnyPlaybackStateChanged += MediaManager_OnAnyPlaybackStateChanged;
        MediaManager.OnAnyMediaPropertyChanged += MediaManager_OnAnyMediaPropertyChanged;
    }

    public Dictionary<int, MediaInfo> MediaSessionInfos { get; }
    public event EventHandler<MediaSessionEventArgs> MediaPropertiesChanged;
    public event EventHandler<MediaSessionEventArgs> PlaybackStateChanged;
    public event EventHandler<MediaSessionEventArgs> FocusedSessionChanged;
    public event EventHandler<MediaSessionEventArgs> MediaSessionClosed;
    public event EventHandler<MediaSessionEventArgs> MediaSessionOpened;

    private async void MediaManager_OnAnySessionOpened(MediaSession session)
    {
        Trace.WriteLine("MediaManager_OnAnySessionOpened called");

        if (session == null)
            return;

        try
        {
            var mediaInfo = new MediaInfo
            {
                MediaSessionId = session.GetHashCode(),
                MediaSessionName = session.Id
            };

            _currentSessionId = session.GetHashCode();
            _currentMediaSession = session;
            MediaSessionInfos[_currentSessionId] = mediaInfo;

            MediaSessionOpened?.Invoke(this, new MediaSessionEventArgs(new MediaSessionEvent
            {
                EventType = EventType.NewSession,
                MediaSessionId = _currentSessionId
            }));
        }
        catch (Exception ex)
        {
            Trace.Write($"Exception caught at MediaManager_OnAnySessionOpened: {ex.Message}");
        }
    }

    private async void MediaManager_OnAnySessionClosed(MediaSession session)
    {
        Trace.WriteLine("MediaManager_OnAnySessionClosed called");

        try
        {
            var mediaSession = GetMediaInfo(session.GetHashCode());

            if (mediaSession == null)
            {
                Trace.WriteLine($"MediaSession not found in MediaSessionInfos.");
            }

            MediaSessionInfos.Remove(mediaSession.GetHashCode());

            MediaSessionClosed?.Invoke(this, new MediaSessionEventArgs(new MediaSessionEvent
            {
                EventType = EventType.CloseSession,
                MediaSessionId = mediaSession.MediaSessionId,
                Artist = mediaSession.Artist,
                MediaSessionName = mediaSession.MediaSessionName,
                PlaybackStatus = mediaSession.PlaybackStatus,
                SongName = mediaSession.SongName
            }));
        }
        catch (Exception ex)
        {
            Trace.Write($"Exception caught at MediaManager_OnAnySessionClosed: {ex.Message}");
        }
    }

    private async void MediaManager_OnFocusedSessionChanged(MediaSession session)
    {
        Trace.WriteLine("MediaManager_OnFocusedSessionChanged called");

        try
        {
            if (session == null) return;

            var mediaSession = GetMediaInfo(session.GetHashCode());

            if (mediaSession == null)
            {
                Trace.WriteLine($"MediaSession not found in MediaSessionInfos.");
            }

            _currentSessionId = mediaSession.MediaSessionId;
            _currentMediaSession = session;

            FocusedSessionChanged?.Invoke(this, new MediaSessionEventArgs(new MediaSessionEvent
            {
                EventType = EventType.SessionFocusChanged,
                MediaSessionId = mediaSession.MediaSessionId,
                Artist = mediaSession.Artist,
                MediaSessionName = mediaSession.MediaSessionName,
                PlaybackStatus = mediaSession.PlaybackStatus,
                SongName = mediaSession.SongName
            }));

        }
        catch (Exception ex)
        {
            Trace.Write($"Exception caught at MediaManager_OnFocusedSessionChanged: {ex.Message}");
        }
    }

    private async void MediaManager_OnAnyPlaybackStateChanged(MediaSession session,
        GlobalSystemMediaTransportControlsSessionPlaybackInfo args)
    {
        Trace.WriteLine("MediaManager_OnAnyPlaybackStateChanged called");

        try
        {
            var mediaSession = GetMediaInfo(session.GetHashCode());

            if (mediaSession == null)
            {
                Trace.WriteLine($"MediaSession not found in MediaSessionInfos.");
            }

            mediaSession.PlaybackStatus = args.PlaybackStatus == null ? "Playing" : args.PlaybackStatus.ToString();
            MediaSessionInfos[mediaSession.MediaSessionId] = mediaSession;

            var currentPlaybackPosition = MediaManager.GetPlaybackPosition();

            PlaybackStateChanged?.Invoke(this, new MediaSessionEventArgs(new MediaSessionEvent
            {
                EventType = EventType.PlaybackStatusChanged,
                MediaSessionId = mediaSession.MediaSessionId,
                Artist = mediaSession.Artist,
                MediaSessionName = mediaSession.MediaSessionName,
                PlaybackStatus = mediaSession.PlaybackStatus,
                SongName = mediaSession.SongName
            }));
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Exception caught at MediaManager_OnAnyPlaybackStateChanged: {ex.Message}");
        }
    }

    private async void MediaManager_OnAnyMediaPropertyChanged(MediaSession session,
        GlobalSystemMediaTransportControlsSessionMediaProperties args)
    {
        Trace.WriteLine("MediaManager_OnAnyMediaPropertyChanged called");

        try
        {
            var mediaSession = GetMediaInfo(session.GetHashCode());

            if (mediaSession == null)
            {
                Trace.WriteLine($"MediaSession not found in MediaSessionInfos.");
            }

            mediaSession.MediaSessionName = session.Id;
            mediaSession.Artist = args.Artist;
            mediaSession.SongName = args.Title;
            mediaSession.PlaybackStatus = session?.ControlSession?.GetPlaybackInfo()?.PlaybackStatus.ToString();

            MediaSessionInfos[mediaSession.MediaSessionId] = mediaSession;

            MediaPropertiesChanged?.Invoke(this, new MediaSessionEventArgs(new MediaSessionEvent
            {
                EventType = EventType.SongChanged,
                MediaSessionId = mediaSession.MediaSessionId,
                Artist = mediaSession.Artist,
                MediaSessionName = mediaSession.MediaSessionName,
                PlaybackStatus = mediaSession.PlaybackStatus,
                SongName = mediaSession.SongName
            }));

            //var songInfo = sender.ControlSession?.TryGetMediaPropertiesAsync()?.GetAwaiter().GetResult();

            //BroadcastThumbnail(songInfo?.Thumbnail);
            //BroadcastThumbnail(args.Thumbnail);
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Exception caught at MediaManager_OnAnyMediaPropertyChanged: {ex.Message}");
        }
    }

    public MediaInfo? GetMediaInfo(int mediaSessionId)
    {
        MediaSessionInfos.TryGetValue(_currentSessionId, out var mediaInfo);
        return mediaInfo;
    }

    public MediaInfo? GetCurrentMediaInfo()
    {
        MediaSessionInfos.TryGetValue(_currentSessionId, out var mediaInfo);
        return mediaInfo;
    }

    public static MediaSession GetCurrentMediaSession()
    {
        return _currentMediaSession;
    }
}