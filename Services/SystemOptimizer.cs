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
    private readonly BackupService _backup = new();

    private void Emit(string msg) => Log?.Invoke(msg);

    public async Task<bool> CreateRestorePointAsync()
    {
        Emit("Creation d'un point de restauration systeme...");
        try
        {
            await Task.Run(() =>
            {
                var scope = new ManagementScope(@"\\.\root\default");
                scope.Connect();
                using var cls = new ManagementClass(scope,
                    new ManagementPath("SystemRestore"), null);
                var inParams = cls.GetMethodParameters("CreateRestorePoint");
                inParams["Description"] = "Arzhe - Avant optimisation";
                inParams["RestorePointType"] = 12;
                inParams["EventType"] = 100;
                var outParams = cls.InvokeMethod("CreateRestorePoint", inParams, null);
                var ret = (uint)(outParams?["ReturnValue"] ?? 0u);
                if (ret != 0)
                    throw new Exception($"WMI SystemRestore a retourne le code {ret}");
            });
            Emit("OK - Point de restauration cree");
            return true;
        }
        catch (Exception ex)
        {
            Emit("[!] Point de restauration impossible");
            Emit($"    Detail : {ex.Message}");
            Emit("    (La protection systeme est peut-etre desactivee)");
            return false;
        }
    }

    public Task ApplyAsync(string id) => id switch
    {
        "power"    => Task.Run(() => { Emit("Plan d'alimentation haute performance..."); ApplyUltimatePerformance(); }),
        "gamebar"  => Task.Run(() => { Emit("Game DVR / Game Bar..."); DisableGameDvr(); }),
        "visual"   => Task.Run(() => { Emit("Effets visuels..."); ReduceVisualEffects(); }),
        "temp"     => Task.Run(() => { Emit("Nettoyage temporaires..."); CleanTemp(); }),
        "services" => Task.Run(() => { Emit("Services non critiques..."); TweakServices(); }),
        "network"  => Task.Run(() => { Emit("Reseau (Nagle, DNS, throttling)..."); TweakNetwork(); }),
        "mouse"    => Task.Run(() => { Emit("Acceleration souris..."); DisableMouseAccel(); }),
        "usb"      => Task.Run(() => { Emit("Suspension USB..."); DisableUsbSuspend(); }),
        "gpu"      => Task.Run(() => { Emit("GPU Hardware Scheduling..."); EnableGpuScheduling(); }),
        "valorant" => Task.Run(() => { Emit("Valorant..."); TweakValorant(); }),
        "fortnite" => Task.Run(() => { Emit("Fortnite..."); TweakFortnite(); }),
        "roblox"   => Task.Run(() => { Emit("Roblox..."); TweakRoblox(); }),
        "disk"     => Task.Run(() => { Emit("Disques fixes..."); OptimizeDisk(); }),
        _ => Task.CompletedTask
    };

    private void ApplyUltimatePerformance()
    {
        RunPowerShell(@"
$guid = 'e9a42b02-d5df-448d-aa00-03f14749eb61'
$exists = (powercfg /list | Select-String $guid)
if (-not $exists) { powercfg -duplicatescheme $guid | Out-Null }
powercfg -SETACTIVE $guid
", "Plan Ultimate Performance applique.");
    }

    private void DisableGameDvr()
    {
        BackupReg(@"HKCU\Software\Microsoft\Windows\CurrentVersion\GameDVR", "AppCaptureEnabled");
        BackupReg(@"HKCU\System\GameConfigStore", "GameDVR_Enabled");
        RunReg(@"HKCU\Software\Microsoft\Windows\CurrentVersion\GameDVR", "AppCaptureEnabled", "REG_DWORD", "0");
        RunReg(@"HKCU\System\GameConfigStore", "GameDVR_Enabled", "REG_DWORD", "0");
        RunReg(@"HKLM\SOFTWARE\Policies\Microsoft\Windows\GameDVR", "AllowGameDVR", "REG_DWORD", "0");
        Emit("OK - Game DVR desactive");
    }

    private void ReduceVisualEffects()
    {
        BackupReg(@"HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\VisualEffects", "VisualFXSetting");
        RunReg(@"HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\VisualEffects",
                "VisualFXSetting", "REG_DWORD", "2");
        Emit("OK - Effets visuels allegés");
    }

    private void CleanTemp()
    {
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
    }

    private void TweakServices()
    {
        RunCmd("sc config DiagTrack start= disabled");
        RunCmd("sc stop DiagTrack");
        Emit("OK - Telemetrie DiagTrack desactivee");
    }

    private void TweakNetwork()
    {
        RunPowerShell(@"
Get-ChildItem 'HKLM:\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces' | ForEach-Object {
    Set-ItemProperty -Path $_.PSPath -Name 'TcpAckFrequency' -Value 1 -Type DWord -ErrorAction SilentlyContinue
    Set-ItemProperty -Path $_.PSPath -Name 'TCPNoDelay' -Value 1 -Type DWord -ErrorAction SilentlyContinue
}
", "Nagle desactive.");
        RunPowerShell(@"
Get-DnsClientServerAddress -AddressFamily IPv4 | Where-Object { $_.ServerAddresses -ne $null } | ForEach-Object {
    try { Set-DnsClientServerAddress -InterfaceIndex $_.InterfaceIndex -ServerAddresses ('1.1.1.1','1.0.0.1') } catch {}
}
", "DNS Cloudflare applique.");
        RunReg(@"HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile",
                "NetworkThrottlingIndex", "REG_DWORD", "ffffffff");
        RunReg(@"HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile",
                "SystemResponsiveness", "REG_DWORD", "10");
        Emit("OK - Network Throttling supprime");
    }

    private void DisableMouseAccel()
    {
        BackupReg(@"HKCU\Control Panel\Mouse", "MouseSpeed");
        BackupReg(@"HKCU\Control Panel\Mouse", "MouseThreshold1");
        BackupReg(@"HKCU\Control Panel\Mouse", "MouseThreshold2");
        RunReg(@"HKCU\Control Panel\Mouse", "MouseSpeed", "REG_SZ", "0");
        RunReg(@"HKCU\Control Panel\Mouse", "MouseThreshold1", "REG_SZ", "0");
        RunReg(@"HKCU\Control Panel\Mouse", "MouseThreshold2", "REG_SZ", "0");
        Emit("OK - Acceleration souris desactivee");
    }

    private void DisableUsbSuspend()
    {
        RunPowerShell(@"
powercfg -SETACVALUEINDEX SCHEME_CURRENT 2a737441-1930-4402-8d77-b2bebba308a3 48e6b7a6-50f5-4782-a5d4-53bb8f07e226 0
powercfg -SETDCVALUEINDEX SCHEME_CURRENT 2a737441-1930-4402-8d77-b2bebba308a3 48e6b7a6-50f5-4782-a5d4-53bb8f07e226 0
powercfg -S SCHEME_CURRENT
", "Suspension USB desactivee.");
    }

    private void EnableGpuScheduling()
    {
        BackupReg(@"HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "HwSchMode");
        RunReg(@"HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers",
                "HwSchMode", "REG_DWORD", "2");
        Emit("OK - GPU Hardware Scheduling active");
    }

    private void TweakValorant()
    {
        RunReg(@"HKCU\Software\Microsoft\DirectX\UserGpuPreferences",
                "VALORANT-Win64-Shipping.exe", "REG_SZ", "GpuPreference=2;");
        Emit("OK - Valorant : preference GPU haute performance");
    }

    private void TweakFortnite()
    {
        RunReg(@"HKCU\Software\Microsoft\DirectX\UserGpuPreferences",
                "FortniteClient-Win64-Shipping.exe", "REG_SZ", "GpuPreference=2;");
        Emit("OK - Fortnite : preference GPU haute performance");
    }

    private void TweakRoblox()
    {
        RunReg(@"HKCU\Software\Microsoft\DirectX\UserGpuPreferences",
                "RobloxPlayerBeta.exe", "REG_SZ", "GpuPreference=2;");
        Emit("OK - Roblox : preference GPU haute performance");
    }

    private void OptimizeDisk()
    {
        RunPowerShell(@"
Get-Volume | Where-Object { $_.DriveType -eq 'Fixed' -and $_.DriveLetter } | ForEach-Object {
    try { Optimize-Volume -DriveLetter $_.DriveLetter -ReTrim -ErrorAction SilentlyContinue } catch {}
}
", "Volumes fixes optimises (TRIM).");
    }

    // ----- Helpers -----

    private void BackupReg(string path, string name)
    {
        try
        {
            var psi = new ProcessStartInfo("reg", $"query \"{path}\" /v \"{name}\"")
            { CreateNoWindow = true, UseShellExecute = false, RedirectStandardOutput = true };
            var p = Process.Start(psi);
            if (p == null) return;
            var output = p.StandardOutput.ReadToEnd();
            p.WaitForExit(3000);
            var lines = output.Split('\n');
            foreach (var l in lines)
            {
                var parts = l.Trim().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 3 && parts[0].Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    _backup.Save($"{path}|{name}", parts[parts.Length - 1]);
                    return;
                }
            }
        }
        catch { }
    }

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
            if (successMsg != null) Emit("OK - " + successMsg);
        }
        catch (Exception ex) { Emit($"[!] PowerShell : {ex.Message}"); }
    }
}