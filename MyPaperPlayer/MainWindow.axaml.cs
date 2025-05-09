using Avalonia.Controls;
using Avalonia.Threading;
using MyPaperPlayer.Audio;
using System;
using System.IO;

namespace MyPaperPlayer;

public partial class MainWindow : Window
{
    internal AudioBackend player;
    private DispatcherTimer guiTimer;

    public MainWindow()
    {
        InitializeComponent();

        player = new AudioBackend();
        AudioTrack track = new AudioTrack("/home/fierke/Nextcloud/Music/Ableton/_export/250501 - ATest 147.wav");
        player.Enqueue(track);
        player.Play();

        // Timer zur GUI-Aktualisierung starten
        guiTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
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
                QueueListBox.Items.Clear();
                foreach(var track in player._queue)
                {
                    var item = new ListBoxItem
                    {
                        Content = Path.GetFileName(track.Path),
                        Tag = track
                    };
                    QueueListBox.Items.Add(item);
                }
                
                player.QueueChanged = false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GUI-Update-Fehler: {ex.Message}");
        }
    }
}
