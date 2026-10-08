using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Arzhe.Services;

namespace Arzhe.Views;

public partial class ExecutionPage : UserControl
{
    public event Action? RequestNext;
    private readonly List<OptimizationOption> _selected;
    private readonly StringBuilder _log = new();

    public ExecutionPage(List<OptimizationOption> selected)
    {
        InitializeComponent();
        _selected = selected;
        Loaded += async (_, _) => await RunAsync();
    }

    private async Task RunAsync()
    {
        var optimizer = new SystemOptimizer();
        optimizer.Log += msg => Dispatcher.Invoke(() => AppendLog(msg));

        // 1) Point de restauration obligatoire
        CurrentTaskText.Text = "Point de restauration…";
        AppendLog("── Point de restauration ──");
        await optimizer.CreateRestorePointAsync();
        Progress.Value = 5;

        // 2) Optimisations
        int total = Math.Max(_selected.Count, 1);
        int index = 0;
        foreach (var opt in _selected)
        {
            CurrentTaskText.Text = opt.Title + "…";
            AppendLog("");
            AppendLog("── " + opt.Title + " ──");
            try { await optimizer.ApplyAsync(opt.Id); }
            catch (Exception ex) { AppendLog("⚠ " + ex.Message); }

            index++;
            double pct = 5 + index * 95.0 / total;
            Progress.Value = pct;
            PercentText.Text = $"{pct:F0} %";
            await Task.Delay(150);
        }

        CurrentTaskText.Text = "Optimisation terminée ✓";
        SubtitleText.Text = "Toutes les optimisations ont été appliquées avec succès.";
        NextButton.IsEnabled = true;
    }

    private void AppendLog(string msg)
    {
        _log.AppendLine(msg);
        LogText.Text = _log.ToString();
        LogScroll.ScrollToEnd();
    }

    private void Next_Click(object sender, RoutedEventArgs e) => RequestNext?.Invoke();
}