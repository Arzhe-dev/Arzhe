using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using Arzhe.Models;

namespace Arzhe.Services;

public static class SystemScanner
{
    public static Task<ScanResult> ScanAsync(Action<string, int>? onProgress = null)
    {
        // On execute le scan dans un Task.Run pour ne pas bloquer l'UI
        return Task.Run(() => ScanInternal(onProgress));
    }

    private static ScanResult ScanInternal(Action<string, int>? onProgress)
    {
        var result = new ScanResult();

        void Report(string msg, int pct) => onProgress?.Invoke(msg, pct);

        try
        {
            Report("Detection du processeur...", 10);
            result.Cpu = ScanCpu();
        }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[ARZHE] CPU err: {ex.Message}"); }

        try
        {
            Report("Detection du GPU...", 25);
            result.Gpu = ScanGpu();
        }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[ARZHE] GPU err: {ex.Message}"); }

        try
        {
            Report("Analyse de la memoire...", 40);
            result.Ram = ScanRam();
        }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[ARZHE] RAM err: {ex.Message}"); }

        try
        {
            Report("Analyse des disques...", 55);
            result.Disks = ScanDisks();
        }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[ARZHE] Disk err: {ex.Message}"); }

        try
        {
            Report("Analyse du systeme...", 70);
            result.Os = ScanOs();
            result.IsLaptop = DetectLaptop();
            result.SupportsHags = result.Os.IsWindows11;
            result.SupportsTimerResolution = result.Os.IsWindows11;
            result.HasVbsEnabled = CheckVbs();
        }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[ARZHE] OS err: {ex.Message}"); }

        try
        {
            Report("Detection des jeux...", 85);
            result.InstalledGames = DetectGames();
        }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[ARZHE] Games err: {ex.Message}"); }

        try
        {
            Report("Calcul du score...", 95);
            result.HealthScore = ComputeHealthScore(result);
            result.Warnings = GenerateWarnings(result);
        }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[ARZHE] Score err: {ex.Message}"); }

        Report("Analyse terminee", 100);
        return result;
    }

    private static CpuInfo ScanCpu()
    {
        var info = new CpuInfo();
        using var s = new ManagementObjectSearcher(
            "SELECT Name, NumberOfCores, NumberOfLogicalProcessors, MaxClockSpeed FROM Win32_Processor");
        foreach (var o in s.Get())
        {
            info.Name = o["Name"]?.ToString()?.Trim() ?? "Inconnu";
            info.Cores = Convert.ToInt32(o["NumberOfCores"] ?? 0);
            info.Threads = Convert.ToInt32(o["NumberOfLogicalProcessors"] ?? 0);
            info.MaxClockMhz = Convert.ToInt32(o["MaxClockSpeed"] ?? 0);
            var n = info.Name.ToLower();
            if (n.Contains("intel")) info.Vendor = "Intel";
            else if (n.Contains("amd") || n.Contains("ryzen")) info.Vendor = "AMD";
            else info.Vendor = "Autre";
            break;
        }
        return info;
    }

    private static GpuInfo ScanGpu()
    {
        var info = new GpuInfo();
        using var s = new ManagementObjectSearcher(
            "SELECT Name, AdapterRAM, DriverVersion FROM Win32_VideoController");
        foreach (var o in s.Get())
        {
            var name = o["Name"]?.ToString() ?? "Inconnu";
            if (string.IsNullOrEmpty(info.Name) || info.Name == "Inconnu"
                || (!name.ToLower().Contains("radeon graphics") && !name.ToLower().Contains("uhd graphics")))
            {
                info.Name = name;
                try { info.VramMb = Convert.ToInt64(o["AdapterRAM"] ?? 0) / 1024 / 1024; } catch { }
                info.DriverVersion = o["DriverVersion"]?.ToString() ?? "";
            }
        }
        var n2 = info.Name.ToLower();
        if (n2.Contains("nvidia") || n2.Contains("geforce") || n2.Contains("rtx") || n2.Contains("gtx"))
            info.Vendor = "NVIDIA";
        else if (n2.Contains("amd") || n2.Contains("radeon"))
            info.Vendor = "AMD";
        else if (n2.Contains("intel"))
            info.Vendor = "Intel";
        info.IsIntegrated = n2.Contains("radeon graphics") || n2.Contains("uhd graphics")
                         || n2.Contains("iris") || n2.Contains("vega");
        return info;
    }

    private static RamInfo ScanRam()
    {
        var info = new RamInfo();
        using var s = new ManagementObjectSearcher("SELECT Capacity, Speed FROM Win32_PhysicalMemory");
        int sticks = 0;
        foreach (var o in s.Get())
        {
            info.TotalMb += Convert.ToInt64(o["Capacity"] ?? 0) / 1024 / 1024;
            var sp = Convert.ToInt32(o["Speed"] ?? 0);
            if (sp > info.SpeedMhz) info.SpeedMhz = sp;
            sticks++;
        }
        info.Channels = sticks;
        info.IsLowEnd = info.TotalMb < 8192;
        return info;
    }

