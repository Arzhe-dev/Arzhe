using System;
using System.Management;

namespace Arzhe.Services;

public class HardwareInfo
{
    public string Cpu { get; set; } = "Inconnu";
    public string Gpu { get; set; } = "Inconnu";
    public string GpuVendor { get; set; } = "Inconnu";
    public string Ram { get; set; } = "Inconnu";
    public string Os { get; set; } = "Inconnu";
    public string Disk { get; set; } = "Inconnu";
    public bool HasValorant { get; set; }
    public bool HasFortnite { get; set; }
    public bool HasRoblox { get; set; }
}

public static class HardwareDetector
{
    public static HardwareInfo Detect()
    {
        var info = new HardwareInfo();
        try { info.Cpu = Wmi("Win32_Processor", "Name"); } catch { }
        try { info.Gpu = Wmi("Win32_VideoController", "Name"); } catch { }
        info.GpuVendor = DetectGpuVendor(info.Gpu);
        try
        {
            var ramBytes = WmiUlong("Win32_ComputerSystem", "TotalPhysicalMemory");
            if (ramBytes > 0) info.Ram = (ramBytes / 1024.0 / 1024 / 1024).ToString("F1") + " Go";
        } catch { }
        try { info.Os = Wmi("Win32_OperatingSystem", "Caption"); } catch { }
        try
        {
            var size = WmiUlong("Win32_LogicalDisk", "Size", "DeviceID='C:'");
            var free = WmiUlong("Win32_LogicalDisk", "FreeSpace", "DeviceID='C:'");
            if (size > 0)
            {
                double totalGb = size / 1024.0 / 1024 / 1024;
                double freeGb = free / 1024.0 / 1024 / 1024;
                info.Disk = $"{freeGb:F1} Go libres / {totalGb:F1} Go";
            }
        } catch { }

        info.HasValorant = DetectGame("VALORANT");
        info.HasFortnite = DetectGame("Fortnite");
        info.HasRoblox   = DetectGame("Roblox");

        return info;
    }

    private static bool DetectGame(string name)
    {
        try
        {
            foreach (var drive in new[] { "C:", "D:", "E:" })
            {
                var paths = new[]
                {
                    $@"{drive}\Riot Games\VALORANT",
                    $@"{drive}\Program Files\Epic Games\Fortnite",
                    $@"{drive}\Program Files (x86)\Epic Games\Fortnite",
                    $@"{drive}\Program Files (x86)\Roblox",
                    $@"{drive}\Program Files\Roblox",
                };
                foreach (var p in paths)
                    if (p.Contains(name, StringComparison.OrdinalIgnoreCase) && System.IO.Directory.Exists(p))
                        return true;
            }
        }
        catch { }
        return false;
    }

    private static string DetectGpuVendor(string gpu)
    {
        var g = gpu.ToLowerInvariant();
        if (g.Contains("nvidia") || g.Contains("geforce") || g.Contains("rtx") || g.Contains("gtx")) return "NVIDIA";
        if (g.Contains("amd") || g.Contains("radeon")) return "AMD";
        if (g.Contains("intel") || g.Contains("iris") || g.Contains("uhd")) return "Intel";
        return "Inconnu";
    }

    private static string Wmi(string cls, string prop, string? where = null)
    {
        var q = $"SELECT {prop} FROM {cls}" + (where != null ? $" WHERE {where}" : "");
        using var s = new ManagementObjectSearcher(q);
        foreach (var o in s.Get())
        {
            var v = o[prop];
            if (v != null) return v.ToString() ?? "Inconnu";
        }
        return "Inconnu";
    }

    private static ulong WmiUlong(string cls, string prop, string? where = null)
    {
        var q = $"SELECT {prop} FROM {cls}" + (where != null ? $" WHERE {where}" : "");
        using var s = new ManagementObjectSearcher(q);
        foreach (var o in s.Get())
        {
            var v = o[prop];
            if (v != null && ulong.TryParse(v.ToString(), out var r)) return r;
        }
        return 0;
    }
}