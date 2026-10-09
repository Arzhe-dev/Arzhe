using System.Collections.Generic;

namespace Arzhe.Models;

public class ScanResult
{
    public CpuInfo Cpu { get; set; } = new();
    public GpuInfo Gpu { get; set; } = new();
    public RamInfo Ram { get; set; } = new();
    public List<DiskInfo> Disks { get; set; } = new();
    public OsInfo Os { get; set; } = new();
    public bool SupportsHags { get; set; }
    public bool SupportsTimerResolution { get; set; }
    public bool HasVbsEnabled { get; set; }
    public bool IsLaptop { get; set; }
    public List<string> InstalledGames { get; set; } = new();
    public int HealthScore { get; set; }
    public List<string> Warnings { get; set; } = new();
}

public class CpuInfo
{
    public string Name { get; set; } = "Inconnu";
    public string Vendor { get; set; } = "Inconnu";
    public int Cores { get; set; }
    public int Threads { get; set; }
    public int MaxClockMhz { get; set; }
    public bool IsLaptop { get; set; }
}

public class GpuInfo
{
    public string Name { get; set; } = "Inconnu";
    public string Vendor { get; set; } = "Inconnu";
    public long VramMb { get; set; }
    public string DriverVersion { get; set; } = "";
    public bool SupportsHags { get; set; }
    public bool IsIntegrated { get; set; }
}

public class RamInfo
{
    public long TotalMb { get; set; }
    public int SpeedMhz { get; set; }
    public int Channels { get; set; }
    public bool IsLowEnd { get; set; }
}

public class DiskInfo
{
    public string Letter { get; set; } = "";
    public string Model { get; set; } = "";
    public string Type { get; set; } = "";
    public long TotalGb { get; set; }
    public long FreeGb { get; set; }
    public string Health { get; set; } = "";
    public bool IsSystemDisk { get; set; }
}

public class OsInfo
{
    public string Name { get; set; } = "";
    public string Build { get; set; } = "";
    public string Edition { get; set; } = "";
    public bool IsWindows11 { get; set; }
}