using ClaudePluginManager.Models;

namespace ClaudePluginManager.Services;

public interface IConfigService
{
    Task<ClaudeSettings> ReadGlobalSettingsAsync();
    Task WriteGlobalSettingsAsync(ClaudeSettings settings);
    Task<ClaudeSettings> ReadProjectSettingsAsync(string projectPath);
    Task WriteProjectSettingsAsync(string projectPath, ClaudeSettings settings);
    string GetGlobalSettingsPath();
    Task<bool> RestoreFromBackupAsync();
}
