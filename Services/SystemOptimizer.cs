using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Management.Automation;
using System.Threading.Tasks;

namespace Arzhe.Services;

public class SystemOptimizer
{
    public event Action<string>? Log;
    private void Emit(string msg) => Log?.Invoke(msg);

    public async Task<bool> CreateRestorePointAsync()
    {
        Emit("Creation d'un point de restauration...");
        try
        {
            await Task.Run(() =>
            {
                var scope = new ManagementScope(@"\\.\root\default");
                scope.Connect();
                using var cls = new ManagementClass(scope, new ManagementPath("SystemRestore"), null);
                var inParams = cls.GetMethodParameters("CreateRestorePoint");
                inParams["Description"] = "Arzhe - Avant optimisation";
                inParams["RestorePointType"] = 12;
                inParams["EventType"] = 100;
                var outParams = cls.InvokeMethod("CreateRestorePoint", inParams, null);
                var ret = (uint)(outParams?["ReturnValue"] ?? 0u);
                if (ret != 0) throw new Exception($"Code {ret}");
            });
            Emit("OK - Point de restauration cree");
            return true;
        }
        catch (Exception ex)
        {
            Emit("[!] Point de restauration impossible : " + ex.Message);
            return false;
        }
    }

    // PERFORMANCE
    public Task ApplyUltimatePerformanceAsync()
    {
        Emit("Plan Ultimate Performance...");
        RunPowerShell(@"
$guid = 'e9a42b02-d5df-448d-aa00-03f14749eb61'
$exists = (powercfg /list | Select-String $guid)
if (-not $exists) { powercfg -duplicatescheme $guid | Out-Null }
powercfg -SETACTIVE $guid
", "OK - Plan Ultimate applique");
        return Task.CompletedTask;
    }

    public Task SetCpuMinStateAsync()
    {
        Emit("CPU minimum 100%...");
        RunCmd("powercfg -setacvalueindex SCHEME_CURRENT SUB_PROCESSOR PROCTHROTTLEMIN 100");
        RunCmd("powercfg -setdcvalueindex SCHEME_CURRENT SUB_PROCESSOR PROCTHROTTLEMIN 100");
        RunCmd("powercfg -setactive SCHEME_CURRENT");
        Emit("OK - CPU minimum 100%");
        return Task.CompletedTask;
    }

    public Task SetTimerResolutionAsync()
    {
        Emit("Timer Resolution...");
        RunCmd("bcdedit /set useplatformtick yes");
        RunCmd("bcdedit /set disabledynamictick yes");
        Emit("OK - Timer Resolution (redemarrage)");
        return Task.CompletedTask;
    }

    public Task DisablePowerThrottlingAsync()
    {
        Emit("Power Throttling...");
        RunReg(@"HKLM\SYSTEM\CurrentControlSet\Control\Power\PowerThrottling",
               "PowerThrottlingOff", "REG_DWORD", "1");
        Emit("OK - Power Throttling desactive");
        return Task.CompletedTask;
    }

    public Task OptimizeMemoryAsync()
    {
        Emit("Gestion memoire...");
        RunReg(@"HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management",
               "DisablePagingExecutive", "REG_DWORD", "1");
        Emit("OK - Memoire optimisee");
        return Task.CompletedTask;
    }

    public Task EnableLargeSystemCacheAsync()
    {
        Emit("LargeSystemCache...");
        RunReg(@"HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management",
               "LargeSystemCache", "REG_DWORD", "1");
        Emit("OK - LargeSystemCache active");
        return Task.CompletedTask;
    }

    // GAMING
    public Task EnableHagsAsync()
    {
        Emit("HAGS...");
        RunReg(@"HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers",
               "HwSchMode", "REG_DWORD", "2");
        Emit("OK - HAGS active (redemarrage)");
        return Task.CompletedTask;
    }

    public Task EnableGameModeAsync()
    {
        Emit("Mode Jeu...");
        RunReg(@"HKCU\Software\Microsoft\GameBar", "AutoGameModeEnabled", "REG_DWORD", "1");
        RunReg(@"HKCU\Software\Microsoft\GameBar", "AllowAutoGameMode", "REG_DWORD", "1");
        Emit("OK - Mode Jeu active");
        return Task.CompletedTask;
    }

    public Task DisableGameDvrAsync()
    {
        Emit("Game DVR...");
        RunReg(@"HKCU\Software\Microsoft\Windows\CurrentVersion\GameDVR", "AppCaptureEnabled", "REG_DWORD", "0");
        RunReg(@"HKCU\System\GameConfigStore", "GameDVR_Enabled", "REG_DWORD", "0");
        RunReg(@"HKLM\SOFTWARE\Policies\Microsoft\Windows\GameDVR", "AllowGameDVR", "REG_DWORD", "0");
        Emit("OK - Game DVR desactive");
        return Task.CompletedTask;
    }

    public Task SetGpuPreferenceAsync()
    {
        Emit("Preference GPU...");
        RunReg(@"HKCU\Software\Microsoft\DirectX\UserGpuPreferences",
               "DirectXUserGlobalSettings", "REG_SZ", "VRROptimizeEnable=0;");
        Emit("OK - Preference GPU definie");
        return Task.CompletedTask;
    }

    public Task DisableNvidiaTelemetryAsync()
    {
        Emit("Telemetrie NVIDIA...");
        RunCmd("sc stop NvTelemetryContainer");
        RunCmd("sc config NvTelemetryContainer start= disabled");
        Emit("OK - Telemetrie NVIDIA desactivee");
        return Task.CompletedTask;
    }

    public Task DisableAmdTelemetryAsync()
    {
        Emit("Telemetrie AMD...");
        RunCmd("sc stop AMD Crash Defender Service");
        RunCmd("sc config AMD Crash Defender Service start= disabled");
        RunCmd("sc stop AMD External Events Utility");
        RunCmd("sc config AMD External Events Utility start= disabled");
        Emit("OK - Telemetrie AMD desactivee");
        return Task.CompletedTask;
    }

    public Task TweakValorantAsync()
    {
        Emit("Tweaks Valorant...");
        RunReg(@"HKCU\Software\Microsoft\DirectX\UserGpuPreferences",
               "VALORANT-Win64-Shipping.exe", "REG_SZ", "GpuPreference=2;");
        Emit("OK - Valorant optimise");
        return Task.CompletedTask;
    }

    public Task TweakFortniteAsync()
    {
        Emit("Tweaks Fortnite...");
        RunReg(@"HKCU\Software\Microsoft\DirectX\UserGpuPreferences",
               "FortniteClient-Win64-Shipping.exe", "REG_SZ", "GpuPreference=2;");
        Emit("OK - Fortnite optimise");
        return Task.CompletedTask;
    }

    public Task TweakRobloxAsync()
    {
        Emit("Tweaks Roblox...");
        RunReg(@"HKCU\Software\Microsoft\DirectX\UserGpuPreferences",
               "RobloxPlayerBeta.exe", "REG_SZ", "GpuPreference=2;");
        Emit("OK - Roblox optimise");
        return Task.CompletedTask;
    }

    // RESEAU
    public Task DisableNagleAsync()
    {
        Emit("Nagle...");
        RunPowerShell(@"
Get-ChildItem 'HKLM:\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces' | ForEach-Object {
    Set-ItemProperty -Path $_.PSPath -Name 'TcpAckFrequency' -Value 1 -Type DWord -ErrorAction SilentlyContinue
    Set-ItemProperty -Path $_.PSPath -Name 'TCPNoDelay' -Value 1 -Type DWord -ErrorAction SilentlyContinue
    Set-ItemProperty -Path $_.PSPath -Name 'TcpDelAckTicks' -Value 0 -Type DWord -ErrorAction SilentlyContinue
}
", "OK - Nagle desactive");
        return Task.CompletedTask;
    }

    public Task SetCloudflareDnsAsync()
    {
        Emit("DNS Cloudflare...");
        RunPowerShell(@"
Get-DnsClientServerAddress -AddressFamily IPv4 | Where-Object { $_.ServerAddresses -ne $null } | ForEach-Object {
    try { Set-DnsClientServerAddress -InterfaceIndex $_.InterfaceIndex -ServerAddresses ('1.1.1.1','1.0.0.1') } catch {}
}
", "OK - DNS Cloudflare applique");
        return Task.CompletedTask;
    }

    public Task DisableNetworkThrottlingAsync()
    {
        Emit("Network Throttling...");
        RunReg(@"HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile",
               "NetworkThrottlingIndex", "REG_DWORD", "ffffffff");
        RunReg(@"HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile",
               "SystemResponsiveness", "REG_DWORD", "10");
        Emit("OK - Network Throttling supprime");
        return Task.CompletedTask;
    }

    public Task OptimizeTcpAsync()
    {
        Emit("Optimisation TCP...");
        RunCmd("netsh int tcp set global autotuninglevel=normal");
        RunCmd("netsh int tcp set global rss=enabled");
        RunCmd("netsh int tcp set heuristics disabled");
        Emit("OK - TCP optimise");
        return Task.CompletedTask;
    }

    // CONFIDENTIALITE
    public Task DisableDiagTrackAsync()
    {
        Emit("Telemetrie Windows...");
        RunCmd("sc stop DiagTrack");
        RunCmd("sc config DiagTrack start= disabled");
        RunCmd("sc stop dmwappushservice");
        RunCmd("sc config dmwappushservice start= disabled");
        RunReg(@"HKLM\SOFTWARE\Policies\Microsoft\Windows\DataCollection",
               "AllowTelemetry", "REG_DWORD", "0");
        Emit("OK - Telemetrie desactivee");
        return Task.CompletedTask;
    }

    public Task DisableOneDriveStartupAsync()
    {
        Emit("OneDrive...");
        RunReg(@"HKCU\Software\Microsoft\Windows\CurrentVersion\Run", "OneDrive", "REG_SZ", "");
        Emit("OK - OneDrive retire");
        return Task.CompletedTask;
    }

    public Task DisableCopilotAsync()
    {
        Emit("Copilot...");
        RunReg(@"HKCU\Software\Policies\Microsoft\Windows\WindowsCopilot",
               "TurnOffWindowsCopilot", "REG_DWORD", "1");
        Emit("OK - Copilot desactive");
        return Task.CompletedTask;
    }

    // VISUEL
    public Task ReduceVisualEffectsAsync()
    {
        Emit("Effets visuels...");
        RunReg(@"HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\VisualEffects",
               "VisualFXSetting", "REG_DWORD", "2");
        Emit("OK - Effets visuels allegés");
        return Task.CompletedTask;
    }

    public Task DisableTransparencyAsync()
    {
        Emit("Transparence...");
        RunReg(@"HKCU\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize",
               "EnableTransparency", "REG_DWORD", "0");
        Emit("OK - Transparence desactivee");
        return Task.CompletedTask;
    }

    // STOCKAGE
    public Task CleanTempAsync()
    {
        Emit("Nettoyage fichiers temporaires...");
        long freed = 0;
        string[] paths = { Path.GetTempPath(), @"C:\Windows\Temp", @"C:\Windows\Prefetch" };
        foreach (var dir in paths)
        {
            try
            {
                foreach (var f in Directory.EnumerateFiles(dir))
                {
                    try { freed += new FileInfo(f).Length; File.Delete(f); } catch { }
                }
            }
            catch { }
        }
        Emit($"OK - {freed / 1024 / 1024:F1} Mo liberes");
        return Task.CompletedTask;
    }

    public Task OptimizeDisksAsync()
    {
        Emit("Optimisation disques TRIM...");
        RunPowerShell(@"
Get-Volume | Where-Object { $_.DriveType -eq 'Fixed' -and $_.DriveLetter } | ForEach-Object {
    try { Optimize-Volume -DriveLetter $_.DriveLetter -ReTrim -ErrorAction SilentlyContinue } catch {}
}
", "OK - Volumes fixes optimises (TRIM)");
        return Task.CompletedTask;
    }

    // PERIPHERIQUES
    public Task DisableMouseAccelAsync()
    {
        Emit("Acceleration souris...");
        RunReg(@"HKCU\Control Panel\Mouse", "MouseSpeed", "REG_SZ", "0");
        RunReg(@"HKCU\Control Panel\Mouse", "MouseThreshold1", "REG_SZ", "0");
        RunReg(@"HKCU\Control Panel\Mouse", "MouseThreshold2", "REG_SZ", "0");
        Emit("OK - Acceleration souris desactivee");
        return Task.CompletedTask;
    }

    public Task DisableUsbSuspendAsync()
    {
        Emit("Suspension USB...");
        RunPowerShell(@"
powercfg -SETACVALUEINDEX SCHEME_CURRENT 2a737441-1930-4402-8d77-b2bebba308a3 48e6b7a6-50f5-4782-a5d4-53bb8f07e226 0
powercfg -SETDCVALUEINDEX SCHEME_CURRENT 2a737441-1930-4402-8d77-b2bebba308a3 48e6b7a6-50f5-4782-a5d4-53bb8f07e226 0
powercfg -S SCHEME_CURRENT
", "OK - Suspension USB desactivee");
        return Task.CompletedTask;
    }

    // SERVICES
    public Task DisableSysMainAsync()
    {
        Emit("SysMain...");
        RunCmd("sc stop SysMain");
        RunCmd("sc config SysMain start= disabled");
        Emit("OK - SysMain desactive");
        return Task.CompletedTask;
    }

    public Task DisableWindowsSearchAsync()
    {
        Emit("Windows Search...");
        RunCmd("sc stop WSearch");
        RunCmd("sc config WSearch start= disabled");
        Emit("OK - Windows Search desactive");
        return Task.CompletedTask;
    }

    // AVANCE
    public Task DisableVbsAsync()
    {
        Emit("VBS/HVCI...");
        RunCmd("bcdedit /set hypervisorlaunchtype off");
        RunReg(@"HKLM\SYSTEM\CurrentControlSet\Control\DeviceGuard",
               "EnableVirtualizationBasedSecurity", "REG_DWORD", "0");
        Emit("OK - VBS desactive (redemarrage)");
        return Task.CompletedTask;
    }

    // HELPERS
    private void RunReg(string path, string name, string type, string value)
    {
        try
        {
            var psi = new ProcessStartInfo("reg",
                $"add \"{path}\" /v \"{name}\" /t {type} /d \"{value}\" /f")
            { CreateNoWindow = true, UseShellExecute = false };
            Process.Start(psi)?.WaitForExit(5000);
        }
        catch (Exception ex) { Emit($"[!] reg : {ex.Message}"); }
    }

    private void RunCmd(string cmd)
    {
        try
        {
            var psi = new ProcessStartInfo("cmd.exe", "/c " + cmd)
            { CreateNoWindow = true, UseShellExecute = false };
            Process.Start(psi)?.WaitForExit(5000);
        }
        catch { }
    }

    private void RunPowerShell(string script, string? successMsg = null)
    {
        try
        {
            using var ps = PowerShell.Create();
            ps.AddScript(script);
            ps.Invoke();
            if (successMsg != null) Emit(successMsg);
        }
        catch (Exception ex) { Emit($"[!] PowerShell : {ex.Message}"); }
    }
}