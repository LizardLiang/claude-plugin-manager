using System.Text.Json;
using ClaudePluginManager.Models;

namespace ClaudePluginManager.ViewModels;

public class PluginListItemViewModel : ViewModelBase
{
    private readonly Plugin _plugin;
    private IReadOnlyList<string>? _tags;

    public PluginListItemViewModel(Plugin plugin)
    {
        _plugin = plugin;
    }

    public Plugin Plugin => _plugin;

    public string Id => _plugin.Id;

    public string Name => _plugin.Name;

    public string Description => _plugin.Description ?? string.Empty;

    public string Version => _plugin.Version ?? "Unknown";

    public string Author => _plugin.Author ?? "Unknown";

    public string Type => _plugin.Type;

    public string DisplayType => FormatType(_plugin.Type);

    public IReadOnlyList<string> Tags => _tags ??= ParseTags(_plugin.Tags);

    private static IReadOnlyList<string> ParseTags(string? tagsJson)
    {
        if (string.IsNullOrWhiteSpace(tagsJson))
            return Array.Empty<string>();

        try
        {
            return JsonSerializer.Deserialize<string[]>(tagsJson) ?? Array.Empty<string>();
        }
        catch (JsonException)
        {
            return Array.Empty<string>();
        }
    }

    private static string FormatType(string type) => type switch
    {
        "MCP_SERVER" => "MCP Server",
        "HOOK" => "Hook",
        "SLASH_COMMAND" => "Slash Command",
        "AGENT" => "Agent",
        "SKILL" => "Skill",
        _ => type.Replace("_", " ")
    };
}
