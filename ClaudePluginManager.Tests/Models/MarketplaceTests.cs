namespace ClaudePluginManager.Tests.Models;

using ClaudePluginManager.Models;

public class MarketplaceTests
{
    [Fact]
    public void Marketplace_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var marketplace = new Marketplace();

        // Assert
        Assert.Equal(string.Empty, marketplace.Id);
        Assert.Equal(string.Empty, marketplace.Name);
        Assert.Equal(string.Empty, marketplace.GitHubOwner);
        Assert.Equal(string.Empty, marketplace.GitHubRepo);
        Assert.False(marketplace.Enabled);
        Assert.Equal(0, marketplace.Priority);
        Assert.False(marketplace.RequiresAuth);
        Assert.Null(marketplace.LastSyncedAt);
        Assert.Equal(default, marketplace.CreatedAt);
    }

    [Fact]
    public void Marketplace_SetProperties_WorkCorrectly()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var createdAt = DateTime.UtcNow.AddDays(-7);

        // Act
        var marketplace = new Marketplace
        {
            Id = "test-id",
            Name = "Test Marketplace",
            GitHubOwner = "test-owner",
            GitHubRepo = "test-repo",
            Enabled = true,
            Priority = 10,
            RequiresAuth = true,
            LastSyncedAt = now,
            CreatedAt = createdAt
        };

        // Assert
        Assert.Equal("test-id", marketplace.Id);
        Assert.Equal("Test Marketplace", marketplace.Name);
        Assert.Equal("test-owner", marketplace.GitHubOwner);
        Assert.Equal("test-repo", marketplace.GitHubRepo);
        Assert.True(marketplace.Enabled);
        Assert.Equal(10, marketplace.Priority);
        Assert.True(marketplace.RequiresAuth);
        Assert.Equal(now, marketplace.LastSyncedAt);
        Assert.Equal(createdAt, marketplace.CreatedAt);
    }
}
