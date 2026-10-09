using System;
using System.Collections.Generic;
using System.IO;

namespace Arzhe.Services;

public class OptimizationRecord
{
    public DateTime Date { get; set; }
    public string Cpu { get; set; } = "";
    public string Gpu { get; set; } = "";
    public string Ram { get; set; } = "";
    public List<string> AppliedModules { get; set; } = new();
    public long FreedMb { get; set; }
}

public static class ReportService
{
    private static readonly string DataDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Arzhe");
    private static readonly string ReportDir = Path.Combine(DataDir, "Reports");

    public static void Initialize()
    {
        Directory.CreateDirectory(DataDir);
        Directory.CreateDirectory(ReportDir);
    }

    public static string ExportTextReport(OptimizationRecord rec)
    {
        try
        {
            Directory.CreateDirectory(ReportDir);
            var filename = $"Arzhe_Report_{rec.Date:yyyyMMdd_HHmmss}.txt";
            var path = Path.Combine(ReportDir, filename);

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("========================================");
            sb.AppendLine("  ARZHE - RAPPORT D'OPTIMISATION");
            sb.AppendLine("========================================");
            sb.AppendLine();
            sb.AppendLine($"Date       : {rec.Date:dd/MM/yyyy HH:mm:ss}");
            sb.AppendLine();
            sb.AppendLine("--- Modules appliques ---");
            foreach (var m in rec.AppliedModules)
                sb.AppendLine($"  [OK] {m}");
            sb.AppendLine();
            sb.AppendLine("========================================");

            File.WriteAllText(path, sb.ToString(), System.Text.Encoding.UTF8);
            return path;
        }
        catch { return ""; }
    }
}