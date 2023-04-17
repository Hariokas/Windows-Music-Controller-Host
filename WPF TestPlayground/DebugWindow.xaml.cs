﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Windows.Media.Control;
using WindowsMediaController;
using static WindowsMediaController.MediaManager;

namespace WPF_TestPlayground
{
    /// <summary>
    /// Interaction logic for DebugWindow.xaml
    /// </summary>
    public partial class DebugWindow : Window
    {
        private static readonly MediaManager mediaManager = new MediaManager();
        private static MediaSession? currentSession = null;

        public List<MediaSession> mediaSessionList { get; set; } = new List<MediaSession>();

        public DebugWindow()
        {
            DataContext = this;
            InitializeComponent();

            mediaManager.OnAnySessionOpened += MediaManager_OnAnySessionOpened;
            mediaManager.OnAnySessionClosed += MediaManager_OnAnySessionClosed;
            mediaManager.OnFocusedSessionChanged += MediaManager_OnFocusedSessionChanged;

            mediaManager.Start();

        }

        private void MediaManager_OnFocusedSessionChanged(MediaSession mediaSession)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                //Change the focused session.
            });
        }

        private void MediaManager_OnAnySessionClosed(MediaSession mediaSession)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (var session in mediaSessionList)
                {
                    if (session.Id == mediaSession.Id)
                        mediaSessionList.Remove(session);
                }
            });
        }

        private void MediaManager_OnAnySessionOpened(MediaSession mediaSession)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var session = mediaSession;

                mediaSessionList.Add(session);
            });
        }

        private void CurrentSession_OnPlaybackStateChanged(MediaSession mediaSession, GlobalSystemMediaTransportControlsSessionPlaybackInfo? playbackInfo = null)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                UpdateUI(mediaSession);
            });
        }

        private void CurrentSession_OnMediaPropertyChanged(MediaSession mediaSession, GlobalSystemMediaTransportControlsSessionMediaProperties mediaProperties)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                UpdateUI(mediaSession);
            });
        }

        private void UpdateUI(MediaSession mediaSession)
        {

        }

    }
}