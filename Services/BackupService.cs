using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Arzhe.Services;

public class BackupService
{
    private static readonly string BackupDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Arzhe", "Backups");

    private static readonly string FilePath = Path.Combine(BackupDir, "backup.json");

    public Dictionary<string, string?> Values { get; private set; } = new();

    public BackupService()
    {
        try
        {
            Directory.CreateDirectory(BackupDir);
            if (File.Exists(FilePath))
            {
                var json = File.ReadAllText(FilePath);
                Values = JsonSerializer.Deserialize<Dictionary<string, string?>>(json) ?? new();
            }
        }
        catch { Values = new(); }
    }

    public void Save(string key, string? value)
    {
        if (!Values.ContainsKey(key))
        {
            Values[key] = value;
            Flush();
        }
    }

    public string? Restore(string key)
    {
        return Values.TryGetValue(key, out var v) ? v : null;
    }

    private void Flush()
    {
        try
        {
            var json = JsonSerializer.Serialize(Values, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FilePath, json);
        }
        catch { }
    }

    public static string GetBackupPath() => FilePath;
}