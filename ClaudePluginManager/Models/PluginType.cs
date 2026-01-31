namespace ClaudePluginManager.Models;

public enum PluginType
{
    McpServer,
    Hook,
    SlashCommand,
    Agent,
    Skill
}

public static class PluginTypeExtensions
{
    public static string ToDbString(this PluginType type) => type switch
    {
        PluginType.McpServer => "MCP_SERVER",
        PluginType.Hook => "HOOK",
        PluginType.SlashCommand => "SLASH_COMMAND",
        PluginType.Agent => "AGENT",
        PluginType.Skill => "SKILL",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown plugin type")
    };

    public static PluginType FromDbString(string value) => value.ToUpperInvariant() switch
    {
        "MCP_SERVER" => PluginType.McpServer,
        "HOOK" => PluginType.Hook,
        "SLASH_COMMAND" => PluginType.SlashCommand,
        "AGENT" => PluginType.Agent,
        "SKILL" => PluginType.Skill,
        _ => throw new ArgumentException($"Unknown plugin type: {value}", nameof(value))
    };
}
