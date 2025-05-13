using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MyPaperPlayer.Audio;

namespace MyPaperPlayer.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    public ObservableCollection<AudioTrack> Queue { get; } = new();

    private AudioTrack? _currentTrack;
    public AudioTrack? CurrentTrack
    {
        get => _currentTrack;
        set
        {
            if (_currentTrack != value)
            {
                _currentTrack = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
