using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Windows.Media.Control;
using Windows.Storage.Streams;
using WindowsMediaController;
using static WindowsMediaController.MediaManager;

namespace WPF_TestPlayground;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private static readonly MediaManager MediaManager = new();
    private static MediaSession? _currentSession;
    private static MediaSessionHandler _mediaSessionHandler;
    private static WebSocketCommunicator _communicator;

    private static List<MediaSessionModel> _mediaSessions;

    //private static readonly SerialCommunicator SerialCommunicator = SerialCommunicator.Instance;

    public MainWindow()
    {
        InitializeComponent();

        MediaManager.OnAnySessionOpened += MediaManager_OnAnySessionOpened;
        MediaManager.OnAnySessionClosed += MediaManager_OnAnySessionClosed;
        MediaManager.OnFocusedSessionChanged += MediaManager_OnFocusedSessionChanged;
        MediaManager.OnAnyPlaybackStateChanged += MediaManager_OnAnyPlaybackStateChanged;
        MediaManager.OnAnyMediaPropertyChanged += MediaManager_OnAnyMediaPropertyChanged;

        //SerialCommunicator.MessageReceived += SerialCommunicator_MessageReceived;

        SongList = new ObservableCollection<SongInfoModel>();
        MediaSessionModelList = new ObservableCollection<MediaSessionModel>();

        _mediaSessionHandler = new MediaSessionHandler(MediaSessionModelList);

        _mediaSessions = new List<MediaSessionModel>();

        _communicator = new WebSocketCommunicator("192.168.0.107", "8080");
        _communicator.CommandReceived += Communicator_CommandReceived;


        DataContext = this;
        MediaManager.Start();
    }

    public static ObservableCollection<SongInfoModel>? SongList { get; set; }
    public static ObservableCollection<MediaSessionModel>? MediaSessionModelList { get; set; }

    private static async void MediaManager_OnAnySessionOpened(MediaSession session)
    {
        Trace.WriteLine("MediaManager_OnAnySessionOpened called");
        if (session == null)
            return;

        _currentSession = session;

        WriteLineColor($"-- New Source: {session.Id}: {session.GetHashCode()}", ConsoleColor.Green);

        var newSessionEvent = new MediaSessionEvent
        {
            EventType = EventType.NewSession,
            MediaSessionId = session.GetHashCode(),
            MediaSessionName = session.Id
        };

        await _communicator.DistributeJsonAsync(newSessionEvent);

        _mediaSessionHandler.AddSession(session);
    }

    private static async void MediaManager_OnAnySessionClosed(MediaSession session)
    {
        Trace.WriteLine("MediaManager_OnAnySessionClosed called");

        if (session == null)
            return;

        WriteLineColor($"-- Removed Source: {session.Id}: {session.GetHashCode()}", ConsoleColor.Red);

        var closedSessionEvent = new MediaSessionEvent
        {
            EventType = EventType.CloseSession,
            MediaSessionId = session.GetHashCode(),
            MediaSessionName = session.Id
        };

        await _communicator.DistributeJsonAsync(closedSessionEvent);

        _mediaSessionHandler.RemoveMediaSession(session);
    }

    private static async void MediaManager_OnFocusedSessionChanged(MediaSession mediaSession)
    {
        Trace.WriteLine("MediaManager_OnFocusedSessionChanged called");

        if (mediaSession == null)
            return;

        _currentSession = mediaSession;

        WriteLineColor("== Session Focus Changed: " + mediaSession?.ControlSession?.SourceAppUserModelId,
            ConsoleColor.Gray);

        var focusedSessionChangedEvent = new MediaSessionEvent
        {
            EventType = EventType.SessionFocusChanged,
            MediaSessionId = mediaSession.GetHashCode(),
            MediaSessionName = mediaSession.Id
        };

        await _communicator.DistributeJsonAsync(focusedSessionChangedEvent);
    }

    private static async void MediaManager_OnAnyPlaybackStateChanged(MediaSession sender,
        GlobalSystemMediaTransportControlsSessionPlaybackInfo args)
    {
        Trace.WriteLine("MediaManager_OnAnyPlaybackStateChanged called");

        if (sender == null || args == null)
            return;
        try
        {
            WriteLineColor($"{sender.Id}: {sender.GetHashCode()} is now {args.PlaybackStatus}", ConsoleColor.Yellow);

            var sessionPlaybackStateChangedEvent = new MediaSessionEvent
            {
                EventType = EventType.PlaybackStatusChanged,
                MediaSessionId = sender.GetHashCode(),
                MediaSessionName = sender.Id,
                PlaybackStatus = args.PlaybackStatus.ToString()
            };

            var currentPlaybackPosition = MediaManager.GetPlaybackPosition();

            await _communicator.DistributeJsonAsync(sessionPlaybackStateChangedEvent);

            _mediaSessionHandler.UpdatePlaybackState(sender, args);
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Exception caught at MediaManager_OnAnyPlaybackStateChanged: {ex.Message}");
        }
    }

    private static async void MediaManager_OnAnyMediaPropertyChanged(MediaSession sender,
        GlobalSystemMediaTransportControlsSessionMediaProperties args)
    {
        Trace.WriteLine("MediaManager_OnAnyMediaPropertyChanged called");

        if (sender == null || args == null)
            return;
        try
        {
            WriteLineColor(
                $"{sender.Id} is now playing {args.Title} {(string.IsNullOrEmpty(args.Artist) ? "" : $"by {args.Artist}")}",
                ConsoleColor.Cyan);

            var sessionMediaPropertyChangedEvent = new MediaSessionEvent
            {
                EventType = EventType.SongChanged,
                MediaSessionId = sender.GetHashCode(),
                MediaSessionName = sender.Id,
                Artist = args.Artist,
                SongName = args.Title,
                PlaybackStatus = _currentSession?.ControlSession.GetPlaybackInfo()?.PlaybackStatus.ToString()
            };

            await _communicator.DistributeJsonAsync(sessionMediaPropertyChangedEvent);
            var songInfo = sender.ControlSession?.TryGetMediaPropertiesAsync()?.GetAwaiter().GetResult();
            BroadcastThumbnail(songInfo?.Thumbnail);
            BroadcastThumbnail(args.Thumbnail);

            _mediaSessionHandler.UpdateMediaProperty(sender, args);
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Exception caught at MediaManager_OnAnyMediaPropertyChanged: {ex.Message}");
        }
    }

    private static void BroadcastThumbnail(IRandomAccessStreamReference thumbnail)
    {
        if (thumbnail == null) return;

        var thumbnailAsByteArray = NotHelper.GetThumbnailAsByteArray(thumbnail);

        if (thumbnailAsByteArray != null)
            _ = _communicator.DistributeImageAsync(thumbnailAsByteArray);
    }

    public static void WriteLineColor(object toPrint, ConsoleColor color = ConsoleColor.White)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            SongList?.Add(new SongInfoModel { FirstLine = $"{DateTime.Now:HH:mm:ss.fff} {toPrint}" });
        });
    }

    private async void Communicator_CommandReceived(object sender, MediaSessionEventArgs e)
    {
        if (_currentSession == null)
        {
            Trace.WriteLine("Current session is null!");
            return;
        }

        try
        {
            switch (e.MediaSessionEvent.EventType)
            {
                case EventType.Play:
                case EventType.Pause:

                    var controlsInfo = _currentSession?.ControlSession.GetPlaybackInfo()?.Controls;

                    if (controlsInfo?.IsPauseEnabled == true)
                        await _currentSession?.ControlSession?.TryPauseAsync();
                    else if (controlsInfo?.IsPlayEnabled == true)
                        await _currentSession?.ControlSession?.TryPlayAsync();

                    break;

                case EventType.Previous:
                    await _currentSession?.ControlSession?.TrySkipPreviousAsync();
                    break;

                case EventType.Next:
                    await _currentSession?.ControlSession?.TrySkipNextAsync();
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
}

public class SongInfoModel
{
    public string FirstLine { get; set; } = "";
    public string SecondLine { get; set; } = "";
}

internal static class NotHelper
{
    public static byte[]? GetThumbnailAsByteArray(IRandomAccessStreamReference thumbnail)
    {
        if (thumbnail == null)
            return null;

        var imageStream = thumbnail.OpenReadAsync().GetAwaiter().GetResult();
        var fileBytes = new byte[imageStream.Size];
        using (var reader = new DataReader(imageStream))
        {
            reader.LoadAsync((uint)imageStream.Size).GetAwaiter().GetResult();
            reader.ReadBytes(fileBytes);
        }

        return fileBytes;
    }

    internal static BitmapImage? GetThumbnail(IRandomAccessStreamReference Thumbnail)
    {
        if (Thumbnail == null)
            return null;

        var imageStream = Thumbnail.OpenReadAsync().GetAwaiter().GetResult();
        var fileBytes = new byte[imageStream.Size];
        using (var reader = new DataReader(imageStream))
        {
            reader.LoadAsync((uint)imageStream.Size).GetAwaiter().GetResult();
            reader.ReadBytes(fileBytes);
        }

        var image = new BitmapImage();
        using (var ms = new MemoryStream(fileBytes))
        {
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = ms;
            image.EndInit();
        }

        return image;
    }
}

#region Methods from previous project, just to see as examples of possible implementations

//private void MediaManager_OnAnySessionOpened(MediaSession mediaSession)
//{
//    Application.Current.Dispatcher.Invoke(() =>
//    {
//        var menuItem = new NavigationViewItem
//        {
//            Content = mediaSession.Id,
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
//            if (((MediaSession?)item?.Tag)?.ToString() == session.Id)
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
//                //item.Content = (((MediaSession?)item?.Tag)?.Id == session?.Id ? "# " : "") + ((MediaSession?)item?.Tag)?.Id;
//                item.Content = ((MediaSession?)item?.Tag)?.Id;
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