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
    //internal AudioBackend player;
    private DispatcherTimer guiTimer;
    //public MainViewModel ViewModel { get; } = new();
    public MainViewModel ViewModel { get; } = new();
    public MainWindow()
    {
        InitializeComponent();
        DataContext = ViewModel;
        _instance = this;

        //ViewModel.Backend = new AudioBackend();

        var track = new AudioTrack("/home/fierke/Nextcloud/Music/Ableton/_export/250501 - ATest 147.wav");
        var track2 = new AudioTrack("/home/fierke/Nextcloud/Music/Ableton/_export/250528 - ATest 150.wav");
        ViewModel.EnqueueTrack(track);
        ViewModel.EnqueueTrack(track2);

        // ViewModel.Queue ist die ObservableCollection, an die die ListBox gebunden ist
        

        ViewModel.Play();

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
        // var current = ViewModel.Backend.CurrentTrack;
        // if (current == null || ViewModel.Backend == null)
        //     return;

        try
        {
            // Zeit-Labels aktualisieren
            var currentTime = TimeSpan.FromSeconds(Math.Floor(ViewModel.Backend.CurrentTime.TotalSeconds));
            var totalTime = TimeSpan.FromSeconds(Math.Floor(ViewModel.Backend.TotalDuration.TotalSeconds));
            var timeLeft = totalTime - currentTime;

            CurrentTimeLabel.Content = $"{currentTime:hh\\:mm\\:ss} / -{timeLeft:hh\\:mm\\:ss}";
            TotalTimeLabel.Content = $"{totalTime:hh\\:mm\\:ss}";

            // Slider setzen
            SldTime.Maximum = ViewModel.Backend.TotalDuration.TotalMilliseconds;
            SldTime.Value = ViewModel.Backend.CurrentTime.TotalMilliseconds;


            // Queue aktualisieren, falls sich was geändert hat
            if (ViewModel.Backend.QueueChanged)
            {
                //QueueListBox.Items.Clear();
                // foreach(var track in ViewModel.Backend._queue)
                // {
                //     var item = new ListBoxItem
                //     {
                //         Content = Path.GetFileName(track.Path),
                //         Tag = track
                //     };
                //     QueueListBox.Items.Add(item);
                // }

                // QueueItems.Clear();
                // foreach (var track in ViewModel.Backend.Queue)
                // {
                //     QueueItems.Add(track);
                // }

                ViewModel.Backend.QueueChanged = false;
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

        if (ViewModel.Backend.IsPlaying)
        {
            ViewModel.Pause();
            //BtnPlay.Content = "Play";
        }
        else
        {
            ViewModel.Play();
            //BtnPlay.Content = "Pause";
        }
    }


    public void BtnNext_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ViewModel.Next();
    }

    public void BtnBack_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ViewModel.Previous();
    }

    public void BtnStop_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ViewModel.Stop();
    }

    public void AdaptPlayButtonIcon()
    {
        if (ViewModel.Backend.IsPlaying)
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
            ViewModel.EnqueueTrack(track);
            QueueItems.Add(track);
        }

        ViewModel.Backend.QueueChanged = true;
    }


    public void BtnRemoveSelected_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (QueueDataGrid.SelectedItem is not AudioTrack selectedTrack) return;

        ViewModel.RemoveFromQueue(selectedTrack);
        QueueItems.Remove(selectedTrack);

        ViewModel.Backend.QueueChanged = true;
    }


    public void BtnClearQueue_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ViewModel.ClearQueue();
        QueueItems.Clear();

        ViewModel.Backend.QueueChanged = true;
    }

    public void BtnSortQueue_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var sorted = QueueItems.OrderBy(t => t.Title).ToList();

        QueueItems.Clear();
        foreach (var track in sorted)
        {
            QueueItems.Add(track);
        }

        ViewModel.SortQueue(sorted);
        ViewModel.Backend.QueueChanged = true;
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

        ViewModel.SortQueue(shuffled); // dieselbe Methode wie bei sortieren
        ViewModel.Backend.QueueChanged = true;
    }

    public void BtnShowInfo_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        
    }
    
}
