using System;
using System.Collections.ObjectModel;
using System.Windows;
using Windows.Storage.Streams;
using Media_Controller_Remote_Host.Controllers;
using Media_Controller_Remote_Host.EventClasses;
using Media_Controller_Remote_Host.Handlers;
using Media_Controller_Remote_Host.Models;
using static Media_Controller_Remote_Host.Controllers.MediaManager;

namespace Media_Controller_Remote_Host;

public partial class MainWindow : Window
{
    private static readonly MediaManager MediaManager = new();
    private static SocketController _communicator;

    private static MediaSessionHandler _mediaSessionHandler;
    private static MasterVolumeHandler _masterVolumeHandler;
    private static VolumeMixerHandler _volumeMixerHandler;

    public static ObservableCollection<SongInfoModel>? SongList { get; set; }
    public static ObservableCollection<MediaInfo>? MediaSessionModelList { get; set; }

    public MainWindow()
    {
        InitializeComponent();

        SongList = new ObservableCollection<SongInfoModel>();
        MediaSessionModelList = new ObservableCollection<MediaInfo>();

        _communicator = new SocketController("192.168.0.107", "8080");

        _mediaSessionHandler = new MediaSessionHandler(MediaManager);
        _masterVolumeHandler = new MasterVolumeHandler();
        _volumeMixerHandler = new VolumeMixerHandler();

        _mediaSessionHandler.MediaPropertiesChanged += MediaSessionHandler_MediaPropertiesChanged;
        _mediaSessionHandler.PlaybackStateChanged += MediaSessionHandler_PlaybackStateChanged;
        _mediaSessionHandler.FocusedSessionChanged += MediaSessionHandler_FocusedSessionChanged;
        _mediaSessionHandler.MediaSessionClosed += MediaSessionHandler_MediaSessionClosed;
        _mediaSessionHandler.MediaSessionOpened += MediaSessionHandler_MediaSessionOpened;
        _mediaSessionHandler.ThumbnailChanged += MediaSessionHandler_ThumbnailChanged;

        _communicator.MasterVolumeCommandReceived += Communicator_MasterVolumeCommandReceived;
        _communicator.VolumeMixerCommandReceived += Communicator_VolumeMixerCommandReceived;
        _communicator.MediaSessionCommandReceived += Communicator_MediaSessionCommandReceived;

        _masterVolumeHandler.SendMessageRequested += MasterVolumeHandler_SendMessageRequested;
        _volumeMixerHandler.SendMessageRequested += VolumeMixerHandler_SendMessageRequested;

        DataContext = this;
        MediaManager.Start();
    }

    private async void VolumeMixerHandler_SendMessageRequested(object? sender, VolumeMixerEventArgs e)
    {
        await _communicator.DistributeJsonAsync(e.VolumeMixerEvent);
    }

    private async void MasterVolumeHandler_SendMessageRequested(object sender, MasterVolumeEventArgs e)
    {
        await _communicator.DistributeJsonAsync(e.MasterVolumeEvent);
    }

    private async void Communicator_MediaSessionCommandReceived(object? sender, MediaSessionEventArgs e)
    {
         _mediaSessionHandler.MediaSessionCommandReceived(sender, e);
    }

    private void Communicator_VolumeMixerCommandReceived(object? sender, VolumeMixerEventArgs e)
    {
        _volumeMixerHandler.VolumeMixerCommandReceived(sender, e);
    }

    private void Communicator_MasterVolumeCommandReceived(object? sender, MasterVolumeEventArgs e)
    {
        _masterVolumeHandler.MasterVolumeCommandReceived(sender, e);
    }

    private async void MediaSessionHandler_ThumbnailChanged(object? sender, MediaSessionHandler.ThumbnailEventArgs e)
    {
        _ = _communicator.DistributeImageAsync(e.ThumbnailData);
    }

    private async void MediaSessionHandler_MediaSessionOpened(object? sender, MediaSessionEventArgs e)
    {
        await _communicator.DistributeJsonAsync(e.MediaSessionEvent);
        WriteLineColor($"New session: [{e.MediaSessionEvent.MediaSessionId}] - [{e.MediaSessionEvent.MediaSessionName}]");
    }

    private async void MediaSessionHandler_MediaSessionClosed(object? sender, MediaSessionEventArgs e)
    {
        await _communicator.DistributeJsonAsync(e.MediaSessionEvent);
        WriteLineColor($"Session closed: [{e.MediaSessionEvent.MediaSessionId}] - [{e.MediaSessionEvent.MediaSessionName}]");
    }

    private async void MediaSessionHandler_FocusedSessionChanged(object? sender, MediaSessionEventArgs e)
    {
        await _communicator.DistributeJsonAsync(e.MediaSessionEvent);
        WriteLineColor($"Focused session changed to: [{e.MediaSessionEvent.MediaSessionId}] - [{e.MediaSessionEvent.MediaSessionName}]");
    }

    private async void MediaSessionHandler_MediaPropertiesChanged(object sender, MediaSessionEventArgs e)
    {
        await _communicator.DistributeJsonAsync(e.MediaSessionEvent);
        WriteLineColor($"Media properties changed for: [{e.MediaSessionEvent.MediaSessionId}] - [{e.MediaSessionEvent.MediaSessionName}]");
        WriteLineColor($"Properties: [{e.MediaSessionEvent.Artist}] - [{e.MediaSessionEvent.SongName}] [{e.MediaSessionEvent.PlaybackStatus}]");
    }

