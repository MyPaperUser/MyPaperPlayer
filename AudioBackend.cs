using LibVLCSharp.Shared;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TagLib;

namespace MyPaperPlayer.Audio;

public class AudioBackend
{
    private readonly LibVLC _libVLC;
    private readonly MediaPlayer _mediaPlayer;
    public bool QueueChanged = true;

    public ObservableCollection<AudioTrack> Queue { get; } = new();
    public int CurrentIndex { get; private set; } = -1;
    public AudioTrack? CurrentTrack => (CurrentIndex >= 0 && CurrentIndex < Queue.Count)
        ? Queue[CurrentIndex]
        : null;

    public bool IsPlaying => _mediaPlayer.IsPlaying;

    public AudioBackend()
    {
        Core.Initialize();
        _libVLC = new LibVLC();
        _mediaPlayer = new MediaPlayer(_libVLC);
        _mediaPlayer.EndReached += (_, _) => PlayNext();
    }

    public void Enqueue(AudioTrack track)
    {
        Queue.Add(track);
        //MainWindow._instance.ViewModel.Queue.Add(track);
        if (CurrentTrack == null)
            Play();
        QueueChanged = true;
    }

    public void ClearQueue()
    {
        Queue.Clear();
        QueueChanged = true;
    }

    public void SortQueue(List<AudioTrack> sortedTracks)
    {
        Queue.Clear();
        foreach (var track in sortedTracks)
        {
            Queue.Add(track);
        }
        QueueChanged = true;
    }

    public void RemoveFromQueue(AudioTrack track)
    {
        var trackToRemove = Queue.FirstOrDefault(t => t == track);
        if (trackToRemove != null)
        {
            Queue.Remove(trackToRemove);
        }
        QueueChanged = true;
    }

    public void Play()
    {
        if (_mediaPlayer.IsPlaying || Queue.Count == 0)
            return;

        if (_mediaPlayer.State == VLCState.Paused)
        {
            _mediaPlayer.Play();
            return;
        }

        if (CurrentIndex == -1)
            CurrentIndex = 0;

        QueueChanged = true;

        PlayCurrent();
    }

    public void PlayCurrent()
    {
        if (CurrentTrack == null)
            return;

        using var media = new Media(_libVLC, CurrentTrack.Path, FromType.FromPath);
        _mediaPlayer.Media = media;
        _mediaPlayer.Play();

        QueueChanged = true;
    }

    public void PlayNext()
    {
        if (CurrentIndex + 1 >= Queue.Count)
            return;

        CurrentIndex++;
        PlayCurrent();

        QueueChanged = true;
    }

    public void PlayPrevious()
    {
        if (CurrentIndex > 0)
        {
            CurrentIndex--;
            PlayCurrent();
            QueueChanged = true;
        }
    }

    public void Pause() => _mediaPlayer.Pause();
    public void Stop()
    {
        _mediaPlayer.Stop();
        CurrentIndex = -1;
        
    }

    public TimeSpan CurrentTime => TimeSpan.FromMilliseconds(_mediaPlayer.Time);
    public TimeSpan TotalDuration => TimeSpan.FromMilliseconds(_mediaPlayer.Length);
}

public class AudioTrack
{
    public string Path { get; }
    public string Title { get; set; }
    public string Artist { get; set; }
    public string Length { get; set; }

    public override string ToString() => Title;


    public AudioTrack(string path)
    {
        Path = path;
        ResolveTags();
        //Length = null;
        Length = GetDurationAsync(Path).ToString();
    }

    public TimeSpan GetDurationAsync(string path)
    {
        Core.Initialize();
        using var libVLC = new LibVLC();
        using var media = new Media(libVLC, new Uri(path));

        media.Parse(MediaParseOptions.ParseLocal);
        return TimeSpan.FromMilliseconds(media.Duration);
    }


    private void ResolveTags()
    {
        try
        {
            var file = TagLib.File.Create(Path);  // TagLibSharp API verwenden
            Title = string.IsNullOrWhiteSpace(file.Tag.Title) ? null : file.Tag.Title;
            Artist = file.Tag.FirstPerformer;

            // Wenn weder Titel noch Künstler vorhanden sind, verwenden wir den Dateinamen
            if (string.IsNullOrWhiteSpace(Title) && string.IsNullOrWhiteSpace(Artist))
            {
                Title = System.IO.Path.GetFileNameWithoutExtension(Path);
                Artist = "Unbekannt";
            }
        }
        catch
        {
            // Fehlerbehandlung: Dateiname als Titel setzen, wenn ein Problem auftritt
            Title = System.IO.Path.GetFileNameWithoutExtension(Path);
            Artist = "Unbekannt";
        }
    }
}