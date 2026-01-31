using System.Collections.ObjectModel;
using ClaudePluginManager.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ClaudePluginManager.ViewModels;

public partial class InstalledViewModel : ViewModelBase
{
    private readonly IPluginService _pluginService;
    private readonly IDialogService _dialogService;

    public InstalledViewModel(IPluginService pluginService, IDialogService dialogService)
    {
        _pluginService = pluginService;
        _dialogService = dialogService;
        Plugins = new ObservableCollection<InstalledPluginViewModel>();
    }

    public string Title => "Installed";

    public ObservableCollection<InstalledPluginViewModel> Plugins { get; }

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    public bool IsEmpty => !IsLoading && Plugins.Count == 0 && ErrorMessage == null;

    public int UpdatesAvailableCount => Plugins.Count(p => p.HasUpdate);

    [RelayCommand]
    private async Task LoadPluginsAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var pluginsWithUpdates = await _pluginService.GetInstalledWithUpdatesAsync();

            Plugins.Clear();
            foreach (var pluginWithUpdate in pluginsWithUpdates)
            {
                var vm = new InstalledPluginViewModel(pluginWithUpdate.InstalledPlugin)
                {
                    HasUpdate = pluginWithUpdate.HasUpdate,
                    AvailableVersion = pluginWithUpdate.AvailableVersion
                };
                Plugins.Add(vm);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load plugins: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            OnPropertyChanged(nameof(IsEmpty));
            OnPropertyChanged(nameof(UpdatesAvailableCount));
        }
    }

    [RelayCommand]
    private async Task UninstallAsync(InstalledPluginViewModel? pluginVm)
    {
        if (pluginVm == null)
            return;

        var confirmed = await _dialogService.ConfirmAsync(
            "Uninstall Plugin",
            $"Are you sure you want to uninstall '{pluginVm.Name}'?");

        if (!confirmed)
            return;

        try
        {
            pluginVm.IsUninstalling = true;

            var result = await _pluginService.UninstallGlobalAsync(pluginVm.Id);

            if (result.Success)
            {
                Plugins.Remove(pluginVm);
                OnPropertyChanged(nameof(IsEmpty));
            }
            else
            {
                ErrorMessage = $"Failed to uninstall: {result.ErrorMessage}";
                pluginVm.IsUninstalling = false;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to uninstall: {ex.Message}";
            pluginVm.IsUninstalling = false;
        }
    }
}
