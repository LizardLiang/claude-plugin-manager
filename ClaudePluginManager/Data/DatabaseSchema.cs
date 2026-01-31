namespace ClaudePluginManager.Data;

public static class DatabaseSchema
{
    public const int CurrentVersion = 2;

    public const string CreateTablesScript = """
        -- Schema version tracking
        CREATE TABLE IF NOT EXISTS SchemaVersion (
            Version INTEGER NOT NULL PRIMARY KEY
        );

        -- Plugins table
        -- Plugin IDs use format: marketplace/plugin-name
        -- Type values: MCP_SERVER, HOOK, SLASH_COMMAND, AGENT, SKILL
        -- JSON fields stored as TEXT
        CREATE TABLE IF NOT EXISTS Plugins (
            Id TEXT NOT NULL PRIMARY KEY,
            Name TEXT NOT NULL,
            Description TEXT,
            Version TEXT,
            Author TEXT,
            Repository TEXT,
            MarketplaceId TEXT NOT NULL,
            Type TEXT NOT NULL,
            Tags TEXT,
            Dependencies TEXT,
            ConfigSchema TEXT,
            Components TEXT,
            CachedAt TEXT NOT NULL
        );

        -- Marketplaces table
        CREATE TABLE IF NOT EXISTS Marketplaces (
            Id TEXT NOT NULL PRIMARY KEY,
            Name TEXT NOT NULL,
            GitHubOwner TEXT NOT NULL,
            GitHubRepo TEXT NOT NULL,
            Enabled INTEGER NOT NULL DEFAULT 1,
            Priority INTEGER NOT NULL DEFAULT 0,
            RequiresAuth INTEGER NOT NULL DEFAULT 0,
            LastSyncedAt TEXT,
            CreatedAt TEXT NOT NULL
        );

        -- Sync history table
        CREATE TABLE IF NOT EXISTS SyncHistory (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            MarketplaceId TEXT NOT NULL,
            SyncType TEXT NOT NULL,
            Status TEXT NOT NULL,
            StartedAt TEXT NOT NULL,
            CompletedAt TEXT,
            PluginCount INTEGER,
            ErrorMessage TEXT,
            FOREIGN KEY (MarketplaceId) REFERENCES Marketplaces(Id)
        );

        -- Installed plugins table (tracks plugins installed to Claude settings)
        CREATE TABLE IF NOT EXISTS InstalledPlugins (
            Id TEXT NOT NULL PRIMARY KEY,
            Name TEXT NOT NULL,
            Version TEXT,
            Type TEXT NOT NULL,
            MarketplaceId TEXT NOT NULL,
            ConfigSnapshot TEXT,
            InstalledAt TEXT NOT NULL
        );

        -- Indexes for common queries
        CREATE INDEX IF NOT EXISTS IX_Plugins_MarketplaceId ON Plugins(MarketplaceId);
        CREATE INDEX IF NOT EXISTS IX_Plugins_Type ON Plugins(Type);
        CREATE INDEX IF NOT EXISTS IX_SyncHistory_MarketplaceId ON SyncHistory(MarketplaceId);
        CREATE INDEX IF NOT EXISTS IX_InstalledPlugins_Type ON InstalledPlugins(Type);
        """;
}
