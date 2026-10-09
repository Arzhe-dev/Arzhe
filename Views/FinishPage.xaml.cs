using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Arzhe.Services;

namespace Arzhe.Views;

public partial class FinishPage : UserControl
{
    private readonly Random _rng = new();
    private readonly List<Confetti> _confettis = new();
    private System.Windows.Threading.DispatcherTimer? _confettiTimer;
    private DateTime _startTime;
    private bool _isLoaded;

    // Stats (recuperees depuis ExecutionPage - simule pour l'instant)
    public int Success { get; set; } = 22;
    public int Warnings { get; set; } = 0;
    public TimeSpan Duration { get; set; } = TimeSpan.FromSeconds(8);

    private class Confetti
    {
        public Rectangle Shape = null!;
        public double SpeedY;
        public double SpeedX;
        public double Rotation;
        public double RotationSpeed;
        public double Size;
    }

    public FinishPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _isLoaded = true;
        _startTime = DateTime.Now;

        // 1. Animer le check
        AnimateCheck();

        // 2. Animations de texte
        AnimateElement(TitleText, TitleTranslate, 500);
        AnimateElement(SubtitleText, null, 700);
        AnimateElement(StatsPanel, StatsTranslate, 900);
        AnimateElement(ButtonsPanel, ButtonsTranslate, 1100);

        // 3. Remplir les stats
        FillStats();

