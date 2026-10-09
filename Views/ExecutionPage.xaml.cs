using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Arzhe.Models;
using Arzhe.Services;

namespace Arzhe.Views;

public partial class ExecutionPage : UserControl
{
    public event Action? RequestNext;
    private readonly List<Optimization> _selected;
    private int _successCount = 0;
    private int _warningCount = 0;
    private double _currentProgress = 0;
    private double _targetProgress = 0;
    private System.Windows.Threading.DispatcherTimer? _arcTimer;

    public ExecutionPage(List<Optimization> selected)
    {
        InitializeComponent();
        _selected = selected;
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _arcTimer = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };
        _arcTimer.Tick += OnArcTick;
        _arcTimer.Start();

        AnimateElement(HeaderTitle, HeaderTranslate, 100);
        AnimateElement(HeaderSubtitle, SubtitleTranslate, 250);
        AnimateElement(ProgressCard, null, 400);

        Dispatcher.BeginInvoke(new Action(async () =>
        {
            await Task.Delay(700);
            await RunAsync();
        }));
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (_arcTimer != null)
        {
            _arcTimer.Stop();
            _arcTimer.Tick -= OnArcTick;
            _arcTimer = null;
        }
    }

    private void OnArcTick(object? sender, EventArgs e)
    {
        if (Math.Abs(_currentProgress - _targetProgress) < 0.1)
            _currentProgress = _targetProgress;
        else
            _currentProgress += (_targetProgress - _currentProgress) * 0.15;

        DrawProgressArc(_currentProgress);
        PercentText.Text = $"{_currentProgress:F0}%";
    }

    private async Task RunAsync()
    {
        var optimizer = new SystemOptimizer();
        optimizer.Log += msg => Dispatcher.BeginInvoke(new Action(() => AppendLog(msg)));

        CurrentTaskText.Text = "Point de restauration";
        CurrentStepText.Text = "Creation d'une sauvegarde systeme";
        AppendLogHeader("-- Point de restauration --");
        SetTarget(0);
        await Task.Delay(600);

        bool ok = await optimizer.CreateRestorePointAsync();
        if (ok) { _successCount++; AppendLogSuccess("Point de restauration cree"); }
        else { _warningCount++; AppendLogWarning("Point de restauration impossible"); }

        SetTarget(5);
        await Task.Delay(400);

        int total = Math.Max(_selected.Count, 1);
        int index = 0;
        foreach (var opt in _selected)
        {
            CurrentTaskText.Text = opt.Title;
            CurrentStepText.Text = $"Etape {index + 1} sur {_selected.Count}";
            AppendLogHeader("-- " + opt.Title + " --");

            try
            {
                await opt.Action(optimizer, new ScanResult());
                _successCount++;
            }
            catch (Exception ex)
            {
                _warningCount++;
                AppendLogWarning("Erreur : " + ex.Message);
            }

            index++;
            double pct = 5 + index * 95.0 / total;
            SetTarget(pct);
            await Task.Delay(150);
        }

        CurrentTaskText.Text = "Optimisation terminee";
        CurrentStepText.Text = $"{_successCount} reussies  ·  {_warningCount} avertissements";
        AppendLogSuccess($"=== Termine : {_successCount} succes, {_warningCount} avertissements ===");
        SetTarget(100);
        await Task.Delay(600);

        NextButton.IsEnabled = true;
        AnimateElement(NextButton, ButtonTranslate, 200);
    }

    private void SetTarget(double target) => _targetProgress = target;

    // ============================================================
    //  DESSIN DE L'ARC
    //  Canvas : 120x120
    //  Cercle de fond : centre (60,60), rayon 50 (Ellipse 100x100 a x=10,y=10)
    //  Arc : meme centre (60,60), rayon 46 (legerement plus petit pour etre dans la bordure)
    // ============================================================
    private void DrawProgressArc(double percent)
    {
        const double cx = 60;
        const double cy = 60;
        const double radius = 50;

        if (percent <= 0.01)
        {
            ProgressArc.Data = null;
            return;
        }

        // Cas 100% : on utilise 2 arcs de 180 deg chacun
        // (WPF bug si un seul arc fait 360 deg)
        if (percent >= 99.5)
        {
            var fullGeo = new PathGeometry();
            var fig1 = new PathFigure
            {
                StartPoint = new Point(cx, cy - radius), // haut
                IsClosed = false
            };
            // Premier demi-cercle : haut -> bas (cote droit)
            fig1.Segments.Add(new ArcSegment
            {
                Point = new Point(cx, cy + radius), // bas
                Size = new Size(radius, radius),
                IsLargeArc = false,
                SweepDirection = SweepDirection.Clockwise
            });
            // Deuxieme demi-cercle : bas -> haut (cote gauche)
            fig1.Segments.Add(new ArcSegment
            {
                Point = new Point(cx, cy - radius), // retour au haut
                Size = new Size(radius, radius),
                IsLargeArc = false,
                SweepDirection = SweepDirection.Clockwise
            });
            fullGeo.Figures.Add(fig1);
            ProgressArc.Data = fullGeo;
            return;
        }

        // Cas normal
        double startAngle = -90;
        double endAngle = startAngle + (percent / 100.0) * 360.0;

        double startRad = startAngle * Math.PI / 180;
        double endRad = endAngle * Math.PI / 180;

        double startX = cx + radius * Math.Cos(startRad);
        double startY = cy + radius * Math.Sin(startRad);
        double endX = cx + radius * Math.Cos(endRad);
        double endY = cy + radius * Math.Sin(endRad);

        var figure = new PathFigure
        {
            StartPoint = new Point(startX, startY),
            IsClosed = false
        };

        figure.Segments.Add(new ArcSegment
        {
            Point = new Point(endX, endY),
            Size = new Size(radius, radius),
            IsLargeArc = (percent > 50),
            SweepDirection = SweepDirection.Clockwise
        });

        var geo = new PathGeometry();
        geo.Figures.Add(figure);
        ProgressArc.Data = geo;
    }

    // ============================================================
    //  JOURNAL
    // ============================================================
    private void AppendLog(string msg) => AppendColored(msg, Color.FromArgb(180, 255, 255, 255), false);
    private void AppendLogSuccess(string msg) => AppendColored("OK  " + msg, Color.FromRgb(74, 222, 128), false);
    private void AppendLogWarning(string msg) => AppendColored("!!  " + msg, Color.FromRgb(251, 191, 36), false);
    private void AppendLogHeader(string msg) => AppendColored(msg, Color.FromRgb(167, 139, 250), true);

    private void AppendColored(string msg, Color color, bool bold)
    {
        var tb = new TextBlock
        {
            Text = msg,
            FontFamily = new FontFamily("Cascadia Mono, Consolas"),
            FontSize = 11,
            Foreground = new SolidColorBrush(color),
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 1, 0, 1)
        };
        if (bold) tb.FontWeight = FontWeights.Bold;

        LogPanel.Children.Add(tb);
        LogLineCount.Text = LogPanel.Children.Count + " lignes";
        LogScroll.ScrollToEnd();
    }

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

    private void Next_Click(object sender, RoutedEventArgs e) => RequestNext?.Invoke();
}