namespace ClaudePluginManager.Models;

/// <summary>
/// Wrapper model for an installed plugin with update information.
/// </summary>
public class InstalledPluginWithUpdate
{
    public required InstalledPlugin InstalledPlugin { get; set; }
    public string? AvailableVersion { get; set; }
    public bool HasUpdate { get; set; }
}
