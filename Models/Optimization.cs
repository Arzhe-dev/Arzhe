using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Media;
using Arzhe.Services;

namespace Arzhe.Models;

public enum RiskLevel { Safe, Moderate, Advanced }
public enum OptimizationCategory
{
    Performance, Gaming, Network, Privacy, Visual, Storage, Input, Services
}

public class Optimization : INotifyPropertyChanged
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Icon { get; set; } = "";
    public OptimizationCategory Category { get; set; }
    public RiskLevel Risk { get; set; }

    public Func<ScanResult, bool> IsAvailable { get; set; } = _ => true;
    public string? UnavailableReason { get; set; }
    public Func<SystemOptimizer, ScanResult, Task> Action { get; set; } = (_, _) => Task.CompletedTask;

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set { _isSelected = value; OnPropertyChanged(); }
    }

    private bool _isAvailableForPc = true;
    public bool IsAvailableForPc
    {
        get => _isAvailableForPc;
        set { _isAvailableForPc = value; OnPropertyChanged(); OnPropertyChanged(nameof(CardOpacity)); }
    }

    // ============================================================
    //  Proprietes calculees pour le XAML
    // ============================================================
    public string RiskLabel => Risk switch
    {
        RiskLevel.Safe => "Sur",
        RiskLevel.Moderate => "Modere",
        RiskLevel.Advanced => "Avance",
        _ => ""
    };

    public Brush RiskBrush => Risk switch
    {
        RiskLevel.Safe => new SolidColorBrush(Color.FromRgb(74, 222, 128)),      // vert
        RiskLevel.Moderate => new SolidColorBrush(Color.FromRgb(251, 191, 36)), // orange
        RiskLevel.Advanced => new SolidColorBrush(Color.FromRgb(248, 113, 113)),// rouge
        _ => Brushes.Gray
    };

    public string CategoryLabel => Category switch
    {
        OptimizationCategory.Performance => "Performance",
        OptimizationCategory.Gaming => "Gaming",
        OptimizationCategory.Network => "Reseau",
        OptimizationCategory.Privacy => "Confidentialite",
        OptimizationCategory.Visual => "Visuel",
        OptimizationCategory.Storage => "Stockage",
        OptimizationCategory.Input => "Peripheriques",
        OptimizationCategory.Services => "Services",
        _ => ""
    };

    public double CardOpacity => IsAvailableForPc ? 1.0 : 0.4;

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? n = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}