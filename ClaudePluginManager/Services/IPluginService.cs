using ClaudePluginManager.Models;

namespace ClaudePluginManager.Services;

public interface IPluginService
{
    Task<InstallResult> InstallGlobalAsync(Plugin plugin);
    Task<UninstallResult> UninstallGlobalAsync(string pluginId);
    Task<IReadOnlyList<InstalledPlugin>> GetInstalledGlobalAsync();
    Task<IReadOnlyList<InstalledPluginWithUpdate>> GetInstalledWithUpdatesAsync();
    Task<bool> IsInstalledAsync(string pluginId);
}
