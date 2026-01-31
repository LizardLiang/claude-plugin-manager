namespace ClaudePluginManager.Models;

public class Marketplace
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string GitHubOwner { get; set; } = string.Empty;
    public string GitHubRepo { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public int Priority { get; set; }
    public bool RequiresAuth { get; set; }
    public DateTime? LastSyncedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
