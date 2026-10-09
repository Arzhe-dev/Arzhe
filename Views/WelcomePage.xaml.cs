using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Arzhe.Views;

public partial class WelcomePage : UserControl
{
    public event Action? RequestNext;

    private readonly List<ParticleData> _particles = new();
    private bool _isLoaded;

    private class ParticleData
    {
        public Ellipse Shape = null!;
        public double Speed;
        public double Drift;
    }

    private readonly Random _rng = new();

    public WelcomePage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _isLoaded = true;

        GenerateParticles();

        // Aurores flottantes
        AnimateAurora(Aurora1Translate, -30, 30, 8);
        AnimateAurora(Aurora2Translate, 30, -30, 10);
        AnimateAurora(Aurora3Translate, -20, 40, 12);

        // Animations en cascade avec effet Pop
        AnimatePop(LogoImage, LogoScale, null, 100, 0.5);
        AnimatePop(TitleText, TitleScale, null, 300, 0.7);
        AnimatePop(SubtitleText, SubtitleScale, null, 450, 0.8);
        AnimatePop(MetaPanel, MetaScale, null, 600, 0.9);
        AnimatePop(StartButton, ButtonScale, null, 750, 0.85);
        AnimateSimple(AdminNote, 900);

        CompositionTarget.Rendering += OnRendering;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _isLoaded = false;
        CompositionTarget.Rendering -= OnRendering;

        ParticlesCanvas.Children.Clear();
        _particles.Clear();

        Aurora1Translate.BeginAnimation(TranslateTransform.XProperty, null);
        Aurora2Translate.BeginAnimation(TranslateTransform.XProperty, null);
        Aurora3Translate.BeginAnimation(TranslateTransform.XProperty, null);

        GC.Collect(2, GCCollectionMode.Forced, true);
    }

    // ============================================================
    //  PARTICULES
    // ============================================================
    private void GenerateParticles()
    {
        _particles.Clear();
        ParticlesCanvas.Children.Clear();

        int count = 15;
        for (int i = 0; i < count; i++)
        {
            var size = _rng.NextDouble() * 2.5 + 1.2;
            var p = new Ellipse
            {
                Width = size,
                Height = size,
                Fill = new SolidColorBrush(Color.FromArgb(
                    (byte)(_rng.Next(80, 160)),
                    167, 139, 250)),
                Opacity = _rng.NextDouble() * 0.5 + 0.3,
                IsHitTestVisible = false
            };

            var x = _rng.NextDouble() * 1400;
            var y = _rng.NextDouble() * 900;
            Canvas.SetLeft(p, x);
            Canvas.SetTop(p, y);

            ParticlesCanvas.Children.Add(p);

            _particles.Add(new ParticleData
            {
                Shape = p,
                Speed = _rng.NextDouble() * 0.8 + 0.4,
                Drift = (_rng.NextDouble() - 0.5) * 0.3
            });
        }
    }

    private double _lastRenderTime;

    private void OnRendering(object? sender, EventArgs e)
    {
        if (!_isLoaded || _particles.Count == 0) return;

        var now = DateTime.Now.TimeOfDay.TotalMilliseconds;
        if (now - _lastRenderTime < 16) return;
        _lastRenderTime = now;

        double w = ActualWidth > 0 ? ActualWidth : 1200;
        double h = ActualHeight > 0 ? ActualHeight : 700;

        for (int i = 0; i < _particles.Count; i++)
        {
            var pd = _particles[i];
            var p = pd.Shape;

            var y = Canvas.GetTop(p) - pd.Speed;
            var x = Canvas.GetLeft(p) + pd.Drift;

            if (y < -10)
            {
                y = h + 10;
                x = _rng.NextDouble() * w;
            }

            if (x < 0 || x > w)
                pd.Drift = -pd.Drift;

            Canvas.SetTop(p, y);
            Canvas.SetLeft(p, x);
        }
    }

    // ============================================================
    //  ANIMATIONS
    // ============================================================
    private void AnimateAurora(TranslateTransform transform, double from, double to, double duration)
    {
        var xAnim = new DoubleAnimation(from, to, TimeSpan.FromSeconds(duration))
        {
            AutoReverse = true,
            RepeatBehavior = RepeatBehavior.Forever,
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        };
        transform.BeginAnimation(TranslateTransform.XProperty, xAnim);

        var yAnim = new DoubleAnimation(from * 0.7, to * 0.7, TimeSpan.FromSeconds(duration * 1.3))
        {
            AutoReverse = true,
            RepeatBehavior = RepeatBehavior.Forever,
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        };
        transform.BeginAnimation(TranslateTransform.YProperty, yAnim);
    }

    /// <summary>
    /// Effet Pop : fade in + scale bounce (rebond elastique)
    /// </summary>
    private void AnimatePop(UIElement element, ScaleTransform scale, TranslateTransform? translate,
                            int delayMs, double startScale)
    {
        // Fade in
        var fade = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(400))
        {
            BeginTime = TimeSpan.FromMilliseconds(delayMs),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        element.BeginAnimation(OpacityProperty, fade);

        // Scale Bounce (BackEase pour l'effet rebond)
        var scaleXAnim = new DoubleAnimation(startScale, 1.0, TimeSpan.FromMilliseconds(600))
        {
            BeginTime = TimeSpan.FromMilliseconds(delayMs),
            EasingFunction = new BackEase
            {
                EasingMode = EasingMode.EaseOut,
                Amplitude = 0.6
            }
        };
        var scaleYAnim = new DoubleAnimation(startScale, 1.0, TimeSpan.FromMilliseconds(600))
        {
            BeginTime = TimeSpan.FromMilliseconds(delayMs),
            EasingFunction = new BackEase
            {
                EasingMode = EasingMode.EaseOut,
                Amplitude = 0.6
            }
        };
        scale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleXAnim);
        scale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleYAnim);

        // Slide down (petit mouvement vertical)
        if (translate != null)
        {
            var slide = new DoubleAnimation(-10, 0, TimeSpan.FromMilliseconds(500))
            {
                BeginTime = TimeSpan.FromMilliseconds(delayMs),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            translate.BeginAnimation(TranslateTransform.YProperty, slide);
        }
    }

    /// <summary>
    /// Animation simple : juste fade in
    /// </summary>
    private void AnimateSimple(UIElement element, int delayMs)
    {
        var fade = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(500))
        {
            BeginTime = TimeSpan.FromMilliseconds(delayMs),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        element.BeginAnimation(OpacityProperty, fade);
    }

    private void Start_Click(object sender, RoutedEventArgs e)
    {
        RequestNext?.Invoke();
    }
}