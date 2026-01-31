using ClaudePluginManager.Models;

namespace ClaudePluginManager.Tests.Models;

public class InstalledPluginTests
{
    [Fact]
    public void InstalledPlugin_CanBeCreated()
    {
        // Arrange & Act
        var plugin = new InstalledPlugin
        {
            Id = "test-id",
            Name = "Test Plugin",
            Version = "1.0.0",
            Type = PluginType.McpServer,
            MarketplaceId = "marketplace/test",
            ConfigSnapshot = """{"command": "test"}""",
            InstalledAt = DateTime.UtcNow
        };

        // Assert
        Assert.Equal("test-id", plugin.Id);
        Assert.Equal("Test Plugin", plugin.Name);
        Assert.Equal("1.0.0", plugin.Version);
        Assert.Equal(PluginType.McpServer, plugin.Type);
        Assert.Equal("marketplace/test", plugin.MarketplaceId);
        Assert.NotNull(plugin.ConfigSnapshot);
        Assert.NotEqual(default, plugin.InstalledAt);
    }

    [Fact]
    public void InstalledPlugin_HasDefaultValues()
    {
        // Arrange & Act
        var plugin = new InstalledPlugin();

        // Assert
        Assert.Equal(string.Empty, plugin.Id);
        Assert.Equal(string.Empty, plugin.Name);
        Assert.Null(plugin.Version);
        Assert.Equal(string.Empty, plugin.MarketplaceId);
        Assert.Null(plugin.ConfigSnapshot);
    }
}
