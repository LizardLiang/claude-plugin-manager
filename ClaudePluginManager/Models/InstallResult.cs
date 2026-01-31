namespace ClaudePluginManager.Models;

public record InstallResult(bool Success, string? ErrorMessage = null)
{
    public static InstallResult Ok() => new(true);
    public static InstallResult Fail(string error) => new(false, error);
}