    private async void MediaSessionHandler_PlaybackStateChanged(object sender, MediaSessionEventArgs e)
    {
        await _communicator.DistributeJsonAsync(e.MediaSessionEvent);
        WriteLineColor($"Playback status changed for: [{e.MediaSessionEvent.MediaSessionId}] - [{e.MediaSessionEvent.MediaSessionName}]; Playback status: [{e.MediaSessionEvent.PlaybackStatus}] [{e.MediaSessionEvent.Artist}] - [{e.MediaSessionEvent.SongName}]");
    }



    public static void WriteLineColor(object toPrint, ConsoleColor color = ConsoleColor.White)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            SongList?.Add(new SongInfoModel { FirstLine = $"{DateTime.Now:HH:mm:ss.fff} {toPrint}" });
        });
    }
}

public class SongInfoModel
{
    public string FirstLine { get; set; } = "";
    public string SecondLine { get; set; } = "";
}

#region Methods from previous project, just to see as examples of possible implementations

//private void MediaManager_OnAnySessionOpened(MediaSession mediaSession)
//{
//    Application.Current.Dispatcher.Invoke(() =>
//    {
//        var menuItem = new NavigationViewItem
//        {
//            Content = mediaSession.MediaSessionId,
//            Icon = new SymbolIcon() { Symbol = Symbol.Audio },
//            Tag = mediaSession
//        };
//        SongList.MenuItems.Add(menuItem);
//    });
//}

//private void MediaManager_OnAnySessionClosed(MediaSession session)
//{
//    Application.Current.Dispatcher.Invoke(() =>
//    {
//        NavigationViewItem? itemToRemove = null;

//        foreach (NavigationViewItem? item in SongList.MenuItems)
//            if (((MediaSession?)item?.Tag)?.ToString() == session.MediaSessionId)
//                itemToRemove = item;

//        if (itemToRemove != null)
//            SongList.MenuItems.Remove(itemToRemove);
//    });
//}

//private void MediaManager_OnFocusedSessionChanged(MediaSession session)
//{
//    Application.Current.Dispatcher.Invoke(() =>
//    {
//        foreach (NavigationViewItem? item in SongList.MenuItems)
//        {
//            if (item != null)
//            {
//                //item.Content = (((MediaSession?)item?.Tag)?.MediaSessionId == session?.MediaSessionId ? "# " : "") + ((MediaSession?)item?.Tag)?.MediaSessionId;
//                item.Content = ((MediaSession?)item?.Tag)?.MediaSessionId;
//            }
//        }
//    });
//}

//private void SongList_SelectionChanged(NavigationView navView, NavigationViewSelectionChangedEventArgs args)
//{
//    if (currentSession != null)
//    {
//        currentSession.OnMediaPropertyChanged -= CurrentSession_OnMediaPropertyChanged;
//        currentSession.OnPlaybackStateChanged -= CurrentSession_OnPlaybackStateChanged;
//        currentSession = null;
//    }

//    if (navView.SelectedItem != null)
//    {
//        currentSession = (MediaSession)((NavigationViewItem)navView.SelectedItem).Tag;
//        currentSession.OnMediaPropertyChanged += CurrentSession_OnMediaPropertyChanged;
//        currentSession.OnPlaybackStateChanged += CurrentSession_OnPlaybackStateChanged;
//        CurrentSession_OnPlaybackStateChanged(currentSession);
//    }
//    else
//    {
//        SongImage.Source = null;
//        SongTitle.Content = "TITLE";
//        SongAuthor.Content = "Author";
//        ControlPlayPause.Content = "▶️";
//    }
//}

//private void CurrentSession_OnPlaybackStateChanged(MediaSession mediaSession, GlobalSystemMediaTransportControlsSessionPlaybackInfo? playbackInfo = null)
//{
//    Application.Current.Dispatcher.Invoke(() =>
//    {
//        UpdateUI(mediaSession);
//    });
//}

//private void CurrentSession_OnMediaPropertyChanged(MediaSession mediaSession, GlobalSystemMediaTransportControlsSessionMediaProperties mediaProperties)
//{
//    Application.Current.Dispatcher.Invoke(() =>
//    {
//        UpdateUI(mediaSession);
//    });
//}

//private void UpdateUI(MediaSession mediaSession)
//{
//    var mediaProp = mediaSession.ControlSession.GetPlaybackInfo();
//    if (mediaProp != null)
//    {
//        if (mediaSession.ControlSession.GetPlaybackInfo().Controls.IsPauseEnabled)
//            ControlPlayPause.Content = "II";
//        else
//            ControlPlayPause.Content = "▶️";
//        ControlBack.IsEnabled = ControlForward.IsEnabled = mediaProp.Controls.IsNextEnabled;
//    }

//    var songInfo = mediaSession.ControlSession.TryGetMediaPropertiesAsync().GetAwaiter().GetResult();
//    if (songInfo != null)
//    {
//        SongTitle.Content = songInfo.Title.ToUpper();
//        SongAuthor.Content = songInfo.Artist;
//        SongImage.Source = Helper.GetThumbnail(songInfo.Thumbnail);
//    }

//}

//private async void Back_Click(object sender, RoutedEventArgs e)
//{
//    await currentSession?.ControlSession.TrySkipPreviousAsync();
//}

//private async void PlayPause_Click(object sender, RoutedEventArgs e)
//{
//    var controlsInfo = currentSession?.ControlSession.GetPlaybackInfo().Controls;

//    if (controlsInfo?.IsPauseEnabled == true)
//        await currentSession?.ControlSession.TryPauseAsync();
//    else if (controlsInfo?.IsPlayEnabled == true)
//        await currentSession?.ControlSession.TryPlayAsync();
//}

//private async void Forward_Click(object sender, RoutedEventArgs e)
//{
//    await currentSession?.ControlSession.TrySkipNextAsync();
//}

#endregion