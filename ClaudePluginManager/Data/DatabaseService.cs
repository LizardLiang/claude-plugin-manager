using System;
using System.Data;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;

namespace ClaudePluginManager.Data;

public class DatabaseService : IDatabaseService
{
    private static readonly string DefaultDatabasePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".claude-plugin-manager",
        "cache.db");

    private readonly SemaphoreSlim _initLock = new(1, 1);
    private bool _initialized;

    public string DatabasePath { get; }

    public DatabaseService() : this(DefaultDatabasePath)
    {
    }

    public DatabaseService(string databasePath)
    {
        DatabasePath = databasePath;
    }

    public IDbConnection CreateConnection()
    {
        var connection = new SqliteConnection($"Data Source={DatabasePath}");
        connection.Open();
        return connection;
    }

    public async Task InitializeAsync()
    {
        if (_initialized)
        {
            return;
        }

        await _initLock.WaitAsync();
        try
        {
            if (_initialized)
            {
                return;
            }

            var directory = Path.GetDirectoryName(DatabasePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using var connection = CreateConnection();
            await connection.ExecuteAsync(DatabaseSchema.CreateTablesScript);

            var versionExists = await connection.QuerySingleAsync<int>(
                "SELECT COUNT(*) FROM SchemaVersion WHERE Version = @Version",
                new { Version = DatabaseSchema.CurrentVersion });

            if (versionExists == 0)
            {
                await connection.ExecuteAsync(
                    "INSERT INTO SchemaVersion (Version) VALUES (@Version)",
                    new { Version = DatabaseSchema.CurrentVersion });
            }

            _initialized = true;
        }
        finally
        {
            _initLock.Release();
        }
    }

    public async Task<int> GetSchemaVersionAsync()
    {
        using var connection = CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<int>(
            "SELECT MAX(Version) FROM SchemaVersion");
    }
}
