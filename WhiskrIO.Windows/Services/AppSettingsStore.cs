using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using WhiskrIO.Windows.Models;

namespace WhiskrIO.Windows.Services;

public sealed class AppSettingsStore
{
    private readonly string _settingsPath;

    public AppSettingsStore()
    {
        var baseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WhiskrIO");
        Directory.CreateDirectory(baseDir);
        _settingsPath = Path.Combine(baseDir, "settings.json");
    }

    public AppSettings Load()
    {
        if (!File.Exists(_settingsPath))
        {
            var defaults = new AppSettings();
            Save(defaults);
            return defaults;
        }

        var json = File.ReadAllText(_settingsPath, Encoding.UTF8);
        var settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();

        if (!string.IsNullOrWhiteSpace(settings.GeminiApiKey))
        {
            settings.GeminiApiKey = Unprotect(settings.GeminiApiKey);
        }

        return settings;
    }

    public void Save(AppSettings settings)
    {
        var cloned = new AppSettings
        {
            GeminiApiKey = string.IsNullOrWhiteSpace(settings.GeminiApiKey)
                ? string.Empty
                : Protect(settings.GeminiApiKey),
            GeminiModel = settings.GeminiModel,
            HotkeyModifiers = settings.HotkeyModifiers,
            HotkeyVirtualKey = settings.HotkeyVirtualKey
        };

        var json = JsonSerializer.Serialize(cloned, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_settingsPath, json, Encoding.UTF8);
    }

    public string EnsureFileExists()
    {
        if (!File.Exists(_settingsPath))
        {
            Save(new AppSettings());
        }

        return _settingsPath;
    }

    private static string Protect(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        var protectedBytes = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
        return Convert.ToBase64String(protectedBytes);
    }

    private static string Unprotect(string value)
    {
        try
        {
            var bytes = Convert.FromBase64String(value);
            var plain = ProtectedData.Unprotect(bytes, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(plain);
        }
        catch
        {
            return string.Empty;
        }
    }
}
