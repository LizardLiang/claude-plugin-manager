using System.Text.Json;
using ClaudePluginManager.Data;
using ClaudePluginManager.Models;
using ClaudePluginManager.Services;
using Dapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace ClaudePluginManager.Tests.Services;

public class MarketplaceServiceTests : IDisposable
{
    private readonly IDatabaseService _databaseService;
    private readonly IGitHubClient _gitHubClient;
    private readonly IMarketplaceService _marketplaceService;
    private readonly string _testDirectory;
    private readonly string _testDbPath;

    public MarketplaceServiceTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"MarketplaceServiceTests_{Guid.NewGuid()}");
        _testDbPath = Path.Combine(_testDirectory, "test.db");
        Directory.CreateDirectory(_testDirectory);

        _databaseService = new DatabaseService(_testDbPath);
        _databaseService.InitializeAsync().Wait();

        _gitHubClient = Substitute.For<IGitHubClient>();
        _marketplaceService = new MarketplaceService(_databaseService, _gitHubClient);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, recursive: true);
        }
    }

    private string CreateMarketplaceJson(params PluginEntry[] plugins)
    {
        var index = new MarketplaceIndex
        {
            Name = "Test Marketplace",
            Version = "1.0.0",
            Plugins = plugins.ToList()
        };
        return JsonSerializer.Serialize(index);
    }

    private async Task InsertTestMarketplace(string id = "official", string owner = "claude-market", string repo = "marketplace")
    {
        using var connection = _databaseService.CreateConnection();
        await connection.ExecuteAsync(
            """
            INSERT OR REPLACE INTO Marketplaces (Id, Name, GitHubOwner, GitHubRepo, Enabled, Priority, RequiresAuth, CreatedAt)
            VALUES (@Id, @Name, @Owner, @Repo, 1, 0, 0, @CreatedAt)
            """,
            new { Id = id, Name = "Test", Owner = owner, Repo = repo, CreatedAt = DateTime.UtcNow.ToString("O") });
    }

    private async Task InsertTestPlugin(string id, string name, string marketplaceId = "official")
    {
        using var connection = _databaseService.CreateConnection();
        await connection.ExecuteAsync(
            """
            INSERT OR REPLACE INTO Plugins (Id, Name, MarketplaceId, Type, CachedAt)
            VALUES (@Id, @Name, @MarketplaceId, 'MCP_SERVER', @CachedAt)
            """,
            new { Id = id, Name = name, MarketplaceId = marketplaceId, CachedAt = DateTime.UtcNow.ToString("O") });
    }

    #region GetPluginsAsync Tests

    [Fact]
    public async Task GetPluginsAsync_WhenCacheEmpty_FetchesFromGitHub()
    {
        // Arrange
        var marketplaceJson = CreateMarketplaceJson(
            new PluginEntry { Name = "test-plugin", Description = "A test plugin", Type = "MCP_SERVER" }
        );
        _gitHubClient.GetFileContentAsync("claude-market", "marketplace", ".claude-plugin/marketplace.json")
            .Returns(marketplaceJson);

        // Act
        var result = await _marketplaceService.GetPluginsAsync();

        // Assert
        await _gitHubClient.Received(1).GetFileContentAsync("claude-market", "marketplace", ".claude-plugin/marketplace.json");
        Assert.NotEmpty(result);
        Assert.Contains(result, p => p.Name == "test-plugin");
    }

    [Fact]
    public async Task GetPluginsAsync_WhenCacheExists_ReturnsCachedData()
    {
        // Arrange
        await InsertTestMarketplace();
        await InsertTestPlugin("official/cached-plugin", "cached-plugin");

        // Act
        var result = await _marketplaceService.GetPluginsAsync();

        // Assert
        await _gitHubClient.DidNotReceive().GetFileContentAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        Assert.Single(result);
        Assert.Equal("cached-plugin", result.First().Name);
    }

    [Fact]
    public async Task GetPluginsAsync_WhenGitHubFails_ThrowsException()
    {
        // Arrange
        _gitHubClient.GetFileContentAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns((string?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _marketplaceService.GetPluginsAsync());
    }

    [Fact]
    public async Task GetPluginsAsync_WhenMalformedJson_ThrowsException()
    {
        // Arrange
        _gitHubClient.GetFileContentAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns("{ invalid json }");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _marketplaceService.GetPluginsAsync());
    }

    #endregion

    #region SearchPluginsAsync Tests

    [Fact]
    public async Task SearchPluginsAsync_FiltersResultsByName()
    {
        // Arrange
        await InsertTestMarketplace();
        await InsertTestPlugin("official/mcp-server", "mcp-server");
        await InsertTestPlugin("official/other-plugin", "other-plugin");

        // Act
        var result = await _marketplaceService.SearchPluginsAsync("mcp");

        // Assert
        Assert.Single(result);
        Assert.Equal("mcp-server", result.First().Name);
    }

    [Fact]
    public async Task SearchPluginsAsync_FiltersResultsByDescription()
    {
        // Arrange
        await InsertTestMarketplace();
        using var connection = _databaseService.CreateConnection();
        await connection.ExecuteAsync(
            """
            INSERT OR REPLACE INTO Plugins (Id, Name, Description, MarketplaceId, Type, CachedAt)
            VALUES ('official/test', 'generic-name', 'This is a database plugin', 'official', 'MCP_SERVER', @CachedAt)
            """,
            new { CachedAt = DateTime.UtcNow.ToString("O") });

        // Act
        var result = await _marketplaceService.SearchPluginsAsync("database");

        // Assert
        Assert.Single(result);
        Assert.Equal("generic-name", result.First().Name);
    }

    [Fact]
    public async Task SearchPluginsAsync_IsCaseInsensitive()
    {
        // Arrange
        await InsertTestMarketplace();
        await InsertTestPlugin("official/MCP-Server", "MCP-Server");

        // Act
        var result = await _marketplaceService.SearchPluginsAsync("mcp");

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public async Task SearchPluginsAsync_WithEmptyQuery_ReturnsAllPlugins()
    {
        // Arrange
        await InsertTestMarketplace();
        await InsertTestPlugin("official/plugin1", "plugin1");
        await InsertTestPlugin("official/plugin2", "plugin2");

        // Act
        var result = await _marketplaceService.SearchPluginsAsync("");

        // Assert
        Assert.Equal(2, result.Count());
    }

    #endregion

    #region RefreshAsync Tests

    [Fact]
    public async Task RefreshAsync_ForcesFreshFetch()
    {
        // Arrange
        await InsertTestMarketplace();
        await InsertTestPlugin("official/old-plugin", "old-plugin");

        var marketplaceJson = CreateMarketplaceJson(
            new PluginEntry { Name = "new-plugin", Description = "Fresh from GitHub", Type = "MCP_SERVER" }
        );
        _gitHubClient.GetFileContentAsync("claude-market", "marketplace", ".claude-plugin/marketplace.json")
            .Returns(marketplaceJson);

        // Act
        await _marketplaceService.RefreshAsync();

        // Assert
        await _gitHubClient.Received(1).GetFileContentAsync("claude-market", "marketplace", ".claude-plugin/marketplace.json");
    }

    [Fact]
    public async Task RefreshAsync_UpdatesLastSyncTime()
    {
        // Arrange
        var marketplaceJson = CreateMarketplaceJson(
            new PluginEntry { Name = "test-plugin", Type = "MCP_SERVER" }
        );
        _gitHubClient.GetFileContentAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(marketplaceJson);

        // Act
        await _marketplaceService.RefreshAsync();
        var lastSync = _marketplaceService.GetLastSyncTime();

        // Assert
        Assert.NotNull(lastSync);
        Assert.True(lastSync.Value > DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public async Task RefreshAsync_UpdatesCacheWithNewPlugins()
    {
        // Arrange
        var marketplaceJson = CreateMarketplaceJson(
            new PluginEntry { Name = "plugin1", Type = "MCP_SERVER" },
            new PluginEntry { Name = "plugin2", Type = "HOOK" }
        );
        _gitHubClient.GetFileContentAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(marketplaceJson);

        // Act
        await _marketplaceService.RefreshAsync();
        var plugins = await _marketplaceService.GetPluginsAsync();

        // Assert
        Assert.Equal(2, plugins.Count());
        Assert.Contains(plugins, p => p.Name == "plugin1");
        Assert.Contains(plugins, p => p.Name == "plugin2");
    }

    [Fact]
    public async Task RefreshAsync_WhenGitHubFails_ThrowsButCachePreserved()
    {
        // Arrange
        await InsertTestMarketplace();
        await InsertTestPlugin("official/existing-plugin", "existing-plugin");
        _gitHubClient.GetFileContentAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns((string?)null);

        // Act - RefreshAsync throws but cache should be preserved
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _marketplaceService.RefreshAsync());

        // GetPluginsAsync should return cached plugins (doesn't call RefreshAsync if cache exists)
        var plugins = await _marketplaceService.GetPluginsAsync();

        // Assert
        Assert.Single(plugins);
        Assert.Equal("existing-plugin", plugins.First().Name);
    }

    #endregion

    #region GetLastSyncTime Tests

    [Fact]
    public void GetLastSyncTime_WhenNeverSynced_ReturnsNull()
    {
        // Act
        var result = _marketplaceService.GetLastSyncTime();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetLastSyncTime_ReturnsCorrectTime()
    {
        // Arrange
        var syncTime = DateTime.UtcNow.AddHours(-2);
        using var connection = _databaseService.CreateConnection();
        await connection.ExecuteAsync(
            """
            INSERT OR REPLACE INTO Marketplaces (Id, Name, GitHubOwner, GitHubRepo, Enabled, Priority, RequiresAuth, LastSyncedAt, CreatedAt)
            VALUES ('official', 'Official', 'claude-market', 'marketplace', 1, 0, 0, @SyncTime, @CreatedAt)
            """,
            new { SyncTime = syncTime.ToString("O"), CreatedAt = DateTime.UtcNow.ToString("O") });

        // Act
        var result = _marketplaceService.GetLastSyncTime();

        // Assert
        Assert.NotNull(result);
        Assert.True(Math.Abs((result.Value - syncTime).TotalSeconds) < 1);
    }

    [Fact]
    public async Task RefreshAsync_WhenDeserializationReturnsNull_ThrowsException()
    {
        // Arrange - JSON "null" deserializes to null object
        _gitHubClient.GetFileContentAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns("null");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _marketplaceService.RefreshAsync());
        Assert.Contains("empty or invalid", ex.Message);
    }

    [Fact]
    public async Task RefreshAsync_WhenPluginsExplicitlyNull_ThrowsException()
    {
        // Arrange - Plugins property is explicitly null in JSON
        _gitHubClient.GetFileContentAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns("{\"name\":\"test\",\"plugins\":null}");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _marketplaceService.RefreshAsync());
        Assert.Contains("empty or invalid", ex.Message);
    }

    [Fact]
    public async Task RefreshAsync_WithCompletelyInvalidJson_ThrowsException()
    {
        // Arrange - use string that will definitely throw JsonException
        _gitHubClient.GetFileContentAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns("not json at all {{{{");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _marketplaceService.RefreshAsync());
        Assert.Contains("invalid JSON format", ex.Message);
    }

    #endregion

    #region Plugin Parsing Tests

    [Fact]
    public async Task GetPluginsAsync_ParsesAllPluginFields()
    {
        // Arrange
        var plugin = new PluginEntry
        {
            Name = "complete-plugin",
            Description = "Full description",
            Version = "1.2.3",
            Author = "Test Author",
            Repository = "https://github.com/test/repo",
            Type = "MCP_SERVER",
            Tags = new List<string> { "database", "tool" }
        };
        var marketplaceJson = CreateMarketplaceJson(plugin);
        _gitHubClient.GetFileContentAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(marketplaceJson);

        // Act
        var result = (await _marketplaceService.GetPluginsAsync()).First();

        // Assert
        Assert.Equal("complete-plugin", result.Name);
        Assert.Equal("Full description", result.Description);
        Assert.Equal("1.2.3", result.Version);
        Assert.Equal("Test Author", result.Author);
        Assert.Equal("https://github.com/test/repo", result.Repository);
        Assert.Equal("MCP_SERVER", result.Type);
        Assert.Contains("database", result.Tags ?? "");
        Assert.Contains("tool", result.Tags ?? "");
    }

    [Fact]
    public async Task GetPluginsAsync_GeneratesCorrectPluginId()
    {
        // Arrange
        var marketplaceJson = CreateMarketplaceJson(
            new PluginEntry { Name = "my-plugin", Type = "MCP_SERVER" }
        );
        _gitHubClient.GetFileContentAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(marketplaceJson);

        // Act
        var result = (await _marketplaceService.GetPluginsAsync()).First();

        // Assert
        Assert.Equal("official/my-plugin", result.Id);
    }

    #endregion
}
