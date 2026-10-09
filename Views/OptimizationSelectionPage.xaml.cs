using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Arzhe.Models;
using Arzhe.Services;

namespace Arzhe.Views;

public partial class OptimizationSelectionPage : UserControl
{
    public event Action<List<Optimization>>? RequestNext;
    private readonly ObservableCollection<Optimization> _options = new();
    private readonly ScanResult _scan;

    public OptimizationSelectionPage(ScanResult scan)
    {
        InitializeComponent();
        _scan = scan;

        var catalog = OptimizationsCatalog.Build(scan);
        foreach (var opt in catalog)
        {
            opt.PropertyChanged += OnOptPropertyChanged;
            _options.Add(opt);
        }

        OptionsList.ItemsSource = _options;
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        AnimateElement(HeaderTitle, HeaderTranslate, 100);
        AnimateElement(HeaderSubtitle, SubtitleTranslate, 250);
        AnimateElement(CounterBorder, null, 350);
        AnimateElement(ProgressBorder, null, 400);
        AnimateElement(PresetsPanel, null, 450);
        AnimateElement(ButtonsPanel, ButtonsTranslate, 550);

        UpdateCounter();
    }

    private void OnOptPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Optimization.IsSelected))
            UpdateCounter();
    }

    private void Toggle_Click(object sender, RoutedEventArgs e)
    {
        // L'evenement se propage, on empeche le Card_Click de se declencher
        e.Handled = true;
    }

    private void Card_Click(object sender, MouseButtonEventArgs e)
    {
        // Clic sur la carte -> toggle
        if (sender is FrameworkElement fe && fe.DataContext is Optimization opt)
        {
            if (opt.IsAvailableForPc)
            {
                opt.IsSelected = !opt.IsSelected;
            }
        }
    }

    private void UpdateCounter()
    {
        int totalAvailable = _options.Count(o => o.IsAvailableForPc);
        int selectedAvailable = _options.Count(o => o.IsAvailableForPc && o.IsSelected);

        CounterText.Text = selectedAvailable.ToString();
        CounterTotal.Text = totalAvailable.ToString();

        // Barre de progression
        double pct = totalAvailable > 0 ? (selectedAvailable / (double)totalAvailable) : 0;
        double maxWidth = 300;
        ProgressFill.Width = maxWidth * pct;
    }

    // ============================================================
    //  PRESETS
    // ============================================================
    private void PresetSafe_Click(object sender, RoutedEventArgs e)
    {
        foreach (var opt in _options.Where(o => o.IsAvailableForPc))
            opt.IsSelected = (opt.Risk == RiskLevel.Safe);
    }

    private void PresetRecommended_Click(object sender, RoutedEventArgs e)
    {
        foreach (var opt in _options.Where(o => o.IsAvailableForPc))
            opt.IsSelected = (opt.Risk != RiskLevel.Advanced);
    }

    private void PresetAll_Click(object sender, RoutedEventArgs e)
    {
        foreach (var opt in _options.Where(o => o.IsAvailableForPc))
            opt.IsSelected = true;
    }

    // ============================================================
    //  BOUTONS
    // ============================================================
    private void SelectAll_Click(object sender, RoutedEventArgs e)
    {
        foreach (var opt in _options.Where(o => o.IsAvailableForPc))
            opt.IsSelected = true;
    }

    private void DeselectAll_Click(object sender, RoutedEventArgs e)
    {
        foreach (var opt in _options.Where(o => o.IsAvailableForPc))
            opt.IsSelected = false;
    }

    private void Next_Click(object sender, RoutedEventArgs e)
    {
        var selected = _options
            .Where(o => o.IsSelected && o.IsAvailableForPc)
            .ToList();
        RequestNext?.Invoke(selected);
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
        element.BeginAnimation(UIElement.OpacityProperty, fade);

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
}