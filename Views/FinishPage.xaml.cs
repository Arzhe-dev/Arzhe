using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Arzhe.Views;

public partial class FinishPage : UserControl
{
    public FinishPage()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            var anim = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(550))
            { EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.7 } };
            CheckScale.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
            CheckScale.BeginAnimation(ScaleTransform.ScaleYProperty, anim);
        };
    }

    private void Report_Click(object sender, RoutedEventArgs e)
    {
        try { Process.Start(new ProcessStartInfo("eventvwr.msc") { UseShellExecute = true }); }
        catch { }
    }

    private void Quit_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
}