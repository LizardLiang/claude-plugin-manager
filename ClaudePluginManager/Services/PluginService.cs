using System.Text.Json;
using ClaudePluginManager.Data;
using ClaudePluginManager.Helpers;
using ClaudePluginManager.Models;
using Dapper;

namespace ClaudePluginManager.Services;

public class PluginService : IPluginService
{
    private readonly IConfigService _configService;
    private readonly IDatabaseService _databaseService;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public PluginService(IConfigService configService, IDatabaseService databaseService)
    {
        _configService = configService;
        _databaseService = databaseService;
    }

    public async Task<InstallResult> InstallGlobalAsync(Plugin plugin)
    {
        // Check if already installed
        if (await IsInstalledAsync(plugin.Id))
        {
            return InstallResult.Fail($"Plugin '{plugin.Name}' is already installed.");
        }

        // Parse components
        if (string.IsNullOrEmpty(plugin.Components))
        {
            return InstallResult.Fail("Plugin has no components to install.");
        }

        PluginComponents? components;
        try
        {
            components = JsonSerializer.Deserialize<PluginComponents>(plugin.Components, JsonOptions);
        }
        catch (JsonException ex)
        {
            return InstallResult.Fail($"Failed to parse plugin components: {ex.Message}");
        }

        if (components == null)
        {
            return InstallResult.Fail("Plugin has no components to install.");
        }

        var pluginType = PluginTypeExtensions.FromDbString(plugin.Type);

        try
        {
            // Read current settings
            var settings = await _configService.ReadGlobalSettingsAsync();

            // Apply changes based on plugin type
            var configSnapshot = ApplyPluginComponents(settings, components, pluginType);

            // Write settings
            await _configService.WriteGlobalSettingsAsync(settings);

            // Track in database
            await TrackInstalledPluginAsync(plugin, pluginType, configSnapshot);

            return InstallResult.Ok();
        }
        catch (Exception ex)
        {
            // Attempt rollback
            await _configService.RestoreFromBackupAsync();
            return InstallResult.Fail($"Installation failed: {ex.Message}");
        }
    }

    public async Task<UninstallResult> UninstallGlobalAsync(string pluginId)
    {
        // Get installed plugin info
        using var connection = _databaseService.CreateConnection();
        var installedRow = await connection.QuerySingleOrDefaultAsync<InstalledPluginRow>(
            "SELECT * FROM InstalledPlugins WHERE Id = @Id",
            new { Id = pluginId });

        if (installedRow == null)
        {
            return UninstallResult.Fail($"Plugin '{pluginId}' is not installed.");
        }

        var pluginType = PluginTypeExtensions.FromDbString(installedRow.Type);

        try
        {
            // Read current settings
            var settings = await _configService.ReadGlobalSettingsAsync();

            // Remove components using the stored config snapshot
            if (!string.IsNullOrEmpty(installedRow.ConfigSnapshot))
            {
                var configSnapshot = JsonSerializer.Deserialize<ConfigSnapshot>(installedRow.ConfigSnapshot, JsonOptions);
                if (configSnapshot != null)
                {
                    RemovePluginComponents(settings, configSnapshot, pluginType);
                }
            }

            // Write settings
            await _configService.WriteGlobalSettingsAsync(settings);

            // Remove from database
            await connection.ExecuteAsync(
                "DELETE FROM InstalledPlugins WHERE Id = @Id",
                new { Id = pluginId });

            return UninstallResult.Ok();
        }
        catch (Exception ex)
        {
            return UninstallResult.Fail($"Uninstallation failed: {ex.Message}");
        }
    }

    public async Task<IReadOnlyList<InstalledPlugin>> GetInstalledGlobalAsync()
    {
        using var connection = _databaseService.CreateConnection();
        var rows = await connection.QueryAsync<InstalledPluginRow>(
            "SELECT * FROM InstalledPlugins ORDER BY InstalledAt DESC");

        return rows.Select(row => new InstalledPlugin
        {
            Id = row.Id,
            Name = row.Name,
            Version = row.Version,
            Type = PluginTypeExtensions.FromDbString(row.Type),
            MarketplaceId = row.MarketplaceId,
            ConfigSnapshot = row.ConfigSnapshot,
            InstalledAt = DateTime.Parse(row.InstalledAt)
        }).ToList();
    }

