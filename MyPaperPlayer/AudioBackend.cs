using LibVLCSharp.Shared;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
    public readonly Queue<AudioTrack> _queue = new();
    private AudioTrack? _currentTrack;
    private CancellationTokenSource? _fadeCts;
    public bool QueueChanged = true;

    public AudioTrack? CurrentTrack => _currentTrack;

    public AudioBackend()
    {
        Core.Initialize();
        _libVLC = new LibVLC();
        _mediaPlayer = new MediaPlayer(_libVLC);
        _mediaPlayer.EndReached += (_, _) => PlayNext();
    }

    public void Enqueue(AudioTrack track)
    {
        _queue.Enqueue(track);
        QueueChanged = true;
        if (_currentTrack == null)
        {
            Play();
        }
    }

    public void Play()
    {
        if (_mediaPlayer.IsPlaying)
            return;

        PlayNext();
    }

    private void PlayNext()
    {
        if (_queue.Count == 0)
        {
            _currentTrack = null;
            return;
        }

        _currentTrack = _queue.Dequeue();
        QueueChanged = true;
        using var media = new Media(_libVLC, _currentTrack.Path, FromType.FromPath);
        _mediaPlayer.Media = media;
        _mediaPlayer.Play();
    }

    public void Pause() => _mediaPlayer.Pause();

    public void Stop()
    {
        _mediaPlayer.Stop();
        _queue.Clear();
        QueueChanged = true;
        _currentTrack = null;
    }

    public TimeSpan CurrentTime => TimeSpan.FromMilliseconds(_mediaPlayer.Time);
    public TimeSpan TotalDuration => TimeSpan.FromMilliseconds(_mediaPlayer.Length);
}
public class AudioTrack
{
    public string Path { get; }
    public string Title { get; set; }
    public string Artist { get; set; }

    public AudioTrack(string path)
    {
        Path = path;
        ResolveTags();
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