using System.Text.Json;
using ClaudePluginManager.Models;
using ClaudePluginManager.Services;

namespace ClaudePluginManager.Tests.Services;

public class ConfigServiceTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly string _testClaudeDirectory;
    private readonly ConfigService _configService;

    public ConfigServiceTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"ConfigServiceTests_{Guid.NewGuid()}");
        _testClaudeDirectory = Path.Combine(_testDirectory, ".claude");
        Directory.CreateDirectory(_testDirectory);

        _configService = new ConfigService(_testDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, recursive: true);
        }
    }

    #region ReadGlobalSettingsAsync Tests

    [Fact]
    public async Task ReadGlobalSettingsAsync_WhenFileDoesNotExist_ReturnsEmptySettings()
    {
        // Act
        var result = await _configService.ReadGlobalSettingsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.McpServers);
        Assert.Null(result.Hooks);
    }

    [Fact]
    public async Task ReadGlobalSettingsAsync_ParsesValidJson()
    {
        // Arrange
        Directory.CreateDirectory(_testClaudeDirectory);
        var settingsPath = Path.Combine(_testClaudeDirectory, "settings.json");
        var json = """
            {
                "mcpServers": {
                    "test-server": {
                        "command": "node",
                        "args": ["server.js"],
                        "env": { "KEY": "value" }
                    }
                },
                "hooks": ["pre-commit", "post-commit"]
            }
            """;
        await File.WriteAllTextAsync(settingsPath, json);

        // Act
        var result = await _configService.ReadGlobalSettingsAsync();

        // Assert
        Assert.NotNull(result.McpServers);
        Assert.Single(result.McpServers);
        Assert.True(result.McpServers.ContainsKey("test-server"));
        var server = result.McpServers["test-server"];
        Assert.Equal("node", server.Command);
        Assert.NotNull(server.Args);
        Assert.Single(server.Args);
        Assert.Equal("server.js", server.Args[0]);
        Assert.NotNull(server.Env);
        Assert.Equal("value", server.Env["KEY"]);
        Assert.NotNull(result.Hooks);
        Assert.Equal(2, result.Hooks.Count);
    }

    [Fact]
    public async Task ReadGlobalSettingsAsync_HandlesMalformedJson_ReturnsEmptySettings()
    {
        // Arrange
        Directory.CreateDirectory(_testClaudeDirectory);
        var settingsPath = Path.Combine(_testClaudeDirectory, "settings.json");
        await File.WriteAllTextAsync(settingsPath, "{ invalid json }}}");

        // Act
        var result = await _configService.ReadGlobalSettingsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.McpServers);
    }

    [Fact]
    public async Task ReadGlobalSettingsAsync_PreservesUnknownProperties()
    {
        // Arrange
        Directory.CreateDirectory(_testClaudeDirectory);
        var settingsPath = Path.Combine(_testClaudeDirectory, "settings.json");
        var json = """
            {
                "mcpServers": {},
                "unknownProperty": "preserved",
                "anotherUnknown": { "nested": true }
            }
            """;
        await File.WriteAllTextAsync(settingsPath, json);

        // Act
        var result = await _configService.ReadGlobalSettingsAsync();

        // Assert
        Assert.NotNull(result.ExtensionData);
        Assert.True(result.ExtensionData.ContainsKey("unknownProperty"));
        Assert.True(result.ExtensionData.ContainsKey("anotherUnknown"));
    }

    #endregion

    #region WriteGlobalSettingsAsync Tests

    [Fact]
    public async Task WriteGlobalSettingsAsync_CreatesDirectoryIfMissing()
    {
        // Arrange
        var settings = new ClaudeSettings
        {
            McpServers = new Dictionary<string, McpServerConfig>
            {
                ["test"] = new McpServerConfig { Command = "echo" }
            }
        };

        // Act
        await _configService.WriteGlobalSettingsAsync(settings);

        // Assert
        Assert.True(Directory.Exists(_testClaudeDirectory));
    }

    [Fact]
    public async Task WriteGlobalSettingsAsync_CreatesFileWithProperFormatting()
    {
        // Arrange
        var settings = new ClaudeSettings
        {
            McpServers = new Dictionary<string, McpServerConfig>
            {
                ["my-server"] = new McpServerConfig
                {
                    Command = "python",
                    Args = new List<string> { "-m", "server" }
                }
            }
        };

        // Act
        await _configService.WriteGlobalSettingsAsync(settings);

        // Assert
        var settingsPath = Path.Combine(_testClaudeDirectory, "settings.json");
        Assert.True(File.Exists(settingsPath));
        var content = await File.ReadAllTextAsync(settingsPath);
        Assert.Contains("\"mcpServers\"", content);
        Assert.Contains("\"my-server\"", content);
        Assert.Contains("\n", content); // Check for proper formatting (indentation)
    }

    [Fact]
    public async Task WriteGlobalSettingsAsync_CreatesBackupOfExistingFile()
    {
        // Arrange
        Directory.CreateDirectory(_testClaudeDirectory);
        var settingsPath = Path.Combine(_testClaudeDirectory, "settings.json");
        var originalContent = """{"mcpServers": {"original": {"command": "old"}}}""";
        await File.WriteAllTextAsync(settingsPath, originalContent);

        var settings = new ClaudeSettings
        {
            McpServers = new Dictionary<string, McpServerConfig>
            {
                ["new"] = new McpServerConfig { Command = "new" }
            }
        };

        // Act
        await _configService.WriteGlobalSettingsAsync(settings);

        // Assert
        var backupPath = Path.Combine(_testClaudeDirectory, "settings.json.bak");
        Assert.True(File.Exists(backupPath));
        var backupContent = await File.ReadAllTextAsync(backupPath);
        Assert.Equal(originalContent, backupContent);
    }

    [Fact]
    public async Task WriteGlobalSettingsAsync_PreservesUnknownProperties()
    {
        // Arrange
        Directory.CreateDirectory(_testClaudeDirectory);
        var settingsPath = Path.Combine(_testClaudeDirectory, "settings.json");
        var originalJson = """
            {
                "mcpServers": {},
                "customProperty": "should-be-preserved"
            }
            """;
        await File.WriteAllTextAsync(settingsPath, originalJson);

        // Read, modify, and write back
        var settings = await _configService.ReadGlobalSettingsAsync();
        settings.McpServers!["new-server"] = new McpServerConfig { Command = "test" };

        // Act
        await _configService.WriteGlobalSettingsAsync(settings);

        // Assert
        var newContent = await File.ReadAllTextAsync(settingsPath);
        Assert.Contains("customProperty", newContent);
        Assert.Contains("should-be-preserved", newContent);
        Assert.Contains("new-server", newContent);
    }

    #endregion

    #region ReadProjectSettingsAsync Tests

    [Fact]
    public async Task ReadProjectSettingsAsync_WhenFileDoesNotExist_ReturnsEmptySettings()
    {
        // Arrange
        var projectPath = Path.Combine(_testDirectory, "project");
        Directory.CreateDirectory(projectPath);

        // Act
        var result = await _configService.ReadProjectSettingsAsync(projectPath);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.McpServers);
    }

    [Fact]
    public async Task ReadProjectSettingsAsync_ParsesValidJson()
    {
        // Arrange
        var projectPath = Path.Combine(_testDirectory, "project");
        var projectClaudeDir = Path.Combine(projectPath, ".claude");
        Directory.CreateDirectory(projectClaudeDir);
        var json = """
            {
                "mcpServers": {
                    "project-server": {
                        "command": "dotnet",
                        "args": ["run"]
                    }
                }
            }
            """;
        await File.WriteAllTextAsync(Path.Combine(projectClaudeDir, "settings.json"), json);

        // Act
        var result = await _configService.ReadProjectSettingsAsync(projectPath);

        // Assert
        Assert.NotNull(result.McpServers);
        Assert.True(result.McpServers.ContainsKey("project-server"));
    }

    #endregion

    #region WriteProjectSettingsAsync Tests

    [Fact]
    public async Task WriteProjectSettingsAsync_CreatesDirectoryIfMissing()
    {
        // Arrange
        var projectPath = Path.Combine(_testDirectory, "new-project");
        var settings = new ClaudeSettings
        {
            McpServers = new Dictionary<string, McpServerConfig>
            {
                ["test"] = new McpServerConfig { Command = "echo" }
            }
        };

        // Act
        await _configService.WriteProjectSettingsAsync(projectPath, settings);

        // Assert
        var projectClaudeDir = Path.Combine(projectPath, ".claude");
        Assert.True(Directory.Exists(projectClaudeDir));
        Assert.True(File.Exists(Path.Combine(projectClaudeDir, "settings.json")));
    }

    [Fact]
    public async Task WriteProjectSettingsAsync_CreatesBackupOfExistingFile()
    {
        // Arrange
        var projectPath = Path.Combine(_testDirectory, "backup-project");
        var projectClaudeDir = Path.Combine(projectPath, ".claude");
        Directory.CreateDirectory(projectClaudeDir);
        var settingsPath = Path.Combine(projectClaudeDir, "settings.json");
        var originalContent = """{"mcpServers": {}}""";
        await File.WriteAllTextAsync(settingsPath, originalContent);

        var settings = new ClaudeSettings
        {
            McpServers = new Dictionary<string, McpServerConfig>
            {
                ["new"] = new McpServerConfig { Command = "new" }
            }
        };

        // Act
        await _configService.WriteProjectSettingsAsync(projectPath, settings);

        // Assert
        var backupPath = Path.Combine(projectClaudeDir, "settings.json.bak");
        Assert.True(File.Exists(backupPath));
    }

    #endregion

    #region GetGlobalSettingsPath Tests

    [Fact]
    public void GetGlobalSettingsPath_ReturnsCorrectPath()
    {
        // Act
        var result = _configService.GetGlobalSettingsPath();

        // Assert
        var expected = Path.Combine(_testDirectory, ".claude", "settings.json");
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetGlobalSettingsPath_DefaultConstructor_UsesUserProfile()
    {
        // Arrange
        var service = new ConfigService();

        // Act
        var result = service.GetGlobalSettingsPath();

        // Assert
        var expected = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".claude",
            "settings.json");
        Assert.Equal(expected, result);
    }

    #endregion

    #region RestoreFromBackupAsync Tests

    [Fact]
    public async Task RestoreFromBackupAsync_WhenBackupExists_RestoresFromBackup()
    {
        // Arrange
        Directory.CreateDirectory(_testClaudeDirectory);
        var settingsPath = Path.Combine(_testClaudeDirectory, "settings.json");
        var backupPath = Path.Combine(_testClaudeDirectory, "settings.json.bak");

        var backupContent = """{"mcpServers": {"original": {"command": "backup"}}}""";
        var currentContent = """{"mcpServers": {"modified": {"command": "current"}}}""";

        await File.WriteAllTextAsync(backupPath, backupContent);
        await File.WriteAllTextAsync(settingsPath, currentContent);

        // Act
        var result = await _configService.RestoreFromBackupAsync();

        // Assert
        Assert.True(result);
        var restoredContent = await File.ReadAllTextAsync(settingsPath);
        Assert.Contains("original", restoredContent);
        Assert.Contains("backup", restoredContent);
    }

    [Fact]
    public async Task RestoreFromBackupAsync_WhenNoBackupExists_ReturnsFalse()
    {
        // Arrange
        Directory.CreateDirectory(_testClaudeDirectory);
        var settingsPath = Path.Combine(_testClaudeDirectory, "settings.json");
        await File.WriteAllTextAsync(settingsPath, """{"mcpServers": {}}""");

        // Act
        var result = await _configService.RestoreFromBackupAsync();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task RestoreFromBackupAsync_WhenNoDirectory_ReturnsFalse()
    {
        // Act
        var result = await _configService.RestoreFromBackupAsync();

        // Assert
        Assert.False(result);
    }

    #endregion

    #region McpServerConfig Extension Data Tests

    [Fact]
    public async Task ReadGlobalSettingsAsync_PreservesMcpServerUnknownProperties()
    {
        // Arrange
        Directory.CreateDirectory(_testClaudeDirectory);
        var settingsPath = Path.Combine(_testClaudeDirectory, "settings.json");
        var json = """
            {
                "mcpServers": {
                    "test-server": {
                        "command": "node",
                        "customField": "preserved",
                        "anotherField": 123
                    }
                }
            }
            """;
        await File.WriteAllTextAsync(settingsPath, json);

        // Act
        var result = await _configService.ReadGlobalSettingsAsync();

        // Assert
        Assert.NotNull(result.McpServers);
        var server = result.McpServers["test-server"];
        Assert.NotNull(server.ExtensionData);
        Assert.True(server.ExtensionData.ContainsKey("customField"));
        Assert.True(server.ExtensionData.ContainsKey("anotherField"));
    }

    #endregion
}
