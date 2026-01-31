using System.Text.Json.Serialization;

namespace ClaudePluginManager.Models;

public class MarketplaceIndex
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("plugins")]
    public List<PluginEntry> Plugins { get; set; } = new();
}

public class PluginEntry
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("version")]
    public string? Version { get; set; }

    [JsonPropertyName("author")]
    public PluginAuthor? Author { get; set; }

    [JsonPropertyName("repository")]
    public string? Repository { get; set; }

    [JsonPropertyName("source")]
    public string? Source { get; set; }

    [JsonPropertyName("license")]
    public string? License { get; set; }

    [JsonPropertyName("keywords")]
    public List<string>? Keywords { get; set; }

    // Component arrays to determine plugin type
    [JsonPropertyName("commands")]
    public List<string>? Commands { get; set; }

    [JsonPropertyName("skills")]
    public List<string>? Skills { get; set; }

    [JsonPropertyName("agents")]
    public List<string>? Agents { get; set; }

    [JsonPropertyName("hooks")]
    public List<string>? Hooks { get; set; }

    [JsonPropertyName("mcpServers")]
    public List<string>? McpServers { get; set; }

    // Computed type based on components
    public string Type => GetPrimaryType();

    // For backwards compatibility
    public List<string>? Tags => Keywords;

    private string GetPrimaryType()
    {
        if (McpServers?.Count > 0) return "MCP_SERVER";
        if (Hooks?.Count > 0) return "HOOK";
        if (Commands?.Count > 0) return "SLASH_COMMAND";
        if (Agents?.Count > 0) return "AGENT";
        if (Skills?.Count > 0) return "SKILL";
        return "PLUGIN";
    }
}

public class PluginAuthor
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    public override string ToString() => Name;
}
