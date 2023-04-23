using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.Media.Control;
using WPF_TestPlayground.Controllers;
using WPF_TestPlayground.EventClasses;
using WPF_TestPlayground.Models;
using static WPF_TestPlayground.Controllers.MediaManager;

namespace WPF_TestPlayground.Handlers;

public class MediaSessionHandler
{
    private readonly MediaManager MediaManager;
    private static MediaSession? _currentMediaSession;

    private static int _currentSessionId;

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

    public static Dictionary<int, MediaInfo> MediaSessionInfos { get; private set; }
    public event EventHandler<MediaSessionEventArgs> MediaPropertiesChanged;
    public event EventHandler<MediaSessionEventArgs> PlaybackStateChanged;
    public event EventHandler<MediaSessionEventArgs> FocusedSessionChanged;
    public event EventHandler<MediaSessionEventArgs> MediaSessionClosed;
    public event EventHandler<MediaSessionEventArgs> MediaSessionOpened;
    public event EventHandler<ThumbnailEventArgs>? ThumbnailChanged;

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
                MediaSessionEventType = MediaSessionEventType.NewSession,
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
                MediaSessionEventType = MediaSessionEventType.CloseSession,
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

            var timelineProperties = MediaManager.GetTimelineProperties();
            var timestamp = DateTime.UtcNow;

            mediaSession.CurrentPlaybackTime = timelineProperties?.Position ?? TimeSpan.MaxValue;
            mediaSession.SongLength = timelineProperties?.EndTime ?? TimeSpan.MaxValue;

            FocusedSessionChanged?.Invoke(this, new MediaSessionEventArgs(new MediaSessionEvent
            {
                MediaSessionEventType = MediaSessionEventType.SessionFocusChanged,
                MediaSessionId = mediaSession.MediaSessionId,
                CurrentPlaybackTime = mediaSession.CurrentPlaybackTime,
                SongLength = mediaSession.SongLength,
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

            var timelineProperties = MediaManager.GetTimelineProperties();
            var timestamp = DateTime.UtcNow;

            mediaSession.PlaybackStatus = args.PlaybackStatus == null ? "Playing" : args.PlaybackStatus.ToString();
            mediaSession.CurrentPlaybackTime = timelineProperties?.Position ?? TimeSpan.MaxValue;
            mediaSession.SongLength = timelineProperties?.EndTime ?? TimeSpan.MaxValue;

            MediaSessionInfos[mediaSession.MediaSessionId] = mediaSession;
            
            PlaybackStateChanged?.Invoke(this, new MediaSessionEventArgs(new MediaSessionEvent
            {
                MediaSessionEventType = MediaSessionEventType.PlaybackStatusChanged,
                CurrentPlaybackTime = mediaSession.CurrentPlaybackTime,
                Timestamp = timestamp,
                SongLength = mediaSession.SongLength,
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

            var timelineProperties = MediaManager.GetTimelineProperties();
            var timestamp = DateTime.UtcNow;
            
            mediaSession.CurrentPlaybackTime = timelineProperties?.Position ?? TimeSpan.MaxValue;
            mediaSession.SongLength = timelineProperties?.EndTime ?? TimeSpan.MaxValue;
            mediaSession.MediaSessionName = session.Id;
            mediaSession.Artist = args.Artist;
            mediaSession.SongName = args.Title;
            mediaSession.PlaybackStatus = session?.ControlSession?.GetPlaybackInfo()?.PlaybackStatus.ToString();


            MediaSessionInfos[mediaSession.MediaSessionId] = mediaSession;

            MediaPropertiesChanged?.Invoke(this, new MediaSessionEventArgs(new MediaSessionEvent
            {
                MediaSessionEventType = MediaSessionEventType.SongChanged,
                CurrentPlaybackTime = mediaSession.CurrentPlaybackTime,
                Timestamp = timestamp,
                SongLength = mediaSession.SongLength,
                MediaSessionId = mediaSession.MediaSessionId,
                Artist = mediaSession.Artist,
                MediaSessionName = mediaSession.MediaSessionName,
                PlaybackStatus = mediaSession.PlaybackStatus,
                SongName = mediaSession.SongName
            }));

            //var songInfo = sender.ControlSession?.TryGetMediaPropertiesAsync()?.GetAwaiter().GetResult();

            var thumbnail = args.Thumbnail;
            var thumbnailData = ImageProcessor.GetThumbnailAsByteArray(thumbnail, 4);
            if (thumbnailData != null) OnThumbnailChanged(thumbnailData);
            //BroadcastThumbnail(songInfo?.Thumbnail);
            //BroadcastThumbnail(args.Thumbnail);
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Exception caught at MediaManager_OnAnyMediaPropertyChanged: {ex.Message}");
        }
    }

    public async void MediaSessionCommandReceived(object sender, MediaSessionEventArgs e)
    {
        var currentMediaSession = MediaSessionHandler.GetCurrentMediaSession();

        if (currentMediaSession == null)
        {
            Trace.WriteLine("Current session is null!");
            return;
        }

        try
        {
            switch (e.MediaSessionEvent.MediaSessionEventType)
            {
                case MediaSessionEventType.Play:
                case MediaSessionEventType.Pause:

                    var currentSession = MediaSessionHandler.GetCurrentMediaSession();
                    var controlsInfo = currentSession?.ControlSession.GetPlaybackInfo()?.Controls;

                    if (controlsInfo?.IsPauseEnabled == true)
                        await currentMediaSession?.ControlSession?.TryPauseAsync();
                    else if (controlsInfo?.IsPlayEnabled == true)
                        await currentMediaSession?.ControlSession?.TryPlayAsync();

                    break;

                case MediaSessionEventType.Previous:
                    await currentMediaSession?.ControlSession?.TrySkipPreviousAsync();
                    break;

                case MediaSessionEventType.Next:
                    await currentMediaSession?.ControlSession?.TrySkipNextAsync();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Error: {ex}");
        }
    }

    private async void OnThumbnailChanged(byte[] thumbnailData)
    {
        ThumbnailChanged?.Invoke(this, new ThumbnailEventArgs(thumbnailData));
    }

    public MediaInfo? GetMediaInfo(int mediaSessionId)
    {
        MediaSessionInfos.TryGetValue(_currentSessionId, out var mediaInfo);
        return mediaInfo;
    }

    public static MediaInfo? GetCurrentMediaInfo()
    {
        MediaSessionInfos.TryGetValue(_currentSessionId, out var mediaInfo);
        return mediaInfo;
    }

    public static MediaSession GetCurrentMediaSession()
    {
        return _currentMediaSession;
    }

    public class ThumbnailEventArgs : EventArgs
    {
        public byte[] ThumbnailData { get; }

        public ThumbnailEventArgs(byte[] thumbnailData)
        {
            ThumbnailData = thumbnailData;
        }
    }
}