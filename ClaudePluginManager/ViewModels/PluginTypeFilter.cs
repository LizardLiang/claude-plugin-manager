namespace ClaudePluginManager.ViewModels;

public class PluginTypeFilter
{
    public string? TypeCode { get; }
    public string DisplayName { get; }

    private PluginTypeFilter(string? typeCode, string displayName)
    {
        TypeCode = typeCode;
        DisplayName = displayName;
    }

    public static readonly PluginTypeFilter All = new(null, "All Types");
    public static readonly PluginTypeFilter McpServer = new("MCP_SERVER", "MCP Server");
    public static readonly PluginTypeFilter Hook = new("HOOK", "Hook");
    public static readonly PluginTypeFilter SlashCommand = new("SLASH_COMMAND", "Slash Command");
    public static readonly PluginTypeFilter Agent = new("AGENT", "Agent");
    public static readonly PluginTypeFilter Skill = new("SKILL", "Skill");

    public static IReadOnlyList<PluginTypeFilter> AllFilters { get; } = new[]
    {
        All,
        McpServer,
        Hook,
        SlashCommand,
        Agent,
        Skill
    };

    public override string ToString() => DisplayName;
}
