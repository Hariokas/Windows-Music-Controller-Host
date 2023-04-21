using System;
using System.Collections.ObjectModel;
using System.Windows;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using WindowsMediaController;
using static WindowsMediaController.MediaManager;

namespace WPF_TestPlayground;

public partial class MainWindow : Window
{
    private static readonly MediaManager MediaManager = new();
    private static SocketController _communicator;
    private static MediaSessionHandler _mediaSessionHandler;

    public static ObservableCollection<SongInfoModel>? SongList { get; set; }
    public static ObservableCollection<MediaInfo>? MediaSessionModelList { get; set; }

    public MainWindow()
    {
        InitializeComponent();

        SongList = new ObservableCollection<SongInfoModel>();
        MediaSessionModelList = new ObservableCollection<MediaInfo>();

        _communicator = new SocketController("192.168.0.107", "8080", MediaManager);

        _mediaSessionHandler = new MediaSessionHandler(MediaManager);

        _mediaSessionHandler.MediaPropertiesChanged += MediaSessionHandler_MediaPropertiesChanged;
        _mediaSessionHandler.PlaybackStateChanged += MediaSessionHandler_PlaybackStateChanged;
        _mediaSessionHandler.FocusedSessionChanged += MediaSessionHandler_FocusedSessionChanged;
        _mediaSessionHandler.MediaSessionClosed += MediaSessionHandler_MediaSessionClosed;
        _mediaSessionHandler.MediaSessionOpened += MediaSessionHandler_MediaSessionOpened;

        DataContext = this;
        MediaManager.Start();
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

    private static void BroadcastThumbnail(IRandomAccessStreamReference thumbnail)
    {
        if (thumbnail == null) return;

        var thumbnailAsByteArray = NotHelper.GetThumbnailAsByteArray(thumbnail, 4);
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

internal static class NotHelper
{
    public static byte[]? GetThumbnailAsByteArray(IRandomAccessStreamReference thumbnail, uint upscaleModifier)
    {
        if (thumbnail == null)
            return null;

        var imageStream = thumbnail.OpenReadAsync().GetAwaiter().GetResult();
        var decoder = BitmapDecoder.CreateAsync(imageStream).GetAwaiter().GetResult();

        //Calculate new width and height while keeping the original aspect ration
        var originalWidth = decoder.PixelWidth;
        var originalHeight = decoder.PixelHeight;
        uint newWidth, newHeight;

        newWidth = (uint)((double)originalWidth * upscaleModifier);
        newHeight = (uint)((double)originalHeight * upscaleModifier);

        // Create a new BitmapTransformer and set the desired size
        var transformer = new BitmapTransform
        {
            ScaledWidth = newWidth,
            ScaledHeight = newHeight,
            InterpolationMode = BitmapInterpolationMode.Fant
        };

        // Apply the transform
        var resizedPixelData = decoder.GetPixelDataAsync(
            BitmapPixelFormat.Bgra8,
            BitmapAlphaMode.Premultiplied,
            transformer,
            ExifOrientationMode.RespectExifOrientation,
            ColorManagementMode.DoNotColorManage
        ).GetAwaiter().GetResult();

        // Create a new InMemoryRandomAccessStream to store the resized image
        using (var resizedImageStream = new InMemoryRandomAccessStream())
        {
            // Create a BitmapEncoder and set the resized pixel data
            var encoder = BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, resizedImageStream).GetAwaiter()
                .GetResult();
            encoder.SetPixelData(
                BitmapPixelFormat.Bgra8,
                BitmapAlphaMode.Premultiplied,
                newWidth,
                newHeight,
                decoder.DpiX,
                decoder.DpiY,
                resizedPixelData.DetachPixelData()
            );

            // Flush the encoder and return the resized image as a byte array
            encoder.FlushAsync().GetAwaiter().GetResult();

            var resizedImageBytes = new byte[resizedImageStream.Size];

            using (var reader = new DataReader(resizedImageStream))
            {
                reader.LoadAsync((uint)resizedImageStream.Size).GetAwaiter().GetResult();
                reader.ReadBytes(resizedImageBytes);
            }

            return resizedImageBytes;
        }
    }
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