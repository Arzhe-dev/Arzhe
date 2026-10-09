using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Arzhe;

public partial class SplashWindow : Window
{
    public SplashWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Animation 1 : pulse du logo
        var pulseScale = new DoubleAnimation(0.8, 1.0, TimeSpan.FromMilliseconds(700))
        {
            EasingFunction = new BackEase
            { EasingMode = EasingMode.EaseOut, Amplitude = 0.6 }
        };
        LogoScale.BeginAnimation(ScaleTransform.ScaleXProperty, pulseScale);
        LogoScale.BeginAnimation(ScaleTransform.ScaleYProperty, pulseScale);

        // Animation 2 : barre de chargement
        var progressAnim = new DoubleAnimation(0, 200, TimeSpan.FromMilliseconds(1800))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
        };
        LoadingBar.BeginAnimation(WidthProperty, progressAnim);

        // Animation 3 : aurores flottantes
        var aurora1Anim = new DoubleAnimation(0, -40, TimeSpan.FromSeconds(4))
        {
            AutoReverse = true,
            RepeatBehavior = RepeatBehavior.Forever,
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        };
        Aurora1.BeginAnimation(System.Windows.Controls.Canvas.LeftProperty, aurora1Anim);

        var aurora2Anim = new DoubleAnimation(200, 240, TimeSpan.FromSeconds(5))
        {
            AutoReverse = true,
            RepeatBehavior = RepeatBehavior.Forever,
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        };
        Aurora2.BeginAnimation(System.Windows.Controls.Canvas.LeftProperty, aurora2Anim);

        // Animation 4 : textes qui apparaissent
        var textAnim = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(500))
        {
            BeginTime = TimeSpan.FromMilliseconds(300)
        };
        LoadingText.BeginAnimation(OpacityProperty, textAnim);

        // Fermeture automatique apres 2.2 secondes
        var timer = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(2200)
        };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            Close();
        };
        timer.Start();
    }
}