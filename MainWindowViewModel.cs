using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MyPaperPlayer.Audio;

namespace MyPaperPlayer.ViewModels;

public class MainViewModel
{
    public AudioBackend Backend { get; } = new();

    public ObservableCollection<AudioTrack> Queue => Backend.Queue;

    public void EnqueueTrack(AudioTrack track) => Backend.Enqueue(track);

    public void Play() => Backend.Play();
    public void Pause() => Backend.Pause();
    public void Stop() => Backend.Stop();
    public void Next() => Backend.PlayNext();
    public void Previous() => Backend.PlayPrevious();
}