    public async Task<IReadOnlyList<InstalledPluginWithUpdate>> GetInstalledWithUpdatesAsync()
    {
        using var connection = _databaseService.CreateConnection();

        // LEFT JOIN to get available versions from marketplace cache
        var rows = await connection.QueryAsync<InstalledPluginWithVersionRow>(
            """
            SELECT
                i.Id, i.Name, i.Version, i.Type, i.MarketplaceId, i.ConfigSnapshot, i.InstalledAt,
                p.Version AS AvailableVersion
            FROM InstalledPlugins i
            LEFT JOIN Plugins p ON i.Id = p.Id
            ORDER BY i.InstalledAt DESC
            """);

        return rows.Select(row =>
        {
            var installedPlugin = new InstalledPlugin
            {
                Id = row.Id,
                Name = row.Name,
                Version = row.Version,
                Type = PluginTypeExtensions.FromDbString(row.Type),
                MarketplaceId = row.MarketplaceId,
                ConfigSnapshot = row.ConfigSnapshot,
                InstalledAt = DateTime.Parse(row.InstalledAt)
            };

            return new InstalledPluginWithUpdate
            {
                InstalledPlugin = installedPlugin,
                AvailableVersion = row.AvailableVersion,
                HasUpdate = VersionComparer.IsNewer(row.AvailableVersion, row.Version)
            };
        }).ToList();
    }

    public async Task<bool> IsInstalledAsync(string pluginId)
    {
        using var connection = _databaseService.CreateConnection();
        var count = await connection.QuerySingleAsync<int>(
            "SELECT COUNT(*) FROM InstalledPlugins WHERE Id = @Id",
            new { Id = pluginId });
        return count > 0;
    }

    private string ApplyPluginComponents(ClaudeSettings settings, PluginComponents components, PluginType pluginType)
    {
        var snapshot = new ConfigSnapshot();

        switch (pluginType)
        {
            case PluginType.McpServer:
                settings.McpServers ??= new Dictionary<string, McpServerConfig>();
                foreach (var (name, server) in components.McpServers)
                {
                    settings.McpServers[name] = server.ToMcpServerConfig();
                    snapshot.McpServerNames.Add(name);
                }
                break;

            case PluginType.Hook:
                // Future: implement hooks
                foreach (var (name, _) in components.Hooks)
                {
                    snapshot.HookNames.Add(name);
                }
                break;

            case PluginType.SlashCommand:
            case PluginType.Agent:
            case PluginType.Skill:
                // Future: implement via ExtensionData
                break;
        }

        return JsonSerializer.Serialize(snapshot);
    }

    private void RemovePluginComponents(ClaudeSettings settings, ConfigSnapshot snapshot, PluginType pluginType)
    {
        switch (pluginType)
        {
            case PluginType.McpServer:
                if (settings.McpServers != null)
                {
                    foreach (var name in snapshot.McpServerNames)
                    {
                        settings.McpServers.Remove(name);
                    }
                }
                break;

            case PluginType.Hook:
                // Future: implement hooks removal
                break;

            case PluginType.SlashCommand:
            case PluginType.Agent:
            case PluginType.Skill:
                // Future: implement via ExtensionData
                break;
        }
    }

    private async Task TrackInstalledPluginAsync(Plugin plugin, PluginType pluginType, string configSnapshot)
    {
        using var connection = _databaseService.CreateConnection();
        await connection.ExecuteAsync(
            """
            INSERT INTO InstalledPlugins (Id, Name, Version, Type, MarketplaceId, ConfigSnapshot, InstalledAt)
            VALUES (@Id, @Name, @Version, @Type, @MarketplaceId, @ConfigSnapshot, @InstalledAt)
            """,
            new
            {
                Id = plugin.Id,
                Name = plugin.Name,
                Version = plugin.Version,
                Type = pluginType.ToDbString(),
                MarketplaceId = plugin.MarketplaceId,
                ConfigSnapshot = configSnapshot,
                InstalledAt = DateTime.UtcNow.ToString("O")
            });
    }

    private class InstalledPluginRow
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Version { get; set; }
        public string Type { get; set; } = string.Empty;
        public string MarketplaceId { get; set; } = string.Empty;
        public string? ConfigSnapshot { get; set; }
        public string InstalledAt { get; set; } = string.Empty;
    }

    private class InstalledPluginWithVersionRow
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Version { get; set; }
        public string Type { get; set; } = string.Empty;
        public string MarketplaceId { get; set; } = string.Empty;
        public string? ConfigSnapshot { get; set; }
        public string InstalledAt { get; set; } = string.Empty;
        public string? AvailableVersion { get; set; }
    }

    private class ConfigSnapshot
    {
        public List<string> McpServerNames { get; set; } = new();
        public List<string> HookNames { get; set; } = new();
        public List<string> CommandNames { get; set; } = new();
        public List<string> AgentNames { get; set; } = new();
        public List<string> SkillNames { get; set; } = new();
    }
}
