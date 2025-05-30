using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using MyPaperPlayer.Audio;
using System;
using System.Collections.ObjectModel;
using System.IO;
using MyPaperPlayer.ViewModels;
using System.Collections.Generic;
using System.Linq;
namespace MyPaperPlayer;

public partial class MainWindow : Window
{
    public static MainWindow _instance = null;
    internal AudioBackend player;
    private DispatcherTimer guiTimer;
    //public MainViewModel ViewModel { get; } = new();
    public MainViewModel ViewModel { get; } = new();
    public MainWindow()
    {
        InitializeComponent();
        DataContext = ViewModel;
        _instance = this;

        player = new AudioBackend();

        var track = new AudioTrack("/home/fierke/Nextcloud/Music/Ableton/_export/250501 - ATest 147.wav");
        var track2 = new AudioTrack("/home/fierke/Nextcloud/Music/Ableton/_export/250528 - ATest 150.wav");
        ViewModel.EnqueueTrack(track);
        ViewModel.EnqueueTrack(track2);

        // ViewModel.Queue ist die ObservableCollection, an die die ListBox gebunden ist
        

        player.Play();

        guiTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(50)
        };
        guiTimer.Tick += (s, e) => RefreshGUI();
        guiTimer.Start();
    }
    public void Exit(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Environment.Exit(0);
    }

    public void RefreshGUI()
    {
        // var current = player.CurrentTrack;
        // if (current == null || player == null)
        //     return;

        try
        {
            // Zeit-Labels aktualisieren
            var currentTime = TimeSpan.FromSeconds(Math.Floor(player.CurrentTime.TotalSeconds));
            var totalTime = TimeSpan.FromSeconds(Math.Floor(player.TotalDuration.TotalSeconds));
            var timeLeft = totalTime - currentTime;

            CurrentTimeLabel.Content = $"{currentTime:hh\\:mm\\:ss} / -{timeLeft:hh\\:mm\\:ss}";
            TotalTimeLabel.Content = $"{totalTime:hh\\:mm\\:ss}";

            // Slider setzen
            SldTime.Maximum = player.TotalDuration.TotalMilliseconds;
            SldTime.Value = player.CurrentTime.TotalMilliseconds;


            // Queue aktualisieren, falls sich was geändert hat
            if (player.QueueChanged)
            {
                //QueueListBox.Items.Clear();
                // foreach(var track in player._queue)
                // {
                //     var item = new ListBoxItem
                //     {
                //         Content = Path.GetFileName(track.Path),
                //         Tag = track
                //     };
                //     QueueListBox.Items.Add(item);
                // }

                // QueueItems.Clear();
                // foreach (var track in player.Queue)
                // {
                //     QueueItems.Add(track);
                // }

                player.QueueChanged = false;
            }

            AdaptPlayButtonIcon();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GUI-Update-Fehler: {ex.Message}");
        }
    }

    public ObservableCollection<AudioTrack> QueueItems { get; } = new();


    public void BtnPlay_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {

        if (player.IsPlaying)
        {
            player.Pause();
            //BtnPlay.Content = "Play";
        }
        else
        {
            player.Play();
            //BtnPlay.Content = "Pause";
        }
    }


    public void BtnNext_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {

    }

    public void BtnBack_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {

    }

    public void BtnStop_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        player.Stop();
    }

    public void AdaptPlayButtonIcon()
    {
        if (player.IsPlaying)
        {
            BtnPlayIcon.Source = new Bitmap("Assets/Icons/pause.png");

        }
        else
        {
            BtnPlayIcon.Source = new Bitmap("Assets/Icons/play.png");

        }
    }

    public async void BtnAddFiles_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            AllowMultiple = true,
            Filters = new List<FileDialogFilter>
            {
                new FileDialogFilter { Name = "Audio", Extensions = { "mp3", "wav", "flac", "ogg" } }
            }
        };

        var files = await dialog.ShowAsync(this);
        if (files is null) return;

        foreach (var file in files)
        {
            var track = new AudioTrack(file);
            player.Enqueue(track);
            QueueItems.Add(track);
        }

        player.QueueChanged = true;
    }


    public void BtnRemoveSelected_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (QueueDataGrid.SelectedItem is not AudioTrack selectedTrack) return;

        player.RemoveFromQueue(selectedTrack);
        QueueItems.Remove(selectedTrack);

        player.QueueChanged = true;
    }


    public void BtnClearQueue_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        player.ClearQueue();
        QueueItems.Clear();

        player.QueueChanged = true;
    }

    public void BtnSortQueue_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var sorted = QueueItems.OrderBy(t => t.Title).ToList();

        QueueItems.Clear();
        foreach (var track in sorted)
        {
            QueueItems.Add(track);
        }

        player.SortQueue(sorted);
        player.QueueChanged = true;
    }

    public void BtnShuffleQueue_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var rnd = new Random();
        var shuffled = QueueItems.OrderBy(_ => rnd.Next()).ToList();

        QueueItems.Clear();
        foreach (var track in shuffled)
        {
            QueueItems.Add(track);
        }

        player.SortQueue(shuffled); // dieselbe Methode wie bei sortieren
        player.QueueChanged = true;
    }

    public void BtnShowInfo_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        
    }
    
}
