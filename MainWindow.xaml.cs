using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Arzhe.Services;
using Arzhe.Views;

namespace Arzhe;

public partial class MainWindow : Window
{
    private HardwareInfo? _hw;

    public MainWindow()
    {
        InitializeComponent();
        Navigate(new WelcomePage());
    }

    public void Navigate(object view)
    {
        MainFrame.Opacity = 0;
        FrameTranslate.X = 40;
        MainFrame.Content = view;
        HookEvents(view);

        var fade = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(320))
        { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
        var slide = new DoubleAnimation(40, 0, TimeSpan.FromMilliseconds(320))
        { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };

        MainFrame.BeginAnimation(OpacityProperty, fade);
        FrameTranslate.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, slide);
    }

    private void HookEvents(object view)
    {
        if (view is WelcomePage wp)
            wp.RequestNext += () => Navigate(new SystemAnalysisPage());

        if (view is SystemAnalysisPage sap)
            sap.RequestNext += hw =>
            {
                _hw = hw;
                Navigate(new OptimizationSelectionPage(hw));
            };

        if (view is OptimizationSelectionPage osp)
            osp.RequestNext += selected => Navigate(new ExecutionPage(selected));

        if (view is ExecutionPage ep)
            ep.RequestNext += () => Navigate(new FinishPage());
    }

    private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left) DragMove();
    }
    private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}