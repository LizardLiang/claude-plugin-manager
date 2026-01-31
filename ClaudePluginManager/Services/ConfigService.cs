using System.Text.Json;
using ClaudePluginManager.Models;

namespace ClaudePluginManager.Services;

public class ConfigService : IConfigService
{
    private static readonly string DefaultBasePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private readonly SemaphoreSlim _fileLock = new(1, 1);
    private readonly string _basePath;

    public ConfigService() : this(DefaultBasePath)
    {
    }

    public ConfigService(string basePath)
    {
        _basePath = basePath;
    }

    public string GetGlobalSettingsPath()
    {
        return Path.Combine(_basePath, ".claude", "settings.json");
    }

    public async Task<ClaudeSettings> ReadGlobalSettingsAsync()
    {
        var settingsPath = GetGlobalSettingsPath();
        return await ReadSettingsAsync(settingsPath);
    }

    public async Task WriteGlobalSettingsAsync(ClaudeSettings settings)
    {
        var settingsPath = GetGlobalSettingsPath();
        await WriteSettingsAsync(settingsPath, settings);
    }

    public async Task<ClaudeSettings> ReadProjectSettingsAsync(string projectPath)
    {
        var settingsPath = Path.Combine(projectPath, ".claude", "settings.json");
        return await ReadSettingsAsync(settingsPath);
    }

    public async Task WriteProjectSettingsAsync(string projectPath, ClaudeSettings settings)
    {
        var settingsPath = Path.Combine(projectPath, ".claude", "settings.json");
        await WriteSettingsAsync(settingsPath, settings);
    }

    private async Task<ClaudeSettings> ReadSettingsAsync(string path)
    {
        await _fileLock.WaitAsync();
        try
        {
            if (!File.Exists(path))
            {
                return new ClaudeSettings();
            }

            var json = await File.ReadAllTextAsync(path);
            var settings = JsonSerializer.Deserialize<ClaudeSettings>(json, JsonOptions);
            return settings ?? new ClaudeSettings();
        }
        catch (JsonException)
        {
            return new ClaudeSettings();
        }
        finally
        {
            _fileLock.Release();
        }
    }

    private async Task WriteSettingsAsync(string path, ClaudeSettings settings)
    {
        await _fileLock.WaitAsync();
        try
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (File.Exists(path))
            {
                var backupPath = path + ".bak";
                File.Copy(path, backupPath, overwrite: true);
            }

            var json = JsonSerializer.Serialize(settings, JsonOptions);
            await File.WriteAllTextAsync(path, json);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async Task<bool> RestoreFromBackupAsync()
    {
        var settingsPath = GetGlobalSettingsPath();
        var backupPath = settingsPath + ".bak";

        await _fileLock.WaitAsync();
        try
        {
            if (!File.Exists(backupPath))
            {
                return false;
            }

            File.Copy(backupPath, settingsPath, overwrite: true);
            return true;
        }
        finally
        {
            _fileLock.Release();
        }
    }
}
