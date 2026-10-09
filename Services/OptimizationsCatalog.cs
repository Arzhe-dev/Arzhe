using System.Collections.Generic;
using Arzhe.Models;

namespace Arzhe.Services;

public static class OptimizationsCatalog
{
    public static List<Optimization> Build(ScanResult scan)
    {
        var list = new List<Optimization>();

        // PERFORMANCE
        list.Add(new Optimization { Id = "ultimate_performance", Title = "Plan Ultimate Performance",
            Description = "Plan d'alimentation haute performance avec reglages CPU raisonnables.",
            Icon = "PWR", Category = OptimizationCategory.Performance, Risk = RiskLevel.Safe,
            IsAvailable = _ => true, Action = (opt, _) => opt.ApplyUltimatePerformanceAsync() });

        list.Add(new Optimization { Id = "cpu_min_state", Title = "CPU minimum 100%",
            Description = "Force le CPU a frequence max. Deconseille sur laptop (chauffe).",
            Icon = "CPU", Category = OptimizationCategory.Performance, Risk = RiskLevel.Advanced,
            IsAvailable = s => !s.Cpu.IsLaptop, UnavailableReason = "Laptop detecte : surchauffe",
            Action = (opt, _) => opt.SetCpuMinStateAsync() });

        list.Add(new Optimization { Id = "timer_resolution", Title = "Timer Resolution 0.5ms",
            Description = "Reduit la latence des timers Windows. Windows 11 22H2+.",
            Icon = "TMR", Category = OptimizationCategory.Performance, Risk = RiskLevel.Safe,
            IsAvailable = s => s.SupportsTimerResolution, UnavailableReason = "Windows 11 requis",
            Action = (opt, _) => opt.SetTimerResolutionAsync() });

        list.Add(new Optimization { Id = "power_throttling", Title = "Desactiver Power Throttling",
            Description = "Empeche Windows de limiter le CPU en arriere-plan.",
            Icon = "THR", Category = OptimizationCategory.Performance, Risk = RiskLevel.Safe,
            IsAvailable = _ => true, Action = (opt, _) => opt.DisablePowerThrottlingAsync() });

        list.Add(new Optimization { Id = "memory_management", Title = "Gestion memoire optimisee",
            Description = "Ajuste les parametres de gestion memoire Windows.",
            Icon = "RAM", Category = OptimizationCategory.Performance, Risk = RiskLevel.Safe,
            IsAvailable = _ => true, Action = (opt, _) => opt.OptimizeMemoryAsync() });

        list.Add(new Optimization { Id = "large_system_cache", Title = "LargeSystemCache",
            Description = "Augmente le cache systeme. 16 Go RAM recommande.",
            Icon = "CAC", Category = OptimizationCategory.Performance, Risk = RiskLevel.Moderate,
            IsAvailable = s => s.Ram.TotalMb >= 16384, UnavailableReason = "16 Go RAM minimum",
            Action = (opt, _) => opt.EnableLargeSystemCacheAsync() });

        // GAMING
        list.Add(new Optimization { Id = "hags", Title = "HAGS (GPU Scheduling)",
            Description = "Reduit la latence GPU en laissant le GPU gerer sa memoire.",
            Icon = "HAG", Category = OptimizationCategory.Gaming, Risk = RiskLevel.Moderate,
            IsAvailable = s => s.SupportsHags, UnavailableReason = "Windows 11 + GPU compatible requis",
            Action = (opt, _) => opt.EnableHagsAsync() });

        list.Add(new Optimization { Id = "game_mode", Title = "Mode Jeu Windows",
            Description = "Prioritise les ressources pour le jeu en cours.",
            Icon = "GAM", Category = OptimizationCategory.Gaming, Risk = RiskLevel.Safe,
            IsAvailable = _ => true, Action = (opt, _) => opt.EnableGameModeAsync() });

        list.Add(new Optimization { Id = "game_dvr", Title = "Desactiver Game DVR",
            Description = "Supprime l'enregistrement en arriere-plan (-10% FPS).",
            Icon = "DVR", Category = OptimizationCategory.Gaming, Risk = RiskLevel.Safe,
            IsAvailable = _ => true, Action = (opt, _) => opt.DisableGameDvrAsync() });

        list.Add(new Optimization { Id = "gpu_preference", Title = "Preference GPU haute performance",
            Description = "Force Windows a utiliser le GPU dedie pour les jeux.",
            Icon = "GPU", Category = OptimizationCategory.Gaming, Risk = RiskLevel.Safe,
            IsAvailable = s => !s.Gpu.IsIntegrated, UnavailableReason = "GPU dedie requis",
            Action = (opt, _) => opt.SetGpuPreferenceAsync() });

        list.Add(new Optimization { Id = "nvidia_telemetry", Title = "Desactiver telemetrie NVIDIA",
            Description = "Supprime les services de tracking NVIDIA.",
            Icon = "NVT", Category = OptimizationCategory.Gaming, Risk = RiskLevel.Safe,
            IsAvailable = s => s.Gpu.Vendor == "NVIDIA", UnavailableReason = "GPU NVIDIA requis",
            Action = (opt, _) => opt.DisableNvidiaTelemetryAsync() });

        list.Add(new Optimization { Id = "amd_telemetry", Title = "Desactiver telemetrie AMD",
            Description = "Supprime les services AMD Crash Defender et External Events.",
            Icon = "AMD", Category = OptimizationCategory.Gaming, Risk = RiskLevel.Safe,
            IsAvailable = s => s.Gpu.Vendor == "AMD", UnavailableReason = "GPU AMD requis",
            Action = (opt, _) => opt.DisableAmdTelemetryAsync() });

        if (scan.InstalledGames.Contains("Valorant"))
            list.Add(new Optimization { Id = "valorant", Title = "Tweaks Valorant",
                Description = "Priorite GPU haute performance pour Valorant.",
                Icon = "VAL", Category = OptimizationCategory.Gaming, Risk = RiskLevel.Safe,
                IsAvailable = _ => true, Action = (opt, _) => opt.TweakValorantAsync() });

        if (scan.InstalledGames.Contains("Fortnite"))
            list.Add(new Optimization { Id = "fortnite", Title = "Tweaks Fortnite",
                Description = "Priorite GPU haute performance pour Fortnite.",
                Icon = "FTN", Category = OptimizationCategory.Gaming, Risk = RiskLevel.Safe,
                IsAvailable = _ => true, Action = (opt, _) => opt.TweakFortniteAsync() });

        if (scan.InstalledGames.Contains("Roblox"))
            list.Add(new Optimization { Id = "roblox", Title = "Tweaks Roblox",
                Description = "Priorite GPU haute performance pour Roblox.",
                Icon = "RBX", Category = OptimizationCategory.Gaming, Risk = RiskLevel.Safe,
                IsAvailable = _ => true, Action = (opt, _) => opt.TweakRobloxAsync() });

        // RESEAU
        list.Add(new Optimization { Id = "nagle", Title = "Desactiver Nagle",
            Description = "Reduit la latence reseau.",
            Icon = "LAG", Category = OptimizationCategory.Network, Risk = RiskLevel.Safe,
            IsAvailable = _ => true, Action = (opt, _) => opt.DisableNagleAsync() });

        list.Add(new Optimization { Id = "dns_cloudflare", Title = "DNS Cloudflare (1.1.1.1)",
            Description = "DNS plus rapides que ceux du FAI.",
            Icon = "DNS", Category = OptimizationCategory.Network, Risk = RiskLevel.Safe,
            IsAvailable = _ => true, Action = (opt, _) => opt.SetCloudflareDnsAsync() });

        list.Add(new Optimization { Id = "network_throttling", Title = "Supprimer Network Throttling",
            Description = "Empeche Windows de limiter le reseau multimedia.",
            Icon = "NTH", Category = OptimizationCategory.Network, Risk = RiskLevel.Safe,
            IsAvailable = _ => true, Action = (opt, _) => opt.DisableNetworkThrottlingAsync() });

        list.Add(new Optimization { Id = "tcp_optimization", Title = "Optimisation TCP",
            Description = "Ajuste les parametres TCP pour reduire la latence.",
            Icon = "TCP", Category = OptimizationCategory.Network, Risk = RiskLevel.Safe,
            IsAvailable = _ => true, Action = (opt, _) => opt.OptimizeTcpAsync() });

        // CONFIDENTIALITE
        list.Add(new Optimization { Id = "diagtrack", Title = "Desactiver telemetrie Windows",
            Description = "Supprime DiagTrack et dmwappushservice.",
            Icon = "TEL", Category = OptimizationCategory.Privacy, Risk = RiskLevel.Safe,
            IsAvailable = _ => true, Action = (opt, _) => opt.DisableDiagTrackAsync() });

        list.Add(new Optimization { Id = "onedrive", Title = "Desactiver OneDrive demarrage",
            Description = "Empeche OneDrive de ralentir le demarrage.",
            Icon = "1DV", Category = OptimizationCategory.Privacy, Risk = RiskLevel.Safe,
            IsAvailable = _ => true, Action = (opt, _) => opt.DisableOneDriveStartupAsync() });

        list.Add(new Optimization { Id = "copilot", Title = "Desactiver Copilot",
            Description = "Supprime l'assistant IA qui consomme des ressources.",
            Icon = "COP", Category = OptimizationCategory.Privacy, Risk = RiskLevel.Safe,
            IsAvailable = s => s.Os.IsWindows11, UnavailableReason = "Windows 11 requis",
            Action = (opt, _) => opt.DisableCopilotAsync() });

        // VISUEL
        list.Add(new Optimization { Id = "visual_effects", Title = "Effets visuels allegés",
            Description = "Reduit les animations Windows.",
            Icon = "VIS", Category = OptimizationCategory.Visual, Risk = RiskLevel.Safe,
            IsAvailable = _ => true, Action = (opt, _) => opt.ReduceVisualEffectsAsync() });

        list.Add(new Optimization { Id = "transparency", Title = "Desactiver transparence",
            Description = "Retire les effets de transparence.",
            Icon = "TRA", Category = OptimizationCategory.Visual, Risk = RiskLevel.Safe,
            IsAvailable = _ => true, Action = (opt, _) => opt.DisableTransparencyAsync() });

        // STOCKAGE
        list.Add(new Optimization { Id = "clean_temp", Title = "Nettoyage intelligent",
            Description = "Supprime Temp, Prefetch et caches Windows Update.",
            Icon = "CLN", Category = OptimizationCategory.Storage, Risk = RiskLevel.Safe,
            IsAvailable = _ => true, Action = (opt, _) => opt.CleanTempAsync() });

        list.Add(new Optimization { Id = "trim_nvme", Title = "Optimisation TRIM (SSD/NVMe)",
            Description = "Execute TRIM sur les volumes fixes.",
            Icon = "TRM", Category = OptimizationCategory.Storage, Risk = RiskLevel.Safe,
            IsAvailable = s => s.Disks.Exists(d => d.Type.Contains("SSD")),
            UnavailableReason = "Aucun SSD detecte",
            Action = (opt, _) => opt.OptimizeDisksAsync() });

        // PERIPHERIQUES
        list.Add(new Optimization { Id = "mouse_accel", Title = "Desactiver accel. souris",
            Description = "Supprime l'acceleration pour un viseur constant.",
            Icon = "MOU", Category = OptimizationCategory.Input, Risk = RiskLevel.Safe,
            IsAvailable = _ => true, Action = (opt, _) => opt.DisableMouseAccelAsync() });

        list.Add(new Optimization { Id = "usb_suspend", Title = "Suspension USB off",
            Description = "Evite les coupures de peripheriques en jeu.",
            Icon = "USB", Category = OptimizationCategory.Input, Risk = RiskLevel.Safe,
            IsAvailable = _ => true, Action = (opt, _) => opt.DisableUsbSuspendAsync() });

        // SERVICES
        list.Add(new Optimization { Id = "sysmain", Title = "Desactiver SysMain",
            Description = "Utile sur HDD, inutile sur SSD. Libere de la RAM.",
            Icon = "SYS", Category = OptimizationCategory.Services, Risk = RiskLevel.Moderate,
            IsAvailable = s => s.Disks.Exists(d => d.IsSystemDisk && d.Type.Contains("SSD")),
            UnavailableReason = "Utile seulement si SSD systeme",
            Action = (opt, _) => opt.DisableSysMainAsync() });

        list.Add(new Optimization { Id = "search_index", Title = "Desactiver Windows Search",
            Description = "Arrete l'indexation permanente des fichiers.",
            Icon = "WSH", Category = OptimizationCategory.Services, Risk = RiskLevel.Moderate,
            IsAvailable = _ => true, Action = (opt, _) => opt.DisableWindowsSearchAsync() });

        // AVANCE
        list.Add(new Optimization { Id = "vbs", Title = "Desactiver VBS/HVCI",
            Description = "Gain jusqu'a +5% FPS. Reduit la securite.",
            Icon = "VBS", Category = OptimizationCategory.Performance, Risk = RiskLevel.Advanced,
            IsAvailable = s => s.HasVbsEnabled, UnavailableReason = "VBS deja desactive",
            Action = (opt, _) => opt.DisableVbsAsync() });

        // Mise a jour disponibilite
        foreach (var opt in list)
        {
            bool available = true;
            try { available = opt.IsAvailable(scan); } catch { available = false; }
            opt.IsAvailableForPc = available;
            opt.IsSelected = available;
        }

        return list;
    }
}