        // 4. Demarrer les confettis
        Dispatcher.BeginInvoke(new Action(() =>
        {
            GenerateConfetti();
            StartConfettiTimer();
        }));
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _isLoaded = false;
        if (_confettiTimer != null)
        {
            _confettiTimer.Stop();
            _confettiTimer.Tick -= OnConfettiTick;
            _confettiTimer = null;
        }
        ConfettiCanvas.Children.Clear();
        _confettis.Clear();
        GC.Collect(2, GCCollectionMode.Forced, true);
    }

    // ============================================================
    //  CHECK ANIME
    // ============================================================
    private void AnimateCheck()
    {
        // Cercle : scale bounce
        var scaleX = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(700))
        {
            BeginTime = TimeSpan.FromMilliseconds(100),
            EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.5 }
        };
        var scaleY = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(700))
        {
            BeginTime = TimeSpan.FromMilliseconds(100),
            EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.5 }
        };
        CheckScale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleX);
        CheckScale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleY);

        var fadeCircle = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(400))
        {
            BeginTime = TimeSpan.FromMilliseconds(100)
        };
        CheckCircle.BeginAnimation(OpacityProperty, fadeCircle);

        // Halo
        var fadeHalo = new DoubleAnimation(0, 0.6, TimeSpan.FromMilliseconds(800))
        {
            BeginTime = TimeSpan.FromMilliseconds(300)
        };
        CheckHalo.BeginAnimation(OpacityProperty, fadeHalo);

        // Pulsation du halo (infinie)
        var pulseHalo = new DoubleAnimation(0.4, 0.7, TimeSpan.FromMilliseconds(1500))
        {
            BeginTime = TimeSpan.FromMilliseconds(1100),
            AutoReverse = true,
            RepeatBehavior = RepeatBehavior.Forever,
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        };
        CheckHalo.BeginAnimation(OpacityProperty, pulseHalo);

        // Check mark : apparaît apres le cercle
        var scaleMarkX = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(500))
        {
            BeginTime = TimeSpan.FromMilliseconds(500),
            EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.8 }
        };
        var scaleMarkY = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(500))
        {
            BeginTime = TimeSpan.FromMilliseconds(500),
            EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.8 }
        };
        CheckMarkScale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleMarkX);
        CheckMarkScale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleMarkY);
    }

    private void FillStats()
    {
        SuccessCountText.Text = Success.ToString();
        WarningCountText.Text = Warnings.ToString();
        DurationText.Text = Duration.TotalSeconds.ToString("F1") + "s";
    }

    // ============================================================
    //  CONFETTI
    // ============================================================
    private void GenerateConfetti()
    {
        _confettis.Clear();
        ConfettiCanvas.Children.Clear();

        int count = 40;
        Color[] colors = new[]
        {
            Color.FromRgb(124, 92, 255),    // violet
            Color.FromRgb(167, 139, 250),   // violet clair
            Color.FromRgb(100, 220, 255),   // cyan
            Color.FromRgb(74, 222, 128),    // vert
            Color.FromRgb(251, 191, 36),    // jaune
            Color.FromRgb(255, 255, 255)    // blanc
        };

        double w = ActualWidth > 0 ? ActualWidth : 1180;
        double h = ActualHeight > 0 ? ActualHeight : 700;

        for (int i = 0; i < count; i++)
        {
            var size = _rng.NextDouble() * 6 + 4;
            var isCircle = _rng.NextDouble() < 0.3;

            FrameworkElement shape;
            if (isCircle)
            {
                shape = new Ellipse { Width = size, Height = size };
            }
            else
            {
                shape = new Rectangle { Width = size, Height = size * 0.5 };
            }

            var color = colors[_rng.Next(colors.Length)];
            var brush = new SolidColorBrush(color);
            if (shape is Ellipse e) e.Fill = brush;
            else if (shape is Rectangle r) r.Fill = brush;

            shape.Opacity = _rng.NextDouble() * 0.5 + 0.5;
            shape.IsHitTestVisible = false;

            // Position de depart : en haut, aleatoire
            var x = _rng.NextDouble() * w;
            var y = -20 - _rng.NextDouble() * 100;
            Canvas.SetLeft(shape, x);
            Canvas.SetTop(shape, y);

            // Rotation initiale
            var transform = new RotateTransform(_rng.Next(360));
            shape.RenderTransform = transform;
            shape.RenderTransformOrigin = new Point(0.5, 0.5);

            ConfettiCanvas.Children.Add(shape);

            _confettis.Add(new Confetti
            {
                Shape = (Rectangle)(shape is Rectangle ? shape : new Rectangle()),
                SpeedY = _rng.NextDouble() * 1.5 + 0.8,
                SpeedX = (_rng.NextDouble() - 0.5) * 1.2,
                Rotation = _rng.Next(360),
                RotationSpeed = (_rng.NextDouble() - 0.5) * 4,
                Size = size
            });
        }
    }

    private void StartConfettiTimer()
    {
        _confettiTimer = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(30)
        };
        _confettiTimer.Tick += OnConfettiTick;
        _confettiTimer.Start();
    }

    private void OnConfettiTick(object? sender, EventArgs e)
    {
        if (!_isLoaded) return;

        double w = ActualWidth > 0 ? ActualWidth : 1180;
        double h = ActualHeight > 0 ? ActualHeight : 700;

        for (int i = 0; i < ConfettiCanvas.Children.Count; i++)
        {
            var child = ConfettiCanvas.Children[i] as FrameworkElement;
            if (child == null) continue;

            var x = Canvas.GetLeft(child) + _confettis[i].SpeedX;
            var y = Canvas.GetTop(child) + _confettis[i].SpeedY;

            // Recommencer en haut si trop bas
            if (y > h + 20)
            {
                y = -20;
                x = _rng.NextDouble() * w;
            }

            // Rebond lateral
            if (x < -20) x = w + 20;
            if (x > w + 20) x = -20;

            Canvas.SetLeft(child, x);
            Canvas.SetTop(child, y);

            // Rotation
            if (child.RenderTransform is RotateTransform rt)
            {
                rt.Angle += _confettis[i].RotationSpeed;
            }
        }
    }

    // ============================================================
    //  ANIMATIONS
    // ============================================================
    private void AnimateElement(UIElement element, TranslateTransform? translate, int delayMs)
    {
        var fade = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(500))
        {
            BeginTime = TimeSpan.FromMilliseconds(delayMs),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        element.BeginAnimation(OpacityProperty, fade);

        if (translate != null)
        {
            var slide = new DoubleAnimation(20, 0, TimeSpan.FromMilliseconds(500))
            {
                BeginTime = TimeSpan.FromMilliseconds(delayMs),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            translate.BeginAnimation(TranslateTransform.YProperty, slide);
        }
    }

    // ============================================================
    //  BOUTONS
    // ============================================================
    private void Report_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var rec = new OptimizationRecord
            {
                Date = DateTime.Now,
                Cpu = "Consultez le rapport",
                Gpu = "",
                Ram = "",
                AppliedModules = new List<string> { $"{Success} optimisations appliquees" }
            };
            var path = ReportService.ExportTextReport(rec);
            if (!string.IsNullOrEmpty(path))
            {
                Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{path}\"")
                { UseShellExecute = true });
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Erreur export : " + ex.Message, "Arzhe",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void Quit_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
}