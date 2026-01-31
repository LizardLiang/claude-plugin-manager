namespace ClaudePluginManager.Models;

public class Plugin
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Version { get; set; }
    public string? Author { get; set; }
    public string? Repository { get; set; }
    public string MarketplaceId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Tags { get; set; }
    public string? Dependencies { get; set; }
    public string? ConfigSchema { get; set; }
    public string? Components { get; set; }
    public DateTime CachedAt { get; set; }
}
