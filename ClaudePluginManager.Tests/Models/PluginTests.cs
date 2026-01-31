namespace ClaudePluginManager.Tests.Models;

using ClaudePluginManager.Models;

public class PluginTests
{
    [Fact]
    public void Plugin_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var plugin = new Plugin();

        // Assert
        Assert.Equal(string.Empty, plugin.Id);
        Assert.Equal(string.Empty, plugin.Name);
        Assert.Null(plugin.Description);
        Assert.Null(plugin.Version);
        Assert.Null(plugin.Author);
        Assert.Null(plugin.Repository);
        Assert.Equal(string.Empty, plugin.MarketplaceId);
        Assert.Equal(string.Empty, plugin.Type);
        Assert.Null(plugin.Tags);
        Assert.Null(plugin.Dependencies);
        Assert.Null(plugin.ConfigSchema);
        Assert.Null(plugin.Components);
        Assert.Equal(default, plugin.CachedAt);
    }

    [Fact]
    public void Plugin_SetProperties_WorkCorrectly()
    {
        // Arrange
        var cachedAt = DateTime.UtcNow;

        // Act
        var plugin = new Plugin
        {
            Id = "plugin-1",
            Name = "Test Plugin",
            Description = "A test plugin",
            Version = "1.0.0",
            Author = "Test Author",
            Repository = "https://github.com/test/plugin",
            MarketplaceId = "marketplace-1",
            Type = "mcp",
            Tags = "test,plugin",
            Dependencies = "dep1,dep2",
            ConfigSchema = "{\"type\":\"object\"}",
            Components = "component1",
            CachedAt = cachedAt
        };

        // Assert
        Assert.Equal("plugin-1", plugin.Id);
        Assert.Equal("Test Plugin", plugin.Name);
        Assert.Equal("A test plugin", plugin.Description);
        Assert.Equal("1.0.0", plugin.Version);
        Assert.Equal("Test Author", plugin.Author);
        Assert.Equal("https://github.com/test/plugin", plugin.Repository);
        Assert.Equal("marketplace-1", plugin.MarketplaceId);
        Assert.Equal("mcp", plugin.Type);
        Assert.Equal("test,plugin", plugin.Tags);
        Assert.Equal("dep1,dep2", plugin.Dependencies);
        Assert.Equal("{\"type\":\"object\"}", plugin.ConfigSchema);
        Assert.Equal("component1", plugin.Components);
        Assert.Equal(cachedAt, plugin.CachedAt);
    }
}
