using System.Text.Json;
using ClaudePluginManager.Data;
using ClaudePluginManager.Models;
using Dapper;

namespace ClaudePluginManager.Services;

public class MarketplaceService : IMarketplaceService
{
    private const string DefaultMarketplaceId = "official";
    private const string DefaultGitHubOwner = "claude-market";
    private const string DefaultGitHubRepo = "marketplace";
    private const string MarketplaceIndexPath = ".claude-plugin/marketplace.json";

    private readonly IDatabaseService _databaseService;
    private readonly IGitHubClient _gitHubClient;

    public MarketplaceService(IDatabaseService databaseService, IGitHubClient gitHubClient)
    {
        _databaseService = databaseService;
        _gitHubClient = gitHubClient;
    }

    public async Task<IEnumerable<Plugin>> GetPluginsAsync()
    {
        var cachedPlugins = await GetCachedPluginsAsync();
        if (cachedPlugins.Any())
        {
            return cachedPlugins;
        }

        await RefreshAsync();
        return await GetCachedPluginsAsync();
    }

    public async Task<IEnumerable<Plugin>> SearchPluginsAsync(string query)
    {
        var plugins = await GetCachedPluginsAsync();

        if (string.IsNullOrWhiteSpace(query))
        {
            return plugins;
        }

        var lowerQuery = query.ToLowerInvariant();
        return plugins.Where(p =>
            p.Name.ToLowerInvariant().Contains(lowerQuery) ||
            (p.Description?.ToLowerInvariant().Contains(lowerQuery) ?? false));
    }

    public async Task RefreshAsync()
    {
        var content = await _gitHubClient.GetFileContentAsync(
            DefaultGitHubOwner,
            DefaultGitHubRepo,
            MarketplaceIndexPath);

        if (string.IsNullOrEmpty(content))
        {
            throw new InvalidOperationException(
                $"Could not fetch marketplace index from {DefaultGitHubOwner}/{DefaultGitHubRepo}");
        }

        MarketplaceIndex? marketplaceIndex;
        try
        {
            marketplaceIndex = JsonSerializer.Deserialize<MarketplaceIndex>(content);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Failed to parse marketplace index: invalid JSON format", ex);
        }

        if (marketplaceIndex?.Plugins == null)
        {
            throw new InvalidOperationException("Marketplace index is empty or invalid");
        }

        await EnsureMarketplaceExistsAsync();
        await SavePluginsAsync(marketplaceIndex.Plugins);
        await UpdateLastSyncTimeAsync();
    }

    public DateTime? GetLastSyncTime()
    {
        using var connection = _databaseService.CreateConnection();
        var lastSyncedAt = connection.QuerySingleOrDefault<string>(
            "SELECT LastSyncedAt FROM Marketplaces WHERE Id = @Id",
            new { Id = DefaultMarketplaceId });

        if (string.IsNullOrEmpty(lastSyncedAt))
        {
            return null;
        }

        return DateTime.Parse(lastSyncedAt, null, System.Globalization.DateTimeStyles.RoundtripKind);
    }

    private async Task<IEnumerable<Plugin>> GetCachedPluginsAsync()
    {
        using var connection = _databaseService.CreateConnection();
        var plugins = await connection.QueryAsync<Plugin>(
            "SELECT * FROM Plugins WHERE MarketplaceId = @MarketplaceId",
            new { MarketplaceId = DefaultMarketplaceId });
        return plugins;
    }

    private async Task EnsureMarketplaceExistsAsync()
    {
        using var connection = _databaseService.CreateConnection();
        await connection.ExecuteAsync(
            """
            INSERT OR IGNORE INTO Marketplaces (Id, Name, GitHubOwner, GitHubRepo, Enabled, Priority, RequiresAuth, CreatedAt)
            VALUES (@Id, @Name, @Owner, @Repo, 1, 0, 0, @CreatedAt)
            """,
            new
            {
                Id = DefaultMarketplaceId,
                Name = "Official Marketplace",
                Owner = DefaultGitHubOwner,
                Repo = DefaultGitHubRepo,
                CreatedAt = DateTime.UtcNow.ToString("O")
            });
    }

    private async Task SavePluginsAsync(List<PluginEntry> pluginEntries)
    {
        using var connection = _databaseService.CreateConnection();
        var cachedAt = DateTime.UtcNow.ToString("O");

        foreach (var entry in pluginEntries)
        {
            var plugin = new Plugin
            {
                Id = $"{DefaultMarketplaceId}/{entry.Name}",
                Name = entry.Name,
                Description = entry.Description,
                Version = entry.Version,
                Author = entry.Author?.Name,
                Repository = entry.Repository ?? entry.Source,
                MarketplaceId = DefaultMarketplaceId,
                Type = entry.Type,
                Tags = entry.Tags != null ? JsonSerializer.Serialize(entry.Tags) : null
            };

            await connection.ExecuteAsync(
                """
                INSERT OR REPLACE INTO Plugins (Id, Name, Description, Version, Author, Repository, MarketplaceId, Type, Tags, CachedAt)
                VALUES (@Id, @Name, @Description, @Version, @Author, @Repository, @MarketplaceId, @Type, @Tags, @CachedAt)
                """,
                new
                {
                    plugin.Id,
                    plugin.Name,
                    plugin.Description,
                    plugin.Version,
                    plugin.Author,
                    plugin.Repository,
                    plugin.MarketplaceId,
                    plugin.Type,
                    plugin.Tags,
                    CachedAt = cachedAt
                });
        }
    }

    private async Task UpdateLastSyncTimeAsync()
    {
        using var connection = _databaseService.CreateConnection();
        await connection.ExecuteAsync(
            "UPDATE Marketplaces SET LastSyncedAt = @SyncTime WHERE Id = @Id",
            new { Id = DefaultMarketplaceId, SyncTime = DateTime.UtcNow.ToString("O") });
    }
}
