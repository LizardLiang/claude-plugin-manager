namespace ClaudePluginManager.Services;

/// <summary>
/// Dialog service implementation. In a real application, this would show
/// modal dialogs. For now, it returns true for confirmations.
/// The actual confirmation UI is handled inline in the view.
/// </summary>
public class DialogService : IDialogService
{
    public Task<bool> ConfirmAsync(string title, string message)
    {
        // For now, always confirm. Real implementation would show a dialog.
        // The InstalledViewModel handles confirmation via IsUninstalling state.
        return Task.FromResult(true);
    }
}
