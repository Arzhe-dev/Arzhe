using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Arzhe.Models;
using Arzhe.Services;

namespace Arzhe.Views;

public class ScanCard
{
    public string Icon { get; set; } = "";
    public string Label { get; set; } = "";
    public string Value { get; set; } = "";
}

public partial class SystemAnalysisPage : UserControl
{
    public event Action<ScanResult>? RequestNext;
    private readonly ObservableCollection<ScanCard> _cards = new();
    private ScanResult _scan = new();
    private double _barMaxWidth = 0;

    public SystemAnalysisPage()
    {
        InitializeComponent();
        CardsContainer.ItemsSource = _cards;
        Loaded += OnLoaded;
        SizeChanged += OnSizeChanged;
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        // Recalculer la largeur max de la barre quand la fenetre change de taille
        UpdateBarMaxWidth();
    }

    private void UpdateBarMaxWidth()
    {
        // Largeur disponible = largeur du Border progress - marges internes
        if (MainProgressBorder.ActualWidth > 0)
        {
            // 22 = padding du GlassCard * 2
            _barMaxWidth = MainProgressBorder.ActualWidth - 44;
        }
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            // Attendre que le layout soit pret pour connaitre la largeur
            await Task.Delay(50);
            UpdateBarMaxWidth();

            System.Diagnostics.Debug.WriteLine($"[ARZHE] Barre max width = {_barMaxWidth}");

            AnimateElement(HeaderTitle, HeaderTranslate, 100);
            AnimateElement(HeaderSubtitle, SubtitleTranslate, 250);
            AnimateElement(MainProgressBorder, null, 400);

            await Task.Delay(500);

            _scan = await SystemScanner.ScanAsync((msg, pct) =>
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    StatusText.Text = msg;
                    PercentText.Text = $"{pct} %";

                    if (_barMaxWidth <= 0) UpdateBarMaxWidth();

                    var targetWidth = _barMaxWidth * (pct / 100.0);
                    var anim = new DoubleAnimation(ProgressFill.Width, targetWidth,
                        TimeSpan.FromMilliseconds(250))
                    {
                        EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                    };
                    ProgressFill.BeginAnimation(WidthProperty, anim);
                }));
            });

            // Barre a 100% garanti
            if (_barMaxWidth <= 0) UpdateBarMaxWidth();
            var finalAnim = new DoubleAnimation(ProgressFill.Width, _barMaxWidth,
                TimeSpan.FromMilliseconds(300))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            ProgressFill.BeginAnimation(WidthProperty, finalAnim);

            StatusText.Text = "Analyse terminee";
            await Task.Delay(400);

            var cards = BuildCards();
            foreach (var card in cards)
            {
                _cards.Add(card);
                await Task.Delay(180);
            }

            AnimateElement(NextButton, ButtonTranslate, 200);
            NextButton.IsEnabled = true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ARZHE] ERREUR: {ex.Message}");
            StatusText.Text = "Erreur : " + ex.Message;
        }
    }

    private List<ScanCard> BuildCards()
    {
        var list = new List<ScanCard>();
        list.Add(new ScanCard { Icon = "CPU", Label = "PROCESSEUR", Value = Safe(_scan.Cpu?.Name) });
        list.Add(new ScanCard { Icon = "GPU", Label = "CARTE GRAPHIQUE",
            Value = Safe(_scan.Gpu?.Name) + (_scan.Gpu != null && !string.IsNullOrEmpty(_scan.Gpu.Vendor) ? $" ({_scan.Gpu.Vendor})" : "") });
        list.Add(new ScanCard { Icon = "RAM", Label = "MEMOIRE",
            Value = _scan.Ram != null ? $"{_scan.Ram.TotalMb / 1024} Go @ {_scan.Ram.SpeedMhz} MHz" : "Inconnu" });

        if (_scan.Disks != null && _scan.Disks.Count > 0)
        {
            var disk = _scan.Disks[0];
            list.Add(new ScanCard { Icon = "SSD", Label = "DISQUE " + disk.Letter,
                Value = $"{disk.FreeGb} Go libres / {disk.TotalGb} Go ({disk.Type})" });
        }

        list.Add(new ScanCard { Icon = "OS", Label = "SYSTEME",
            Value = _scan.Os != null ? _scan.Os.Name + $" (Build {_scan.Os.Build})" : "Inconnu" });

        string note = _scan.HealthScore >= 80 ? " - Excellent" :
                      _scan.HealthScore >= 60 ? " - Bon" : " - A ameliorer";
        list.Add(new ScanCard { Icon = "FPS", Label = "SCORE SANTE",
            Value = $"{_scan.HealthScore}/100{note}" });

        if (_scan.InstalledGames != null && _scan.InstalledGames.Count > 0)
            list.Add(new ScanCard { Icon = "GAM", Label = "JEUX DETECTES",
                Value = string.Join(", ", _scan.InstalledGames) });

        return list;
    }

    private static string Safe(string? s) => string.IsNullOrEmpty(s) ? "Inconnu" : s;

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

    private void Next_Click(object sender, RoutedEventArgs e) => RequestNext?.Invoke(_scan);
}