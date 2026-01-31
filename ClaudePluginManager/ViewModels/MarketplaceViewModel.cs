using System.Collections.ObjectModel;
using ClaudePluginManager.Helpers;
using ClaudePluginManager.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ClaudePluginManager.ViewModels;

public partial class MarketplaceViewModel : ViewModelBase
{
    private readonly IMarketplaceService _marketplaceService;
    private readonly Func<PluginDetailsViewModel> _detailsFactory;
    private List<PluginListItemViewModel> _allPlugins = new();
    private CancellationTokenSource? _searchDebounceTokenSource;
    private const int DebounceDelayMs = 300;

    public MarketplaceViewModel(IMarketplaceService marketplaceService, Func<PluginDetailsViewModel> detailsFactory)
    {
        _marketplaceService = marketplaceService;
        _detailsFactory = detailsFactory;
        Plugins = new ObservableCollection<PluginListItemViewModel>();
        TypeFilters = new ObservableCollection<PluginTypeFilter>(PluginTypeFilter.AllFilters);
        _selectedTypeFilter = PluginTypeFilter.All;
    }

    public string Title => "Marketplace";

    public ObservableCollection<PluginListItemViewModel> Plugins { get; }

    public ObservableCollection<PluginTypeFilter> TypeFilters { get; }

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private PluginTypeFilter _selectedTypeFilter;

    [ObservableProperty]
    private PluginListItemViewModel? _selectedPlugin;

    [ObservableProperty]
    private PluginDetailsViewModel? _selectedPluginDetails;

    [ObservableProperty]
    private bool _isRefreshing;

    [ObservableProperty]
    private string? _refreshError;

    [ObservableProperty]
    private DateTime? _lastSyncedAt;

    public string LastSyncedText => RelativeTimeFormatter.Format(LastSyncedAt);

    public bool IsDetailsOpen => SelectedPluginDetails != null;

    public bool IsEmpty => !IsLoading && Plugins.Count == 0 && ErrorMessage == null && !HasSearchText && SelectedTypeFilter?.TypeCode == null;

    public string ResultCountText => $"Showing {Plugins.Count} of {_allPlugins.Count} plugins";

    public bool HasSearchText => !string.IsNullOrWhiteSpace(SearchText);

    partial void OnSearchTextChanged(string value)
    {
        OnPropertyChanged(nameof(HasSearchText));
        DebounceAndApplyFilters();
    }

    partial void OnSelectedTypeFilterChanged(PluginTypeFilter value)
    {
        ApplyFilters();
    }

    private async void DebounceAndApplyFilters()
    {
        _searchDebounceTokenSource?.Cancel();
        _searchDebounceTokenSource = new CancellationTokenSource();

        try
        {
            await Task.Delay(DebounceDelayMs, _searchDebounceTokenSource.Token);
            ApplyFilters();
        }
        catch (TaskCanceledException)
        {
            // Debounce cancelled, ignore
        }
    }

    private void ApplyFilters()
    {
        var filtered = _allPlugins.AsEnumerable();

        // Search filter (OR across fields)
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var search = SearchText.ToLowerInvariant();
            filtered = filtered.Where(p => MatchesSearch(p, search));
        }

        // Type filter
        if (SelectedTypeFilter?.TypeCode != null)
        {
            filtered = filtered.Where(p => p.Type == SelectedTypeFilter.TypeCode);
        }

        Plugins.Clear();
        foreach (var plugin in filtered)
        {
            Plugins.Add(plugin);
        }

        OnPropertyChanged(nameof(ResultCountText));
        OnPropertyChanged(nameof(IsEmpty));
    }

    private static bool MatchesSearch(PluginListItemViewModel plugin, string search)
    {
        return plugin.Name.ToLowerInvariant().Contains(search) ||
               plugin.Description.ToLowerInvariant().Contains(search) ||
               plugin.Author.ToLowerInvariant().Contains(search) ||
               plugin.Tags.Any(t => t.ToLowerInvariant().Contains(search));
    }

    [RelayCommand]
    private void ClearSearch()
    {
        SearchText = string.Empty;
    }

    [RelayCommand]
    private async Task SelectPluginAsync(PluginListItemViewModel? pluginVm)
    {
        if (pluginVm == null)
            return;

        SelectedPlugin = pluginVm;

        var details = _detailsFactory();
        details.CloseRequested += OnDetailsCloseRequested;
        await details.InitializeAsync(pluginVm.Plugin);

        SelectedPluginDetails = details;
        OnPropertyChanged(nameof(IsDetailsOpen));
    }

    [RelayCommand]
    private void CloseDetails()
    {
        if (SelectedPluginDetails != null)
        {
            SelectedPluginDetails.CloseRequested -= OnDetailsCloseRequested;
        }

        SelectedPlugin = null;
        SelectedPluginDetails = null;
        OnPropertyChanged(nameof(IsDetailsOpen));
    }

    private void OnDetailsCloseRequested(object? sender, EventArgs e)
    {
        CloseDetails();
    }

    [RelayCommand]
    private async Task LoadPluginsAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            // Auto-refresh if cache is stale (>24 hours) or never synced
            var lastSync = _marketplaceService.GetLastSyncTime();
            if (lastSync == null || (DateTime.UtcNow - lastSync.Value).TotalHours > 24)
            {
                try
                {
                    await _marketplaceService.RefreshAsync();
                }
                catch
                {
                    // Silent fail, use stale cache
                }
            }

            var plugins = await _marketplaceService.GetPluginsAsync();

            _allPlugins.Clear();
            foreach (var plugin in plugins)
            {
                _allPlugins.Add(new PluginListItemViewModel(plugin));
            }

            ApplyFilters();
            UpdateLastSyncedAt();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load plugins: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            OnPropertyChanged(nameof(IsEmpty));
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        if (IsRefreshing)
            return;

        try
        {
            IsRefreshing = true;
            RefreshError = null;

            await _marketplaceService.RefreshAsync();

            // Reload plugins
            var plugins = await _marketplaceService.GetPluginsAsync();
            _allPlugins.Clear();
            foreach (var plugin in plugins)
            {
                _allPlugins.Add(new PluginListItemViewModel(plugin));
            }
            ApplyFilters();

            UpdateLastSyncedAt();
        }
        catch (HttpRequestException ex)
        {
            RefreshError = $"Network error: {ex.Message}";
        }
        catch (Exception ex)
        {
            RefreshError = $"Failed to refresh: {ex.Message}";
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    private void UpdateLastSyncedAt()
    {
        LastSyncedAt = _marketplaceService.GetLastSyncTime();
        OnPropertyChanged(nameof(LastSyncedText));
    }
}
