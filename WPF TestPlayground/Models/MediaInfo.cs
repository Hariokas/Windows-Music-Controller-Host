using System.ComponentModel;
using System.Runtime.CompilerServices;
using System;

namespace WPF_TestPlayground.Models;

public class MediaInfo : INotifyPropertyChanged
{
    private int _mediaSessionId;
    private string _artist = "";
    private string _mediaSessionName = "";
    private string _playbackStatus = "";
    private string _songName = "";

    public int MediaSessionId
    {
        get => _mediaSessionId;
        set
        {
            _mediaSessionId = value;
            OnPropertyChanged();
        }
    }

    public string MediaSessionName
    {
        get => _mediaSessionName;
        set
        {
            _mediaSessionName = value;
            OnPropertyChanged();
        }
    }

    public string Artist
    {
        get => _artist;
        set
        {
            _artist = value;
            OnPropertyChanged();
        }
    }

    public string SongName
    {
        get => _songName;
        set
        {
            _songName = value;
            OnPropertyChanged();
        }
    }

    public string PlaybackStatus
    {
        get => _playbackStatus;
        set
        {
            _playbackStatus = value;
            OnPropertyChanged();
        }
    }

    public TimeSpan SongLength { get; set; }
    public TimeSpan CurrentPlaybackTime { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}