namespace ClaudePluginManager.Models;

public record UninstallResult(bool Success, string? ErrorMessage = null)
{
    public static UninstallResult Ok() => new(true);
    public static UninstallResult Fail(string error) => new(false, error);
}
