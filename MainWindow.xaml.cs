using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using Arzhe.Models;
using Arzhe.Views;

namespace Arzhe;

public partial class MainWindow : Window
{
    private ScanResult? _scan;
    private bool _isAnimating;

    public MainWindow()
    {
        InitializeComponent();
        Navigate(new WelcomePage());
    }

    public void Navigate(object view)
    {
        if (_isAnimating) return;
        _isAnimating = true;

        // 1. Animation de SORTIE (page actuelle qui part)
        var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(180))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
        };
        var slideOut = new DoubleAnimation(0, -30, TimeSpan.FromMilliseconds(180))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
        };
        var blurOut = new DoubleAnimation(0, 8, TimeSpan.FromMilliseconds(180))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
        };

        // Créer un effet blur temporaire si pas présent
        if (MainFrame.Effect == null)
            MainFrame.Effect = new BlurEffect { Radius = 0 };

        var blurEffect = (BlurEffect)MainFrame.Effect;

        fadeOut.Completed += (_, _) =>
        {
            // 2. Changer la page
            MainFrame.Content = view;
            HookEvents(view);

            // Reset
            FrameTranslate.X = 40;
            blurEffect.Radius = 8;

            // 3. Animation d'ENTREE (nouvelle page qui arrive)
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(320))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            var slideIn = new DoubleAnimation(40, 0, TimeSpan.FromMilliseconds(320))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            var blurIn = new DoubleAnimation(8, 0, TimeSpan.FromMilliseconds(400))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            fadeIn.Completed += (_, _) => _isAnimating = false;

            MainFrame.BeginAnimation(OpacityProperty, fadeIn);
            FrameTranslate.BeginAnimation(TranslateTransform.XProperty, slideIn);
            blurEffect.BeginAnimation(BlurEffect.RadiusProperty, blurIn);
        };

        MainFrame.BeginAnimation(OpacityProperty, fadeOut);
        FrameTranslate.BeginAnimation(TranslateTransform.XProperty, slideOut);
        blurEffect.BeginAnimation(BlurEffect.RadiusProperty, blurOut);
    }

    private void HookEvents(object view)
    {
        if (view is WelcomePage wp)
            wp.RequestNext += () => Navigate(new SystemAnalysisPage());

        if (view is SystemAnalysisPage sap)
            sap.RequestNext += (scan) =>
            {
                _scan = scan;
                Navigate(new OptimizationSelectionPage(_scan));
            };

        if (view is OptimizationSelectionPage osp)
            osp.RequestNext += selected => Navigate(new ExecutionPage(selected));

        if (view is ExecutionPage ep)
            ep.RequestNext += () => Navigate(new FinishPage());
    }

    private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}