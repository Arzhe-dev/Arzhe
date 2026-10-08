using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Arzhe.Services;

namespace Arzhe.Views;

public partial class SystemAnalysisPage : UserControl
{
    public event Action<HardwareInfo>? RequestNext;
    private readonly ObservableCollection<string> _details = new();
    private HardwareInfo _info = new();

    public SystemAnalysisPage()
    {
        InitializeComponent();
        DetailsList.ItemsSource = _details;
        Loaded += async (_, _) => await RunAsync();
    }

    private async Task RunAsync()
    {
        StatusText.Text = "Detection du materiel...";
        AnalysisProgress.Value = 15;
        await Task.Delay(250);

        _info = await Task.Run(HardwareDetector.Detect);
        AnalysisProgress.Value = 70;

        _details.Add("CPU : " + _info.Cpu);
        await Task.Delay(120);
        _details.Add("GPU : " + _info.Gpu + "  (" + _info.GpuVendor + ")");
        await Task.Delay(120);
        _details.Add("RAM : " + _info.Ram);
        await Task.Delay(120);
        _details.Add("Disque C: : " + _info.Disk);
        await Task.Delay(120);
        _details.Add("OS : " + _info.Os);
        await Task.Delay(120);

        var jeux = new System.Collections.Generic.List<string>();
        if (_info.HasValorant) jeux.Add("Valorant");
        if (_info.HasFortnite) jeux.Add("Fortnite");
        if (_info.HasRoblox)   jeux.Add("Roblox");
        if (jeux.Count > 0)
            _details.Add("Jeux detectes : " + string.Join(", ", jeux));

        AnalysisProgress.Value = 100;
        StatusText.Text = "Analyse terminee.";
        NextButton.IsEnabled = true;
    }

    private void Next_Click(object sender, RoutedEventArgs e) => RequestNext?.Invoke(_info);
}