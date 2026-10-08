using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Arzhe.Views;

public partial class WelcomePage : UserControl
{
    public event Action? RequestNext;

    public WelcomePage()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            var anim = new DoubleAnimation(0.7, 1.0, TimeSpan.FromMilliseconds(900))
            { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
            LogoScale.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
            LogoScale.BeginAnimation(ScaleTransform.ScaleYProperty, anim);
        };
    }

    private void Start_Click(object sender, RoutedEventArgs e) => RequestNext?.Invoke();
}