using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Arzhe.Services;

namespace Arzhe.Views;

public class OptimizationOption : INotifyPropertyChanged
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";

    private bool _isSelected = true;
    public bool IsSelected
    {
        get => _isSelected;
        set { _isSelected = value; OnPropertyChanged(); }
    }
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? n = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}

public partial class OptimizationSelectionPage : UserControl
{
    public event Action<List<OptimizationOption>>? RequestNext;
    private readonly ObservableCollection<OptimizationOption> _options = new();

    public OptimizationSelectionPage(HardwareInfo hw)
    {
        InitializeComponent();

        void Add(string id, string t, string d) =>
            _options.Add(new OptimizationOption { Id = id, Title = t, Description = d });

        Add("power",    "Plan Ultimate Performance",
                        "Plan d'alimentation haute performance avec r\u00E9glages CPU raisonnables.");
        Add("gamebar",  "Mode Jeu Windows + Game DVR off",
                        "D\u00E9sactive les captures en arri\u00E8re-plan et lib\u00E8re des ressources.");
        Add("visual",   "Effets visuels all\u00E9g\u00E9s",
                        "R\u00E9duit les animations syst\u00E8me sans d\u00E9grader l'exp\u00E9rience.");
        Add("temp",     "Nettoyage intelligent",
                        "Temp, Prefetch, cache Windows Update. Aucune suppression critique.");
        Add("services", "Services non critiques",
                        "D\u00E9sactive uniquement DiagTrack (t\u00E9l\u00E9m\u00E9trie).");
        Add("network",  "Optimisation r\u00E9seau",
                        "Nagle d\u00E9sactiv\u00E9 + DNS Cloudflare (1.1.1.1) + Network Throttling supprim\u00E9.");
        Add("mouse",    "Souris raw input",
                        "D\u00E9sactive l'acc\u00E9l\u00E9ration souris pour un viseur constant.");
        Add("usb",      "Suspension USB d\u00E9sactiv\u00E9e",
                        "\u00C9vite les coupures de p\u00E9riph\u00E9riques en pleine partie.");
        Add("gpu",      "GPU Hardware Scheduling",
                        "Active la planification GPU mat\u00E9rielle (support requis).");

        if (hw.HasValorant)
            Add("valorant", "Tweaks Valorant",
                "Pr\u00E9f\u00E9rence GPU haute performance pour VALORANT.");
        if (hw.HasFortnite)
            Add("fortnite", "Tweaks Fortnite",
                "Pr\u00E9f\u00E9rence GPU haute performance pour Fortnite.");
        if (hw.HasRoblox)
            Add("roblox",   "Tweaks Roblox",
                "Pr\u00E9f\u00E9rence GPU haute performance pour Roblox.");

        Add("disk",     "Optimisation disque (TRIM)",
                        "Optimize-Volume sur volumes fixes uniquement.");

        OptionsList.ItemsSource = _options;
    }

    private void SelectAll_Click(object sender, RoutedEventArgs e)
    {
        bool all = _options.All(o => o.IsSelected);
        foreach (var o in _options) o.IsSelected = !all;
    }

    private void Next_Click(object sender, RoutedEventArgs e)
    {
        var selected = _options.Where(o => o.IsSelected).ToList();
        RequestNext?.Invoke(selected);
    }
}