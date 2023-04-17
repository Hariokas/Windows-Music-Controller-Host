using System;
using System.Windows;
using System.Windows.Media.Imaging;
using Windows.Media.Control;
using Windows.Storage.Streams;
using WindowsMediaController;
using static WindowsMediaController.MediaManager;
using System.Linq;
using System.Collections.ObjectModel;

namespace WPF_TestPlayground
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly MediaManager mediaManager = new MediaManager();
        private static MediaSession? currentSession = null;
        private static MediaSessionHandler mediaSessionHandler;

        public static ObservableCollection<SongInfoModel>? SongList { get; set; }
        public static ObservableCollection<MediaSessionModel>? MediaSessionModelList { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            mediaManager.OnAnySessionOpened += MediaManager_OnAnySessionOpened;
            mediaManager.OnAnySessionClosed += MediaManager_OnAnySessionClosed;
            mediaManager.OnFocusedSessionChanged += MediaManager_OnFocusedSessionChanged;
            mediaManager.OnAnyPlaybackStateChanged += MediaManager_OnAnyPlaybackStateChanged;
            mediaManager.OnAnyMediaPropertyChanged += MediaManager_OnAnyMediaPropertyChanged;

            SongList = new ObservableCollection<SongInfoModel>();
            MediaSessionModelList = new ObservableCollection<MediaSessionModel>();

            mediaSessionHandler = new MediaSessionHandler(MediaSessionModelList);

            DataContext = this;

            mediaManager.Start();
        }

        private static void MediaManager_OnAnySessionOpened(MediaManager.MediaSession session)
        {
            if (session == null)
                return;

            WriteLineColor("-- New Source: " + session.Id, ConsoleColor.Green);

            mediaSessionHandler.AddSession(session);
        }

        private static void MediaManager_OnAnySessionClosed(MediaManager.MediaSession session)
        {
            if (session == null)
                return;

            WriteLineColor("-- Removed Source: " + session.Id, ConsoleColor.Red);

            mediaSessionHandler.RemoveMediaSession(session);
        }

        private static void MediaManager_OnFocusedSessionChanged(MediaManager.MediaSession mediaSession)
        {
            if (mediaSession == null)
                return;

            WriteLineColor("== Session Focus Changed: " + mediaSession?.ControlSession?.SourceAppUserModelId, ConsoleColor.Gray);
        }

        private static void MediaManager_OnAnyPlaybackStateChanged(MediaManager.MediaSession sender, GlobalSystemMediaTransportControlsSessionPlaybackInfo args)
        {
            if (sender == null || args == null)
                return;

            WriteLineColor($"{sender.Id} is now {args.PlaybackStatus}", ConsoleColor.Yellow);
            mediaSessionHandler.UpdatePlaybackState(sender, args);
        }

        private static void MediaManager_OnAnyMediaPropertyChanged(MediaManager.MediaSession sender, GlobalSystemMediaTransportControlsSessionMediaProperties args)
        {
            if (sender == null || args == null)
                return;

            WriteLineColor($"{sender.Id} is now playing {args.Title} {(String.IsNullOrEmpty(args.Artist) ? "" : $"by {args.Artist}")}", ConsoleColor.Cyan);
            mediaSessionHandler.UpdateMediaProperty(sender, args);
        }

        public static void WriteLineColor(object toprint, ConsoleColor color = ConsoleColor.White)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                SongList.Add(new SongInfoModel() { FirstLine = $"{DateTime.Now.ToString("HH:mm:ss.fff")} {toprint}" });
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
        internal static BitmapImage? GetThumbnail(IRandomAccessStreamReference Thumbnail)
        {
            if (Thumbnail == null)
                return null;

            var imageStream = Thumbnail.OpenReadAsync().GetAwaiter().GetResult();
            byte[] fileBytes = new byte[imageStream.Size];
            using (DataReader reader = new DataReader(imageStream))
            {
                reader.LoadAsync((uint)imageStream.Size).GetAwaiter().GetResult();
                reader.ReadBytes(fileBytes);
            }

            var image = new BitmapImage();
            using (var ms = new System.IO.MemoryStream(fileBytes))
            {
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = ms;
                image.EndInit();
            }
            return image;
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