    private static List<DiskInfo> ScanDisks()
    {
        var list = new List<DiskInfo>();
        try
        {
            using var logS = new ManagementObjectSearcher(
                "SELECT DeviceID, Size, FreeSpace FROM Win32_LogicalDisk WHERE DriveType=3");
            using var physS = new ManagementObjectSearcher(
                "SELECT FriendlyName, MediaType, HealthStatus FROM Win32_DiskDrive");

            var physList = new List<(string Model, string Type, string Health)>();
            foreach (var o in physS.Get())
            {
                var model = o["FriendlyName"]?.ToString() ?? "Inconnu";
                var media = o["MediaType"]?.ToString() ?? "";
                var health = o["HealthStatus"]?.ToString() ?? "OK";
                var type = media.ToLower().Contains("ssd") ? "SSD" :
                           media.ToLower().Contains("fixed") ? "HDD" : "SSD";
                physList.Add((model, type, health));
            }

            int i = 0;
            foreach (var o in logS.Get())
            {
                var letter = o["DeviceID"]?.ToString() ?? "";
                var total = Convert.ToInt64(o["Size"] ?? 0) / 1024 / 1024 / 1024;
                var free = Convert.ToInt64(o["FreeSpace"] ?? 0) / 1024 / 1024 / 1024;
                var phys = i < physList.Count ? physList[i] : (Model: "Inconnu", Type: "SSD", Health: "OK");
                list.Add(new DiskInfo
                {
                    Letter = letter,
                    Model = phys.Model,
                    Type = phys.Type,
                    TotalGb = total,
                    FreeGb = free,
                    Health = phys.Health,
                    IsSystemDisk = letter == "C:"
                });
                i++;
            }
        }
        catch { }
        return list;
    }

    private static OsInfo ScanOs()
    {
        var info = new OsInfo();
        using var s = new ManagementObjectSearcher(
            "SELECT Caption, BuildNumber FROM Win32_OperatingSystem");
        foreach (var o in s.Get())
        {
            info.Name = o["Caption"]?.ToString()?.Replace("Microsoft ", "") ?? "";
            info.Build = o["BuildNumber"]?.ToString() ?? "";
            info.Edition = info.Name;
            var build = Convert.ToInt32(info.Build);
            info.IsWindows11 = build >= 22000;
            break;
        }
        return info;
    }

    private static bool DetectLaptop()
    {
        try
        {
            using var s = new ManagementObjectSearcher("SELECT ChassisTypes FROM Win32_SystemEnclosure");
            foreach (var o in s.Get())
            {
                var types = (ushort[])o["ChassisTypes"];
                return types.Any(t => t == 8 || t == 9 || t == 10 || t == 11
                                   || t == 12 || t == 14 || t == 18
                                   || t == 21 || t == 30 || t == 31 || t == 32);
            }
        }
        catch { }
        return false;
    }

    private static bool CheckVbs()
    {
        try
        {
            using var s = new ManagementObjectSearcher(
                @"root\Microsoft\Windows\DeviceGuard",
                "SELECT VirtualizationBasedSecurityStatus FROM Win32_DeviceGuard");
            foreach (var o in s.Get())
            {
                var status = Convert.ToInt32(o["VirtualizationBasedSecurityStatus"] ?? 0);
                return status == 1 || status == 2;
            }
        }
        catch { }
        return false;
    }

    private static List<string> DetectGames()
    {
        var games = new List<string>();
        var checks = new Dictionary<string, string[]>
        {
            ["Valorant"] = new[] { @"C:\Riot Games\VALORANT", @"D:\Riot Games\VALORANT" },
            ["Fortnite"] = new[] { @"C:\Program Files\Epic Games\Fortnite", @"D:\Program Files\Epic Games\Fortnite" },
            ["Roblox"]   = new[] { @"C:\Program Files (x86)\Roblox", @"C:\Program Files\Roblox" }
        };
        foreach (var kv in checks)
            if (kv.Value.Any(Directory.Exists))
                games.Add(kv.Key);
        return games;
    }

    private static int ComputeHealthScore(ScanResult s)
    {
        int score = 100;
        if (s.Ram.IsLowEnd) score -= 10;
        if (s.HasVbsEnabled) score -= 8;
        if (s.Cpu.IsLaptop) score -= 3;
        foreach (var d in s.Disks.Where(d => d.IsSystemDisk))
        {
            if (d.FreeGb < 15) score -= 15;
            else if (d.FreeGb < 30) score -= 5;
            if (d.Health != "Healthy" && d.Health != "OK" && d.Health != "Inconnu") score -= 10;
        }
        return Math.Max(0, Math.Min(100, score));
    }

    private static List<string> GenerateWarnings(ScanResult s)
    {
        var w = new List<string>();
        if (s.HasVbsEnabled) w.Add("VBS/HVCI actif");
        if (s.Ram.IsLowEnd) w.Add("RAM < 8 Go");
        if (s.Cpu.IsLaptop) w.Add("Laptop detecte");
        if (!s.Os.IsWindows11) w.Add("Windows 10 detecte");
        return w;
    }
}