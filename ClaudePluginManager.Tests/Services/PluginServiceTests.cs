using System.Text.Json;
using ClaudePluginManager.Data;
using ClaudePluginManager.Models;
using ClaudePluginManager.Services;
using Dapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace ClaudePluginManager.Tests.Services;

public class PluginServiceTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly string _testDbPath;
    private readonly string _testClaudeDirectory;
    private readonly IDatabaseService _databaseService;
    private readonly ConfigService _configService;
    private readonly PluginService _pluginService;

    public PluginServiceTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"PluginServiceTests_{Guid.NewGuid()}");
        _testDbPath = Path.Combine(_testDirectory, "test.db");
        _testClaudeDirectory = Path.Combine(_testDirectory, ".claude");
        Directory.CreateDirectory(_testDirectory);

        _databaseService = new DatabaseService(_testDbPath);
        _configService = new ConfigService(_testDirectory);
        _pluginService = new PluginService(_configService, _databaseService);

        // Initialize database
        _databaseService.InitializeAsync().GetAwaiter().GetResult();
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, recursive: true);
        }
    }

    private Plugin CreateMcpServerPlugin(string id = "test/test-mcp-server", string name = "Test MCP Server")
    {
        var components = new PluginComponents
        {
            McpServers = new Dictionary<string, McpServerComponent>
            {
                ["test-server"] = new McpServerComponent
                {
                    Command = "npx",
                    Args = new List<string> { "-y", "@test/mcp-server" },
                    Env = new Dictionary<string, string> { ["API_KEY"] = "test-key" }
                }
            }
        };

        return new Plugin
        {
            Id = id,
            Name = name,
            Version = "1.0.0",
            Type = PluginType.McpServer.ToDbString(),
            MarketplaceId = "test-marketplace",
            Components = JsonSerializer.Serialize(components),
            CachedAt = DateTime.UtcNow
        };
    }

    private Plugin CreateHookPlugin(string id = "test/test-hook", string name = "Test Hook")
    {
        var components = new PluginComponents
        {
            Hooks = new Dictionary<string, HookComponent>
            {
                ["pre-tool-use"] = new HookComponent
                {
                    Matcher = "PreToolUse",
                    Script = "echo 'Running pre-tool hook'"
                }
            }
        };

        return new Plugin
        {
            Id = id,
            Name = name,
            Version = "1.0.0",
            Type = PluginType.Hook.ToDbString(),
            MarketplaceId = "test-marketplace",
            Components = JsonSerializer.Serialize(components),
            CachedAt = DateTime.UtcNow
        };
    }

    #region InstallGlobalAsync Tests

    [Fact]
    public async Task InstallGlobalAsync_McpServer_AddsToMcpServersInSettings()
    {
        // Arrange
        var plugin = CreateMcpServerPlugin();

        // Act
        var result = await _pluginService.InstallGlobalAsync(plugin);

        // Assert
        Assert.True(result.Success);
        var settings = await _configService.ReadGlobalSettingsAsync();
        Assert.NotNull(settings.McpServers);
        Assert.True(settings.McpServers.ContainsKey("test-server"));
        Assert.Equal("npx", settings.McpServers["test-server"].Command);
    }

    [Fact]
    public async Task InstallGlobalAsync_McpServer_TracksInDatabase()
    {
        // Arrange
        var plugin = CreateMcpServerPlugin();

        // Act
        var result = await _pluginService.InstallGlobalAsync(plugin);

        // Assert
        Assert.True(result.Success);
        using var connection = _databaseService.CreateConnection();
        var installed = await connection.QuerySingleOrDefaultAsync<InstalledPluginRow>(
            "SELECT * FROM InstalledPlugins WHERE Id = @Id",
            new { Id = plugin.Id });
        Assert.NotNull(installed);
        Assert.Equal(plugin.Name, installed.Name);
        Assert.Equal(plugin.Version, installed.Version);
        Assert.Equal(PluginType.McpServer.ToDbString(), installed.Type);
    }

    [Fact]
    public async Task InstallGlobalAsync_WhenAlreadyInstalled_ReturnsError()
    {
        // Arrange
        var plugin = CreateMcpServerPlugin();
        await _pluginService.InstallGlobalAsync(plugin);

        // Act
        var result = await _pluginService.InstallGlobalAsync(plugin);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("already installed", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task InstallGlobalAsync_WhenNoComponents_ReturnsError()
    {
        // Arrange
        var plugin = new Plugin
        {
            Id = "test/no-components",
            Name = "No Components",
            Type = PluginType.McpServer.ToDbString(),
            MarketplaceId = "test",
            Components = null,
            CachedAt = DateTime.UtcNow
        };

        // Act
        var result = await _pluginService.InstallGlobalAsync(plugin);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("no components", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task InstallGlobalAsync_PreservesExistingMcpServers()
    {
        // Arrange
        Directory.CreateDirectory(_testClaudeDirectory);
        var existingSettings = new ClaudeSettings
        {
            McpServers = new Dictionary<string, McpServerConfig>
            {
                ["existing-server"] = new McpServerConfig { Command = "existing" }
            }
        };
        await _configService.WriteGlobalSettingsAsync(existingSettings);

        var plugin = CreateMcpServerPlugin();

        // Act
        var result = await _pluginService.InstallGlobalAsync(plugin);

        // Assert
        Assert.True(result.Success);
        var settings = await _configService.ReadGlobalSettingsAsync();
        Assert.NotNull(settings.McpServers);
        Assert.True(settings.McpServers.ContainsKey("existing-server"));
        Assert.True(settings.McpServers.ContainsKey("test-server"));
    }

    [Fact]
    public async Task InstallGlobalAsync_McpServer_StoresConfigSnapshot()
    {
        // Arrange
        var plugin = CreateMcpServerPlugin();

        // Act
        var result = await _pluginService.InstallGlobalAsync(plugin);

        // Assert
        Assert.True(result.Success);
        using var connection = _databaseService.CreateConnection();
        var installed = await connection.QuerySingleOrDefaultAsync<InstalledPluginRow>(
            "SELECT * FROM InstalledPlugins WHERE Id = @Id",
            new { Id = plugin.Id });
        Assert.NotNull(installed);
        Assert.NotNull(installed.ConfigSnapshot);
        Assert.Contains("test-server", installed.ConfigSnapshot);
    }

    [Fact]
    public async Task InstallGlobalAsync_MultipleServers_AddsAllToSettings()
    {
        // Arrange
        var components = new PluginComponents
        {
            McpServers = new Dictionary<string, McpServerComponent>
            {
                ["server-one"] = new McpServerComponent { Command = "cmd1" },
                ["server-two"] = new McpServerComponent { Command = "cmd2" }
            }
        };
        var plugin = new Plugin
        {
            Id = "test/multi-server",
            Name = "Multi Server",
            Type = PluginType.McpServer.ToDbString(),
            MarketplaceId = "test",
            Components = JsonSerializer.Serialize(components),
            CachedAt = DateTime.UtcNow
        };

        // Act
        var result = await _pluginService.InstallGlobalAsync(plugin);

        // Assert
        Assert.True(result.Success);
        var settings = await _configService.ReadGlobalSettingsAsync();
        Assert.NotNull(settings.McpServers);
        Assert.True(settings.McpServers.ContainsKey("server-one"));
        Assert.True(settings.McpServers.ContainsKey("server-two"));
    }

    #endregion

    #region UninstallGlobalAsync Tests

    [Fact]
    public async Task UninstallGlobalAsync_RemovesFromSettings()
    {
        // Arrange
        var plugin = CreateMcpServerPlugin();
        await _pluginService.InstallGlobalAsync(plugin);

        // Verify it was installed
        var settingsBefore = await _configService.ReadGlobalSettingsAsync();
        Assert.True(settingsBefore.McpServers!.ContainsKey("test-server"));

        // Act
        var result = await _pluginService.UninstallGlobalAsync(plugin.Id);

        // Assert
        Assert.True(result.Success);
        var settings = await _configService.ReadGlobalSettingsAsync();
        Assert.False(settings.McpServers?.ContainsKey("test-server") ?? false);
    }

    [Fact]
    public async Task UninstallGlobalAsync_RemovesFromDatabase()
    {
        // Arrange
        var plugin = CreateMcpServerPlugin();
        await _pluginService.InstallGlobalAsync(plugin);

        // Act
        var result = await _pluginService.UninstallGlobalAsync(plugin.Id);

        // Assert
        Assert.True(result.Success);
        using var connection = _databaseService.CreateConnection();
        var installed = await connection.QuerySingleOrDefaultAsync<InstalledPluginRow>(
            "SELECT * FROM InstalledPlugins WHERE Id = @Id",
            new { Id = plugin.Id });
        Assert.Null(installed);
    }

    [Fact]
    public async Task UninstallGlobalAsync_WhenNotInstalled_ReturnsError()
    {
        // Act
        var result = await _pluginService.UninstallGlobalAsync("non-existent/plugin");

        // Assert
        Assert.False(result.Success);
        Assert.Contains("not installed", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UninstallGlobalAsync_PreservesOtherMcpServers()
    {
        // Arrange
        var plugin = CreateMcpServerPlugin();
        await _pluginService.InstallGlobalAsync(plugin);

        // Add another server directly
        var settings = await _configService.ReadGlobalSettingsAsync();
        settings.McpServers!["other-server"] = new McpServerConfig { Command = "other" };
        await _configService.WriteGlobalSettingsAsync(settings);

        // Act
        var result = await _pluginService.UninstallGlobalAsync(plugin.Id);

        // Assert
        Assert.True(result.Success);
        var finalSettings = await _configService.ReadGlobalSettingsAsync();
        Assert.True(finalSettings.McpServers!.ContainsKey("other-server"));
        Assert.False(finalSettings.McpServers.ContainsKey("test-server"));
    }

    [Fact]
    public async Task UninstallGlobalAsync_MultipleServers_RemovesAllFromSettings()
    {
        // Arrange
        var components = new PluginComponents
        {
            McpServers = new Dictionary<string, McpServerComponent>
            {
                ["server-one"] = new McpServerComponent { Command = "cmd1" },
                ["server-two"] = new McpServerComponent { Command = "cmd2" }
            }
        };
        var plugin = new Plugin
        {
            Id = "test/multi-server",
            Name = "Multi Server",
            Type = PluginType.McpServer.ToDbString(),
            MarketplaceId = "test",
            Components = JsonSerializer.Serialize(components),
            CachedAt = DateTime.UtcNow
        };
        await _pluginService.InstallGlobalAsync(plugin);

        // Act
        var result = await _pluginService.UninstallGlobalAsync(plugin.Id);

        // Assert
        Assert.True(result.Success);
        var settings = await _configService.ReadGlobalSettingsAsync();
        Assert.False(settings.McpServers?.ContainsKey("server-one") ?? false);
        Assert.False(settings.McpServers?.ContainsKey("server-two") ?? false);
    }

    #endregion

    #region GetInstalledGlobalAsync Tests

    [Fact]
    public async Task GetInstalledGlobalAsync_WhenNoPlugins_ReturnsEmptyList()
    {
        // Act
        var result = await _pluginService.GetInstalledGlobalAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetInstalledGlobalAsync_ReturnsAllInstalledPlugins()
    {
        // Arrange
        var plugin1 = CreateMcpServerPlugin("test/plugin1", "Plugin 1");
        var plugin2 = CreateMcpServerPlugin("test/plugin2", "Plugin 2");
        plugin2.Components = JsonSerializer.Serialize(new PluginComponents
        {
            McpServers = new Dictionary<string, McpServerComponent>
            {
                ["other-server"] = new McpServerComponent { Command = "other" }
            }
        });

        await _pluginService.InstallGlobalAsync(plugin1);
        await _pluginService.InstallGlobalAsync(plugin2);

        // Act
        var result = await _pluginService.GetInstalledGlobalAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, p => p.Id == "test/plugin1");
        Assert.Contains(result, p => p.Id == "test/plugin2");
    }

    [Fact]
    public async Task GetInstalledGlobalAsync_ReturnsCorrectPluginDetails()
    {
        // Arrange
        var plugin = CreateMcpServerPlugin();
        await _pluginService.InstallGlobalAsync(plugin);

        // Act
        var result = await _pluginService.GetInstalledGlobalAsync();

        // Assert
        Assert.Single(result);
        var installed = result[0];
        Assert.Equal(plugin.Id, installed.Id);
        Assert.Equal(plugin.Name, installed.Name);
        Assert.Equal(plugin.Version, installed.Version);
        Assert.Equal(PluginType.McpServer, installed.Type);
        Assert.Equal(plugin.MarketplaceId, installed.MarketplaceId);
        Assert.NotNull(installed.ConfigSnapshot);
    }

    #endregion

    #region IsInstalledAsync Tests

    [Fact]
    public async Task IsInstalledAsync_WhenInstalled_ReturnsTrue()
    {
        // Arrange
        var plugin = CreateMcpServerPlugin();
        await _pluginService.InstallGlobalAsync(plugin);

        // Act
        var result = await _pluginService.IsInstalledAsync(plugin.Id);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsInstalledAsync_WhenNotInstalled_ReturnsFalse()
    {
        // Act
        var result = await _pluginService.IsInstalledAsync("non-existent/plugin");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsInstalledAsync_AfterUninstall_ReturnsFalse()
    {
        // Arrange
        var plugin = CreateMcpServerPlugin();
        await _pluginService.InstallGlobalAsync(plugin);
        await _pluginService.UninstallGlobalAsync(plugin.Id);

        // Act
        var result = await _pluginService.IsInstalledAsync(plugin.Id);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region GetInstalledWithUpdatesAsync Tests

    [Fact]
    public async Task GetInstalledWithUpdatesAsync_WhenNoPlugins_ReturnsEmptyList()
    {
        // Act
        var result = await _pluginService.GetInstalledWithUpdatesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetInstalledWithUpdatesAsync_ReturnsAllInstalledPlugins()
    {
        // Arrange
        var plugin1 = CreateMcpServerPlugin("test/plugin1", "Plugin 1");
        var plugin2 = CreateMcpServerPlugin("test/plugin2", "Plugin 2");
        plugin2.Components = JsonSerializer.Serialize(new PluginComponents
        {
            McpServers = new Dictionary<string, McpServerComponent>
            {
                ["other-server"] = new McpServerComponent { Command = "other" }
            }
        });

        await _pluginService.InstallGlobalAsync(plugin1);
        await _pluginService.InstallGlobalAsync(plugin2);

        // Act
        var result = await _pluginService.GetInstalledWithUpdatesAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, p => p.InstalledPlugin.Id == "test/plugin1");
        Assert.Contains(result, p => p.InstalledPlugin.Id == "test/plugin2");
    }

    [Fact]
    public async Task GetInstalledWithUpdatesAsync_WhenNewerVersionInMarketplace_HasUpdateIsTrue()
    {
        // Arrange
        var plugin = CreateMcpServerPlugin();
        plugin.Version = "1.0.0";
        await _pluginService.InstallGlobalAsync(plugin);

        // Update the marketplace cache with a newer version
        await CachePluginInMarketplace(plugin.Id, "2.0.0");

        // Act
        var result = await _pluginService.GetInstalledWithUpdatesAsync();

        // Assert
        Assert.Single(result);
        Assert.True(result[0].HasUpdate);
        Assert.Equal("2.0.0", result[0].AvailableVersion);
    }

    [Fact]
    public async Task GetInstalledWithUpdatesAsync_WhenSameVersionInMarketplace_HasUpdateIsFalse()
    {
        // Arrange
        var plugin = CreateMcpServerPlugin();
        plugin.Version = "1.0.0";
        await _pluginService.InstallGlobalAsync(plugin);

        // Cache same version in marketplace
        await CachePluginInMarketplace(plugin.Id, "1.0.0");

        // Act
        var result = await _pluginService.GetInstalledWithUpdatesAsync();

        // Assert
        Assert.Single(result);
        Assert.False(result[0].HasUpdate);
    }

    [Fact]
    public async Task GetInstalledWithUpdatesAsync_WhenOlderVersionInMarketplace_HasUpdateIsFalse()
    {
        // Arrange
        var plugin = CreateMcpServerPlugin();
        plugin.Version = "2.0.0";
        await _pluginService.InstallGlobalAsync(plugin);

        // Cache older version in marketplace
        await CachePluginInMarketplace(plugin.Id, "1.0.0");

        // Act
        var result = await _pluginService.GetInstalledWithUpdatesAsync();

        // Assert
        Assert.Single(result);
        Assert.False(result[0].HasUpdate);
    }

    [Fact]
    public async Task GetInstalledWithUpdatesAsync_WhenPluginNotInMarketplace_HasUpdateIsFalse()
    {
        // Arrange
        var plugin = CreateMcpServerPlugin();
        plugin.Version = "1.0.0";
        await _pluginService.InstallGlobalAsync(plugin);
        // Don't cache anything in marketplace

        // Act
        var result = await _pluginService.GetInstalledWithUpdatesAsync();

        // Assert
        Assert.Single(result);
        Assert.False(result[0].HasUpdate);
        Assert.Null(result[0].AvailableVersion);
    }

    [Fact]
    public async Task GetInstalledWithUpdatesAsync_WhenInstalledVersionNull_HasUpdateIsFalse()
    {
        // Arrange
        var plugin = CreateMcpServerPlugin();
        plugin.Version = null;
        await _pluginService.InstallGlobalAsync(plugin);

        await CachePluginInMarketplace(plugin.Id, "2.0.0");

        // Act
        var result = await _pluginService.GetInstalledWithUpdatesAsync();

        // Assert
        Assert.Single(result);
        Assert.False(result[0].HasUpdate);
    }

    [Fact]
    public async Task GetInstalledWithUpdatesAsync_WhenMarketplaceVersionNull_HasUpdateIsFalse()
    {
        // Arrange
        var plugin = CreateMcpServerPlugin();
        plugin.Version = "1.0.0";
        await _pluginService.InstallGlobalAsync(plugin);

        await CachePluginInMarketplace(plugin.Id, null);

        // Act
        var result = await _pluginService.GetInstalledWithUpdatesAsync();

        // Assert
        Assert.Single(result);
        Assert.False(result[0].HasUpdate);
        Assert.Null(result[0].AvailableVersion);
    }

    [Fact]
    public async Task GetInstalledWithUpdatesAsync_MultiplePlugins_CorrectlyIdentifiesUpdates()
    {
        // Arrange
        var plugin1 = CreateMcpServerPlugin("test/plugin1", "Plugin 1");
        plugin1.Version = "1.0.0";
        var plugin2 = CreateMcpServerPlugin("test/plugin2", "Plugin 2");
        plugin2.Version = "2.0.0";
        plugin2.Components = JsonSerializer.Serialize(new PluginComponents
        {
            McpServers = new Dictionary<string, McpServerComponent>
            {
                ["server2"] = new McpServerComponent { Command = "cmd2" }
            }
        });

        await _pluginService.InstallGlobalAsync(plugin1);
        await _pluginService.InstallGlobalAsync(plugin2);

        // Plugin1 has update available, plugin2 does not
        await CachePluginInMarketplace("test/plugin1", "2.0.0");
        await CachePluginInMarketplace("test/plugin2", "2.0.0");

        // Act
        var result = await _pluginService.GetInstalledWithUpdatesAsync();

        // Assert
        Assert.Equal(2, result.Count);

        var pluginWithUpdate = result.First(p => p.InstalledPlugin.Id == "test/plugin1");
        Assert.True(pluginWithUpdate.HasUpdate);
        Assert.Equal("2.0.0", pluginWithUpdate.AvailableVersion);

        var pluginNoUpdate = result.First(p => p.InstalledPlugin.Id == "test/plugin2");
        Assert.False(pluginNoUpdate.HasUpdate);
    }

    private async Task CachePluginInMarketplace(string pluginId, string? version)
    {
        using var connection = _databaseService.CreateConnection();
        await connection.ExecuteAsync(
            """
            INSERT OR REPLACE INTO Plugins (Id, Name, Description, Version, Author, Repository, MarketplaceId, Type, Tags, Dependencies, ConfigSchema, Components, CachedAt)
            VALUES (@Id, @Name, '', @Version, '', '', 'test-marketplace', 'MCP_SERVER', '', '', '', '', @CachedAt)
            """,
            new
            {
                Id = pluginId,
                Name = "Test Plugin",
                Version = version,
                CachedAt = DateTime.UtcNow.ToString("O")
            });
    }

    #endregion

    // Helper class for Dapper mapping
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
}
