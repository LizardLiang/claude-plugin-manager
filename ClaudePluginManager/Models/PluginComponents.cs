using System.Text.Json.Serialization;

namespace ClaudePluginManager.Models;

public class PluginComponents
{
    [JsonPropertyName("mcpServers")]
    public Dictionary<string, McpServerComponent> McpServers { get; set; } = new();

    [JsonPropertyName("hooks")]
    public Dictionary<string, HookComponent> Hooks { get; set; } = new();

    [JsonPropertyName("commands")]
    public Dictionary<string, CommandComponent> Commands { get; set; } = new();

    [JsonPropertyName("agents")]
    public Dictionary<string, AgentComponent> Agents { get; set; } = new();

    [JsonPropertyName("skills")]
    public Dictionary<string, SkillComponent> Skills { get; set; } = new();
}

public class McpServerComponent
{
    [JsonPropertyName("command")]
    public string? Command { get; set; }

    [JsonPropertyName("args")]
    public List<string>? Args { get; set; }

    [JsonPropertyName("env")]
    public Dictionary<string, string>? Env { get; set; }

    [JsonExtensionData]
    public Dictionary<string, System.Text.Json.JsonElement>? ExtensionData { get; set; }

    public McpServerConfig ToMcpServerConfig()
    {
        return new McpServerConfig
        {
            Command = Command,
            Args = Args,
            Env = Env
        };
    }
}

public class HookComponent
{
    [JsonPropertyName("matcher")]
    public string? Matcher { get; set; }

    [JsonPropertyName("script")]
    public string? Script { get; set; }

    [JsonExtensionData]
    public Dictionary<string, System.Text.Json.JsonElement>? ExtensionData { get; set; }
}

public class CommandComponent
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonExtensionData]
    public Dictionary<string, System.Text.Json.JsonElement>? ExtensionData { get; set; }
}

public class AgentComponent
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonExtensionData]
    public Dictionary<string, System.Text.Json.JsonElement>? ExtensionData { get; set; }
}

public class SkillComponent
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonExtensionData]
    public Dictionary<string, System.Text.Json.JsonElement>? ExtensionData { get; set; }
}
