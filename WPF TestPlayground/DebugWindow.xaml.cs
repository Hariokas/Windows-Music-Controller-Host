using System.Collections.Generic;
using System.Windows;
using Windows.Media.Control;
using Media_Controller_Remote_Host.Controllers;
using static Media_Controller_Remote_Host.Controllers.MediaManager;

namespace Media_Controller_Remote_Host;

/// <summary>
///     Interaction logic for DebugWindow.xaml
/// </summary>
public partial class DebugWindow : Window
{
    private static readonly MediaManager mediaManager = new();
    private static MediaManager.MediaSession? currentSession = null;

    public DebugWindow()
    {
        DataContext = this;
        InitializeComponent();

        mediaManager.OnAnySessionOpened += MediaManager_OnAnySessionOpened;
        mediaManager.OnAnySessionClosed += MediaManager_OnAnySessionClosed;
        mediaManager.OnFocusedSessionChanged += MediaManager_OnFocusedSessionChanged;

        mediaManager.Start();
    }

    public List<MediaManager.MediaSession> mediaSessionList { get; set; } = new();

    private void MediaManager_OnFocusedSessionChanged(MediaManager.MediaSession mediaSession)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            //Change the focused session.
        });
    }

    private void MediaManager_OnAnySessionClosed(MediaManager.MediaSession mediaSession)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            foreach (var session in mediaSessionList)
                if (session.Id == mediaSession.Id)
                    mediaSessionList.Remove(session);
        });
    }

    private void MediaManager_OnAnySessionOpened(MediaManager.MediaSession mediaSession)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var session = mediaSession;

            mediaSessionList.Add(session);
        });
    }

    private void CurrentSession_OnPlaybackStateChanged(MediaManager.MediaSession mediaSession,
        GlobalSystemMediaTransportControlsSessionPlaybackInfo? playbackInfo = null)
    {
        Application.Current.Dispatcher.Invoke(() => { UpdateUI(mediaSession); });
    }

    private void CurrentSession_OnMediaPropertyChanged(MediaManager.MediaSession mediaSession,
        GlobalSystemMediaTransportControlsSessionMediaProperties mediaProperties)
    {
        Application.Current.Dispatcher.Invoke(() => { UpdateUI(mediaSession); });
    }

    private void UpdateUI(MediaManager.MediaSession mediaSession)
    {
    }
}