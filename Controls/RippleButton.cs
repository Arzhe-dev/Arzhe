using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Arzhe.Controls;

/// <summary>
/// Bouton avec effet ripple (Material Design) au clic.
/// </summary>
public class RippleButton : Button
{
    static RippleButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RippleButton),
            new FrameworkPropertyMetadata(typeof(RippleButton)));
    }

    public RippleButton()
    {
        // Activer le clipping pour que le ripple reste dans le bouton
        ClipToBounds = true;
    }

    protected override void OnClick()
    {
        base.OnClick();
        StartRipple();
    }

    private void StartRipple()
    {
        // Chercher le Grid interne du template
        var grid = Template?.FindName("RippleGrid", this) as Grid;
        if (grid == null) return;

        // Position du clic
        var mousePos = Mouse.GetPosition(this);
        if (mousePos.X < 0 || mousePos.Y < 0)
        {
            // Fallback : centre du bouton
            mousePos = new Point(ActualWidth / 2, ActualHeight / 2);
        }

        // Creer le cercle ripple
        var ripple = new Ellipse
        {
            Width = 0,
            Height = 0,
            Fill = new SolidColorBrush(Color.FromArgb(80, 255, 255, 255)),
            IsHitTestVisible = false,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top
        };

        // Positionner au point de clic
        Canvas.SetLeft(ripple, mousePos.X);
        Canvas.SetTop(ripple, mousePos.Y);

        // Canvas overlay
        var canvas = new Canvas
        {
            Width = ActualWidth,
            Height = ActualHeight,
            IsHitTestVisible = false
        };
        canvas.Children.Add(ripple);

        grid.Children.Add(canvas);

        // Taille finale : couvrir tout le bouton
        double maxDistance = Math.Sqrt(
            Math.Pow(Math.Max(mousePos.X, ActualWidth - mousePos.X), 2) +
            Math.Pow(Math.Max(mousePos.Y, ActualHeight - mousePos.Y), 2)
        ) * 2;

        // Animer la taille
        var sizeAnim = new DoubleAnimation(0, maxDistance,
            TimeSpan.FromMilliseconds(600))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        var opacityAnim = new DoubleAnimation(0.7, 0,
            TimeSpan.FromMilliseconds(600))
        {
            BeginTime = TimeSpan.FromMilliseconds(200),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        // Callback a chaque frame pour animer width/height/left/top
        var startTime = DateTime.Now;
        var duration = 600.0;

        var timer = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };

        timer.Tick += (_, _) =>
        {
            var elapsed = (DateTime.Now - startTime).TotalMilliseconds;
            if (elapsed >= duration)
            {
                grid.Children.Remove(canvas);
                timer.Stop();
                return;
            }

            double t = elapsed / duration;
            double eased = 1 - Math.Pow(1 - t, 3);
            double size = maxDistance * eased;

            ripple.Width = size;
            ripple.Height = size;
            Canvas.SetLeft(ripple, mousePos.X - size / 2);
            Canvas.SetTop(ripple, mousePos.Y - size / 2);

            // Opacite : augmente puis diminue
            double op = t < 0.3
                ? 0.6 * (t / 0.3)
                : 0.6 * (1 - (t - 0.3) / 0.7);
            ripple.Opacity = op;
        };

        timer.Start();
    }
}