namespace ClaudePluginManager.Models;

public class InstalledPlugin
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Version { get; set; }
    public PluginType Type { get; set; }
    public string MarketplaceId { get; set; } = string.Empty;
    public string? ConfigSnapshot { get; set; }
    public DateTime InstalledAt { get; set; }
}
