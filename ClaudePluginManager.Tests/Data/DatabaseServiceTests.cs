using System.Data;
using ClaudePluginManager.Data;
using Dapper;
using Microsoft.Data.Sqlite;

namespace ClaudePluginManager.Tests.Data;

public class DatabaseServiceTests : IDisposable
{
    private readonly string _testDbPath;
    private readonly string _testDirectory;

    public DatabaseServiceTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"DatabaseServiceTests_{Guid.NewGuid()}");
        _testDbPath = Path.Combine(_testDirectory, "test.db");
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, recursive: true);
        }
    }

    [Fact]
    public void DatabasePath_ReturnsConfiguredPath()
    {
        var service = new DatabaseService(_testDbPath);

        Assert.Equal(_testDbPath, service.DatabasePath);
    }

    [Fact]
    public void CreateConnection_ReturnsOpenConnection()
    {
        Directory.CreateDirectory(_testDirectory);
        var service = new DatabaseService(_testDbPath);

        using var connection = service.CreateConnection();

        Assert.NotNull(connection);
        Assert.Equal(ConnectionState.Open, connection.State);
    }

    [Fact]
    public void CreateConnection_ReturnsNewInstanceEachTime()
    {
        Directory.CreateDirectory(_testDirectory);
        var service = new DatabaseService(_testDbPath);

        using var connection1 = service.CreateConnection();
        using var connection2 = service.CreateConnection();

        Assert.NotSame(connection1, connection2);
    }

    [Fact]
    public async Task InitializeAsync_CreatesDatabaseFile()
    {
        var service = new DatabaseService(_testDbPath);

        await service.InitializeAsync();

        Assert.True(File.Exists(_testDbPath));
    }

    [Fact]
    public async Task InitializeAsync_CreatesParentDirectory()
    {
        var nestedPath = Path.Combine(_testDirectory, "nested", "deep", "cache.db");
        var service = new DatabaseService(nestedPath);

        await service.InitializeAsync();

        Assert.True(Directory.Exists(Path.GetDirectoryName(nestedPath)));
        Assert.True(File.Exists(nestedPath));
    }

    [Fact]
    public async Task InitializeAsync_IsIdempotent()
    {
        var service = new DatabaseService(_testDbPath);

        await service.InitializeAsync();
        var firstModified = File.GetLastWriteTimeUtc(_testDbPath);

        await Task.Delay(10);
        await service.InitializeAsync();
        var secondModified = File.GetLastWriteTimeUtc(_testDbPath);

        Assert.Equal(firstModified, secondModified);
    }

    [Fact]
    public async Task InitializeAsync_IsThreadSafe()
    {
        var service = new DatabaseService(_testDbPath);
        var tasks = Enumerable.Range(0, 10).Select(_ => service.InitializeAsync());

        await Task.WhenAll(tasks);

        Assert.True(File.Exists(_testDbPath));
        var version = await service.GetSchemaVersionAsync();
        Assert.Equal(DatabaseSchema.CurrentVersion, version);
    }

    [Fact]
    public async Task InitializeAsync_CalledAfterInitialized_ReturnsImmediately()
    {
        // Arrange
        var service = new DatabaseService(_testDbPath);
        await service.InitializeAsync();

        // Act - call again after already initialized
        await service.InitializeAsync();
        await service.InitializeAsync();

        // Assert - still only one version entry
        using var connection = service.CreateConnection();
        var versionCount = await connection.QuerySingleAsync<int>(
            "SELECT COUNT(*) FROM SchemaVersion");
        Assert.Equal(1, versionCount);
    }

    [Fact]
    public async Task InitializeAsync_CreatesPluginsTable()
    {
        var service = new DatabaseService(_testDbPath);

        await service.InitializeAsync();

        using var connection = service.CreateConnection();
        var tableExists = await connection.QuerySingleAsync<int>(
            "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='Plugins'");
        Assert.Equal(1, tableExists);
    }

    [Fact]
    public async Task InitializeAsync_CreatesMarketplacesTable()
    {
        var service = new DatabaseService(_testDbPath);

        await service.InitializeAsync();

        using var connection = service.CreateConnection();
        var tableExists = await connection.QuerySingleAsync<int>(
            "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='Marketplaces'");
        Assert.Equal(1, tableExists);
    }

    [Fact]
    public async Task InitializeAsync_CreatesSyncHistoryTable()
    {
        var service = new DatabaseService(_testDbPath);

        await service.InitializeAsync();

        using var connection = service.CreateConnection();
        var tableExists = await connection.QuerySingleAsync<int>(
            "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='SyncHistory'");
        Assert.Equal(1, tableExists);
    }

    [Fact]
    public async Task GetSchemaVersionAsync_ReturnsCurrentVersion()
    {
        var service = new DatabaseService(_testDbPath);
        await service.InitializeAsync();

        var version = await service.GetSchemaVersionAsync();

        Assert.Equal(DatabaseSchema.CurrentVersion, version);
    }

    [Fact]
    public async Task PluginsTable_HasExpectedColumns()
    {
        var service = new DatabaseService(_testDbPath);
        await service.InitializeAsync();

        using var connection = service.CreateConnection();
        var columns = await connection.QueryAsync<string>(
            "SELECT name FROM pragma_table_info('Plugins')");
        var columnList = columns.ToList();

        Assert.Contains("Id", columnList);
        Assert.Contains("Name", columnList);
        Assert.Contains("Description", columnList);
        Assert.Contains("Version", columnList);
        Assert.Contains("Author", columnList);
        Assert.Contains("Repository", columnList);
        Assert.Contains("MarketplaceId", columnList);
        Assert.Contains("Type", columnList);
        Assert.Contains("Tags", columnList);
        Assert.Contains("Dependencies", columnList);
        Assert.Contains("ConfigSchema", columnList);
        Assert.Contains("Components", columnList);
        Assert.Contains("CachedAt", columnList);
    }

    [Fact]
    public async Task MarketplacesTable_HasExpectedColumns()
    {
        var service = new DatabaseService(_testDbPath);
        await service.InitializeAsync();

        using var connection = service.CreateConnection();
        var columns = await connection.QueryAsync<string>(
            "SELECT name FROM pragma_table_info('Marketplaces')");
        var columnList = columns.ToList();

        Assert.Contains("Id", columnList);
        Assert.Contains("Name", columnList);
        Assert.Contains("GitHubOwner", columnList);
        Assert.Contains("GitHubRepo", columnList);
        Assert.Contains("Enabled", columnList);
        Assert.Contains("Priority", columnList);
        Assert.Contains("RequiresAuth", columnList);
        Assert.Contains("LastSyncedAt", columnList);
        Assert.Contains("CreatedAt", columnList);
    }

    [Fact]
    public async Task SyncHistoryTable_HasExpectedColumns()
    {
        var service = new DatabaseService(_testDbPath);
        await service.InitializeAsync();

        using var connection = service.CreateConnection();
        var columns = await connection.QueryAsync<string>(
            "SELECT name FROM pragma_table_info('SyncHistory')");
        var columnList = columns.ToList();

        Assert.Contains("Id", columnList);
        Assert.Contains("MarketplaceId", columnList);
        Assert.Contains("SyncType", columnList);
        Assert.Contains("Status", columnList);
        Assert.Contains("StartedAt", columnList);
        Assert.Contains("CompletedAt", columnList);
        Assert.Contains("PluginCount", columnList);
        Assert.Contains("ErrorMessage", columnList);
    }

    [Fact]
    public async Task InitializeAsync_CreatesIndexOnPluginsMarketplaceId()
    {
        var service = new DatabaseService(_testDbPath);
        await service.InitializeAsync();

        using var connection = service.CreateConnection();
        var indexExists = await connection.QuerySingleAsync<int>(
            "SELECT COUNT(*) FROM sqlite_master WHERE type='index' AND name='IX_Plugins_MarketplaceId'");
        Assert.Equal(1, indexExists);
    }

    [Fact]
    public async Task InitializeAsync_CreatesIndexOnPluginsType()
    {
        var service = new DatabaseService(_testDbPath);
        await service.InitializeAsync();

        using var connection = service.CreateConnection();
        var indexExists = await connection.QuerySingleAsync<int>(
            "SELECT COUNT(*) FROM sqlite_master WHERE type='index' AND name='IX_Plugins_Type'");
        Assert.Equal(1, indexExists);
    }

    [Fact]
    public async Task InitializeAsync_CreatesIndexOnSyncHistoryMarketplaceId()
    {
        var service = new DatabaseService(_testDbPath);
        await service.InitializeAsync();

        using var connection = service.CreateConnection();
        var indexExists = await connection.QuerySingleAsync<int>(
            "SELECT COUNT(*) FROM sqlite_master WHERE type='index' AND name='IX_SyncHistory_MarketplaceId'");
        Assert.Equal(1, indexExists);
    }

    [Fact]
    public async Task InitializeAsync_CreatesInstalledPluginsTable()
    {
        var service = new DatabaseService(_testDbPath);

        await service.InitializeAsync();

        using var connection = service.CreateConnection();
        var tableExists = await connection.QuerySingleAsync<int>(
            "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='InstalledPlugins'");
        Assert.Equal(1, tableExists);
    }

    [Fact]
    public async Task InstalledPluginsTable_HasExpectedColumns()
    {
        var service = new DatabaseService(_testDbPath);
        await service.InitializeAsync();

        using var connection = service.CreateConnection();
        var columns = await connection.QueryAsync<string>(
            "SELECT name FROM pragma_table_info('InstalledPlugins')");
        var columnList = columns.ToList();

        Assert.Contains("Id", columnList);
        Assert.Contains("Name", columnList);
        Assert.Contains("Version", columnList);
        Assert.Contains("Type", columnList);
        Assert.Contains("MarketplaceId", columnList);
        Assert.Contains("ConfigSnapshot", columnList);
        Assert.Contains("InstalledAt", columnList);
    }

    [Fact]
    public async Task InitializeAsync_CreatesIndexOnInstalledPluginsType()
    {
        var service = new DatabaseService(_testDbPath);
        await service.InitializeAsync();

        using var connection = service.CreateConnection();
        var indexExists = await connection.QuerySingleAsync<int>(
            "SELECT COUNT(*) FROM sqlite_master WHERE type='index' AND name='IX_InstalledPlugins_Type'");
        Assert.Equal(1, indexExists);
    }
}
