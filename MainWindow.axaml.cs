using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using MyPaperPlayer.Audio;
using System;
using System.Collections.ObjectModel;
using System.IO;
using MyPaperPlayer.ViewModels;
namespace MyPaperPlayer;

public partial class MainWindow : Window
{
    internal AudioBackend player;
    private DispatcherTimer guiTimer;
    //public MainViewModel ViewModel { get; } = new();
    public MainViewModel ViewModel { get; } = new();
    public MainWindow()
    {
        InitializeComponent();
        DataContext = ViewModel;

        player = new AudioBackend();

        var track = new AudioTrack("/home/fierke/Nextcloud/Music/Ableton/_export/250501 - ATest 147.wav");
        player.Enqueue(track);

        // ViewModel.Queue ist die ObservableCollection, an die die ListBox gebunden ist
        ViewModel.Queue.Add(track);

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

    public void AdaptPlayButtonIcon(){
        if(player.IsPlaying)
        {
            BtnPlayIcon.Source = new Bitmap("Assets/Icons/pause.png");
            
        }
        else
        {
            BtnPlayIcon.Source = new Bitmap("Assets/Icons/play.png");

        }
    }
    
}
