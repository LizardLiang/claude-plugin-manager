using ClaudePluginManager.Models;

namespace ClaudePluginManager.Services;

public interface IMarketplaceService
{
    Task<IEnumerable<Plugin>> GetPluginsAsync();
    Task<IEnumerable<Plugin>> SearchPluginsAsync(string query);
    Task RefreshAsync();
    DateTime? GetLastSyncTime();
}
