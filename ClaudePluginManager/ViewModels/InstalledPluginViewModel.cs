using ClaudePluginManager.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ClaudePluginManager.ViewModels;

public partial class InstalledPluginViewModel : ViewModelBase
{
    public InstalledPluginViewModel(InstalledPlugin plugin)
    {
        InstalledPlugin = plugin;
    }

    public InstalledPlugin InstalledPlugin { get; }

    public string Id => InstalledPlugin.Id;

    public string Name => InstalledPlugin.Name;

    public string? Version => InstalledPlugin.Version;

    public PluginType Type => InstalledPlugin.Type;

    public string DisplayType => Type switch
    {
        PluginType.McpServer => "MCP Server",
        PluginType.Hook => "Hook",
        PluginType.SlashCommand => "Slash Command",
        PluginType.Agent => "Agent",
        PluginType.Skill => "Skill",
        _ => Type.ToString()
    };

    public DateTime InstalledAt => InstalledPlugin.InstalledAt;

    public string InstalledAtDisplay => InstalledAt.ToString("MMM dd, yyyy");

    [ObservableProperty]
    private bool _isUninstalling;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(VersionDisplay))]
    private string? _availableVersion;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(VersionDisplay))]
    private bool _hasUpdate;

    public string VersionDisplay => HasUpdate ? $"{Version} → {AvailableVersion}" : Version ?? "";
}
