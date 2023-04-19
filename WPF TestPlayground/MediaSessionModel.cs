using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WPF_TestPlayground;

public class MediaSessionModel : INotifyPropertyChanged
{
    private string _artist = "";

    private int _id;
    private string _mediaSessionName = "";
    private string _playbackStatus = "";
    private string _songName = "";

    public string Information =>
        $"Status of [{SongName}] by [{Artist}] on [{MediaSessionName}], [{Id}] - [{PlaybackStatus}]";

    public int Id
    {
        get => _id;
        set
        {
            _id = value;
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

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        if (propertyName != nameof(Information)) OnPropertyChanged(nameof(Information));
    }
}