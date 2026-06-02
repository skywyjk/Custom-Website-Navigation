using System.Text.Json;
using WebNavigator.Models;

namespace WebNavigator.Services;

public class ConfigService
{
    private static readonly string ConfigFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config");
    private static readonly string ConfigPath = Path.Combine(ConfigFolder, "config.json");
    private static readonly string LogoFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logos");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static AppConfig LoadConfig()
    {
        EnsureFoldersExist();

        if (!File.Exists(ConfigPath))
        {
            var defaultConfig = new AppConfig();
            SaveConfig(defaultConfig);
            return defaultConfig;
        }

        try
        {
            var json = File.ReadAllText(ConfigPath);
            return JsonSerializer.Deserialize<AppConfig>(json, JsonOptions) ?? new AppConfig();
        }
        catch
        {
            return new AppConfig();
        }
    }

    public static void SaveConfig(AppConfig config)
    {
        EnsureFoldersExist();
        config.LastModified = DateTime.Now;
        var json = JsonSerializer.Serialize(config, JsonOptions);
        File.WriteAllText(ConfigPath, json);
    }

    public static string CopyLogo(string sourcePath, string websiteName)
    {
        EnsureFoldersExist();

        if (!File.Exists(sourcePath))
            return string.Empty;

        var extension = Path.GetExtension(sourcePath);
        var safeFileName = GetSafeFileName(websiteName) + extension;
        var destPath = Path.Combine(LogoFolder, safeFileName);

        int counter = 1;
        while (File.Exists(destPath))
        {
            safeFileName = $"{GetSafeFileName(websiteName)}_{counter}{extension}";
            destPath = Path.Combine(LogoFolder, safeFileName);
            counter++;
        }

        File.Copy(sourcePath, destPath, true);
        return Path.Combine("logos", safeFileName);
    }

    public static void DeleteLogo(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath))
            return;

        var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }

    private static void EnsureFoldersExist()
    {
        if (!Directory.Exists(ConfigFolder))
            Directory.CreateDirectory(ConfigFolder);

        if (!Directory.Exists(LogoFolder))
            Directory.CreateDirectory(LogoFolder);
    }

    private static string GetSafeFileName(string name)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sb = new System.Text.StringBuilder(name.Length);
        foreach (var c in name)
        {
            sb.Append(invalidChars.Contains(c) ? '_' : c);
        }
        var safeName = sb.ToString();
        return string.IsNullOrEmpty(safeName) ? "website" : safeName;
    }

    public static string GetLogoFolder()
    {
        EnsureFoldersExist();
        return LogoFolder;
    }
}
