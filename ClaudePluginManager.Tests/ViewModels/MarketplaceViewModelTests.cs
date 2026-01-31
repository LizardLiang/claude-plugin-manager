using ClaudePluginManager.Models;
using ClaudePluginManager.Services;
using ClaudePluginManager.ViewModels;
using NSubstitute;

namespace ClaudePluginManager.Tests.ViewModels;

public class MarketplaceViewModelTests
{
    private readonly IMarketplaceService _marketplaceService;
    private readonly Func<PluginDetailsViewModel> _detailsFactory;
    private readonly IGitHubClient _gitHubClient;
    private readonly MarketplaceViewModel _viewModel;

    public MarketplaceViewModelTests()
    {
        _marketplaceService = Substitute.For<IMarketplaceService>();
        _gitHubClient = Substitute.For<IGitHubClient>();
        _detailsFactory = () => new PluginDetailsViewModel(_gitHubClient);
        _viewModel = new MarketplaceViewModel(_marketplaceService, _detailsFactory);
    }

    [Fact]
    public void InheritsFromViewModelBase()
    {
        Assert.IsAssignableFrom<ViewModelBase>(_viewModel);
    }

    [Fact]
    public void Title_ReturnsMarketplace()
    {
        Assert.Equal("Marketplace", _viewModel.Title);
    }

    [Fact]
    public void Constructor_AcceptsMarketplaceServiceAndFactory()
    {
        var service = Substitute.For<IMarketplaceService>();
        var factory = () => new PluginDetailsViewModel(_gitHubClient);
        var viewModel = new MarketplaceViewModel(service, factory);

        Assert.NotNull(viewModel);
    }

    [Fact]
    public void Plugins_InitiallyEmpty()
    {
        Assert.Empty(_viewModel.Plugins);
    }

    [Fact]
    public void IsLoading_InitiallyFalse()
    {
        Assert.False(_viewModel.IsLoading);
    }

    [Fact]
    public void IsEmpty_WhenNoPlugins_ReturnsTrue()
    {
        Assert.True(_viewModel.IsEmpty);
    }

    [Fact]
    public async Task IsEmpty_WhenPluginsExist_ReturnsFalse()
    {
        var plugins = new List<Plugin>
        {
            new Plugin { Id = "1", Name = "Plugin 1", Type = "MCP_SERVER" }
        };
        _marketplaceService.GetPluginsAsync().Returns(plugins);

        await _viewModel.LoadPluginsCommand.ExecuteAsync(null);

        Assert.False(_viewModel.IsEmpty);
    }

    [Fact]
    public async Task LoadPluginsCommand_SetsIsLoadingTrue_DuringLoad()
    {
        var tcs = new TaskCompletionSource<IEnumerable<Plugin>>();
        _marketplaceService.GetPluginsAsync().Returns(tcs.Task);

        var loadTask = _viewModel.LoadPluginsCommand.ExecuteAsync(null);

        Assert.True(_viewModel.IsLoading);

        tcs.SetResult(new List<Plugin>());
        await loadTask;
    }

    [Fact]
    public async Task LoadPluginsCommand_SetsIsLoadingFalse_AfterLoad()
    {
        _marketplaceService.GetPluginsAsync().Returns(new List<Plugin>());

        await _viewModel.LoadPluginsCommand.ExecuteAsync(null);

        Assert.False(_viewModel.IsLoading);
    }

    [Fact]
    public async Task LoadPluginsCommand_PopulatesPlugins()
    {
        var plugins = new List<Plugin>
        {
            new Plugin { Id = "1", Name = "Plugin 1", Type = "MCP_SERVER" },
            new Plugin { Id = "2", Name = "Plugin 2", Type = "HOOK" }
        };
        _marketplaceService.GetPluginsAsync().Returns(plugins);

        await _viewModel.LoadPluginsCommand.ExecuteAsync(null);

        Assert.Equal(2, _viewModel.Plugins.Count);
    }

    [Fact]
    public async Task LoadPluginsCommand_WrapsPluginsInViewModel()
    {
        var plugins = new List<Plugin>
        {
            new Plugin { Id = "test-id", Name = "Test Plugin", Type = "MCP_SERVER" }
        };
        _marketplaceService.GetPluginsAsync().Returns(plugins);

        await _viewModel.LoadPluginsCommand.ExecuteAsync(null);

        var pluginVm = _viewModel.Plugins.First();
        Assert.IsType<PluginListItemViewModel>(pluginVm);
        Assert.Equal("test-id", pluginVm.Id);
        Assert.Equal("Test Plugin", pluginVm.Name);
    }

    [Fact]
    public async Task LoadPluginsCommand_OnException_SetsErrorMessage()
    {
        _marketplaceService.GetPluginsAsync().Returns<IEnumerable<Plugin>>(
            _ => throw new Exception("Network error"));

        await _viewModel.LoadPluginsCommand.ExecuteAsync(null);

        Assert.NotNull(_viewModel.ErrorMessage);
        Assert.Contains("Network error", _viewModel.ErrorMessage);
    }

    [Fact]
    public async Task LoadPluginsCommand_OnException_SetsIsLoadingFalse()
    {
        _marketplaceService.GetPluginsAsync().Returns<IEnumerable<Plugin>>(
            _ => throw new Exception("Network error"));

        await _viewModel.LoadPluginsCommand.ExecuteAsync(null);

        Assert.False(_viewModel.IsLoading);
    }

    [Fact]
    public async Task LoadPluginsCommand_ClearsErrorMessage_OnSuccessfulLoad()
    {
        var callCount = 0;
        _marketplaceService.GetPluginsAsync().Returns(_ =>
        {
            callCount++;
            if (callCount == 1)
                throw new Exception("Network error");
            return Task.FromResult<IEnumerable<Plugin>>(new List<Plugin>());
        });

        await _viewModel.LoadPluginsCommand.ExecuteAsync(null);
        Assert.NotNull(_viewModel.ErrorMessage);

        await _viewModel.LoadPluginsCommand.ExecuteAsync(null);
        Assert.Null(_viewModel.ErrorMessage);
    }

    [Fact]
    public async Task IsEmpty_WhenErrorMessage_ReturnsFalse()
    {
        _marketplaceService.GetPluginsAsync().Returns<IEnumerable<Plugin>>(
            _ => throw new Exception("Network error"));

        await _viewModel.LoadPluginsCommand.ExecuteAsync(null);

        Assert.False(_viewModel.IsEmpty);
    }

    [Fact]
    public async Task LoadPluginsCommand_ClearsPreviousPlugins_BeforeLoading()
    {
        var plugins1 = new List<Plugin>
        {
            new Plugin { Id = "1", Name = "Plugin 1", Type = "MCP_SERVER" }
        };
        var plugins2 = new List<Plugin>
        {
            new Plugin { Id = "2", Name = "Plugin 2", Type = "HOOK" }
        };

        _marketplaceService.GetPluginsAsync().Returns(plugins1);
        await _viewModel.LoadPluginsCommand.ExecuteAsync(null);

        _marketplaceService.GetPluginsAsync().Returns(plugins2);
        await _viewModel.LoadPluginsCommand.ExecuteAsync(null);

        Assert.Single(_viewModel.Plugins);
        Assert.Equal("2", _viewModel.Plugins.First().Id);
    }

    [Fact]
    public void SearchText_InitiallyEmpty()
    {
        Assert.Equal(string.Empty, _viewModel.SearchText);
    }

    [Fact]
    public void TypeFilters_ContainsAllFilterOptions()
    {
        Assert.Equal(6, _viewModel.TypeFilters.Count);
        Assert.Contains(_viewModel.TypeFilters, f => f.DisplayName == "All Types");
        Assert.Contains(_viewModel.TypeFilters, f => f.DisplayName == "MCP Server");
        Assert.Contains(_viewModel.TypeFilters, f => f.DisplayName == "Hook");
        Assert.Contains(_viewModel.TypeFilters, f => f.DisplayName == "Slash Command");
        Assert.Contains(_viewModel.TypeFilters, f => f.DisplayName == "Agent");
        Assert.Contains(_viewModel.TypeFilters, f => f.DisplayName == "Skill");
    }

    [Fact]
    public void SelectedTypeFilter_InitiallyAllTypes()
    {
        Assert.NotNull(_viewModel.SelectedTypeFilter);
        Assert.Null(_viewModel.SelectedTypeFilter.TypeCode);
    }

    [Fact]
    public async Task SearchText_FiltersByName_CaseInsensitive()
    {
        var plugins = new List<Plugin>
        {
            new Plugin { Id = "1", Name = "Test Plugin", Type = "MCP_SERVER" },
            new Plugin { Id = "2", Name = "Another One", Type = "MCP_SERVER" }
        };
        _marketplaceService.GetPluginsAsync().Returns(plugins);
        await _viewModel.LoadPluginsCommand.ExecuteAsync(null);

        _viewModel.SearchText = "test";
        await Task.Delay(350); // Wait for debounce

        Assert.Single(_viewModel.Plugins);
        Assert.Equal("Test Plugin", _viewModel.Plugins.First().Name);
    }

    [Fact]
    public async Task SearchText_FiltersByDescription()
    {
        var plugins = new List<Plugin>
        {
            new Plugin { Id = "1", Name = "Plugin A", Description = "A useful tool", Type = "MCP_SERVER" },
            new Plugin { Id = "2", Name = "Plugin B", Description = "Something else", Type = "MCP_SERVER" }
        };
        _marketplaceService.GetPluginsAsync().Returns(plugins);
        await _viewModel.LoadPluginsCommand.ExecuteAsync(null);

        _viewModel.SearchText = "useful";
        await Task.Delay(350);

        Assert.Single(_viewModel.Plugins);
        Assert.Equal("Plugin A", _viewModel.Plugins.First().Name);
    }

    [Fact]
    public async Task SearchText_FiltersByAuthor()
    {
        var plugins = new List<Plugin>
        {
            new Plugin { Id = "1", Name = "Plugin A", Author = "John Doe", Type = "MCP_SERVER" },
            new Plugin { Id = "2", Name = "Plugin B", Author = "Jane Smith", Type = "MCP_SERVER" }
        };
        _marketplaceService.GetPluginsAsync().Returns(plugins);
        await _viewModel.LoadPluginsCommand.ExecuteAsync(null);

        _viewModel.SearchText = "john";
        await Task.Delay(350);

        Assert.Single(_viewModel.Plugins);
        Assert.Equal("Plugin A", _viewModel.Plugins.First().Name);
    }

    [Fact]
    public async Task SearchText_FiltersByTags()
    {
        var plugins = new List<Plugin>
        {
            new Plugin { Id = "1", Name = "Plugin A", Tags = "[\"cli\", \"productivity\"]", Type = "MCP_SERVER" },
            new Plugin { Id = "2", Name = "Plugin B", Tags = "[\"web\", \"api\"]", Type = "MCP_SERVER" }
        };
        _marketplaceService.GetPluginsAsync().Returns(plugins);
        await _viewModel.LoadPluginsCommand.ExecuteAsync(null);

        _viewModel.SearchText = "productivity";
        await Task.Delay(350);

        Assert.Single(_viewModel.Plugins);
        Assert.Equal("Plugin A", _viewModel.Plugins.First().Name);
    }

    [Fact]
    public async Task SelectedTypeFilter_FiltersPluginsByType()
    {
        var plugins = new List<Plugin>
        {
            new Plugin { Id = "1", Name = "MCP Plugin", Type = "MCP_SERVER" },
            new Plugin { Id = "2", Name = "Hook Plugin", Type = "HOOK" }
        };
        _marketplaceService.GetPluginsAsync().Returns(plugins);
        await _viewModel.LoadPluginsCommand.ExecuteAsync(null);

        _viewModel.SelectedTypeFilter = PluginTypeFilter.Hook;

        Assert.Single(_viewModel.Plugins);
        Assert.Equal("Hook Plugin", _viewModel.Plugins.First().Name);
    }

    [Fact]
    public async Task SearchAndTypeFilter_UseAndLogic()
    {
        var plugins = new List<Plugin>
        {
            new Plugin { Id = "1", Name = "Test MCP", Type = "MCP_SERVER" },
            new Plugin { Id = "2", Name = "Test Hook", Type = "HOOK" },
            new Plugin { Id = "3", Name = "Other MCP", Type = "MCP_SERVER" }
        };
        _marketplaceService.GetPluginsAsync().Returns(plugins);
        await _viewModel.LoadPluginsCommand.ExecuteAsync(null);

        _viewModel.SearchText = "test";
        _viewModel.SelectedTypeFilter = PluginTypeFilter.McpServer;
        await Task.Delay(350);

        Assert.Single(_viewModel.Plugins);
        Assert.Equal("Test MCP", _viewModel.Plugins.First().Name);
    }

    [Fact]
    public async Task ClearSearchCommand_ResetsSearchText()
    {
        var plugins = new List<Plugin>
        {
            new Plugin { Id = "1", Name = "Test Plugin", Type = "MCP_SERVER" }
        };
        _marketplaceService.GetPluginsAsync().Returns(plugins);
        await _viewModel.LoadPluginsCommand.ExecuteAsync(null);

        _viewModel.SearchText = "test";
        await Task.Delay(350);

        _viewModel.ClearSearchCommand.Execute(null);
        await Task.Delay(350);

        Assert.Equal(string.Empty, _viewModel.SearchText);
        Assert.Single(_viewModel.Plugins);
    }

    [Fact]
    public async Task ResultCountText_ShowsCorrectCounts()
    {
        var plugins = new List<Plugin>
        {
            new Plugin { Id = "1", Name = "Test Plugin", Type = "MCP_SERVER" },
            new Plugin { Id = "2", Name = "Another Plugin", Type = "MCP_SERVER" }
        };
        _marketplaceService.GetPluginsAsync().Returns(plugins);
        await _viewModel.LoadPluginsCommand.ExecuteAsync(null);

        Assert.Equal("Showing 2 of 2 plugins", _viewModel.ResultCountText);

        _viewModel.SearchText = "test";
        await Task.Delay(350);

        Assert.Equal("Showing 1 of 2 plugins", _viewModel.ResultCountText);
    }

    [Fact]
    public void HasSearchText_ReturnsFalse_WhenEmpty()
    {
        Assert.False(_viewModel.HasSearchText);
    }

    [Fact]
    public void HasSearchText_ReturnsTrue_WhenNotEmpty()
    {
        _viewModel.SearchText = "test";
        Assert.True(_viewModel.HasSearchText);
    }

    [Fact]
    public async Task SearchText_IsDebounced()
    {
        var plugins = new List<Plugin>
        {
            new Plugin { Id = "1", Name = "Test Plugin", Type = "MCP_SERVER" },
            new Plugin { Id = "2", Name = "Another Plugin", Type = "MCP_SERVER" }
        };
        _marketplaceService.GetPluginsAsync().Returns(plugins);
        await _viewModel.LoadPluginsCommand.ExecuteAsync(null);

        _viewModel.SearchText = "t";
        _viewModel.SearchText = "te";
        _viewModel.SearchText = "tes";
        _viewModel.SearchText = "test";

        // Immediately after typing, no filtering should have occurred
        Assert.Equal(2, _viewModel.Plugins.Count);

        // Wait for debounce
        await Task.Delay(350);

        // Now filtering should have occurred
        Assert.Single(_viewModel.Plugins);
    }

    [Fact]
    public void SelectedPlugin_InitiallyNull()
    {
        Assert.Null(_viewModel.SelectedPlugin);
    }

    [Fact]
    public void SelectedPluginDetails_InitiallyNull()
    {
        Assert.Null(_viewModel.SelectedPluginDetails);
    }

    [Fact]
    public void IsDetailsOpen_InitiallyFalse()
    {
        Assert.False(_viewModel.IsDetailsOpen);
    }

    [Fact]
    public async Task SelectPluginCommand_SetsSelectedPlugin()
    {
        var plugins = new List<Plugin>
        {
            new Plugin { Id = "1", Name = "Test Plugin", Type = "MCP_SERVER" }
        };
        _marketplaceService.GetPluginsAsync().Returns(plugins);
        await _viewModel.LoadPluginsCommand.ExecuteAsync(null);

        var pluginVm = _viewModel.Plugins.First();
        await _viewModel.SelectPluginCommand.ExecuteAsync(pluginVm);

        Assert.Equal(pluginVm, _viewModel.SelectedPlugin);
    }

    [Fact]
    public async Task SelectPluginCommand_CreatesPluginDetails()
    {
        var plugins = new List<Plugin>
        {
            new Plugin { Id = "1", Name = "Test Plugin", Type = "MCP_SERVER" }
        };
        _marketplaceService.GetPluginsAsync().Returns(plugins);
        await _viewModel.LoadPluginsCommand.ExecuteAsync(null);

        var pluginVm = _viewModel.Plugins.First();
        await _viewModel.SelectPluginCommand.ExecuteAsync(pluginVm);

        Assert.NotNull(_viewModel.SelectedPluginDetails);
        Assert.Equal("Test Plugin", _viewModel.SelectedPluginDetails.Name);
    }

    [Fact]
    public async Task SelectPluginCommand_SetsIsDetailsOpenTrue()
    {
        var plugins = new List<Plugin>
        {
            new Plugin { Id = "1", Name = "Test Plugin", Type = "MCP_SERVER" }
        };
        _marketplaceService.GetPluginsAsync().Returns(plugins);
        await _viewModel.LoadPluginsCommand.ExecuteAsync(null);

        var pluginVm = _viewModel.Plugins.First();
        await _viewModel.SelectPluginCommand.ExecuteAsync(pluginVm);

        Assert.True(_viewModel.IsDetailsOpen);
    }

    [Fact]
    public void CloseDetailsCommand_ClearsSelection()
    {
        _viewModel.CloseDetailsCommand.Execute(null);

        Assert.Null(_viewModel.SelectedPlugin);
        Assert.Null(_viewModel.SelectedPluginDetails);
        Assert.False(_viewModel.IsDetailsOpen);
    }

    [Fact]
    public async Task CloseDetailsCommand_AfterSelection_ClearsState()
    {
        var plugins = new List<Plugin>
        {
            new Plugin { Id = "1", Name = "Test Plugin", Type = "MCP_SERVER" }
        };
        _marketplaceService.GetPluginsAsync().Returns(plugins);
        await _viewModel.LoadPluginsCommand.ExecuteAsync(null);

        var pluginVm = _viewModel.Plugins.First();
        await _viewModel.SelectPluginCommand.ExecuteAsync(pluginVm);
        Assert.True(_viewModel.IsDetailsOpen);

        _viewModel.CloseDetailsCommand.Execute(null);

        Assert.Null(_viewModel.SelectedPlugin);
        Assert.Null(_viewModel.SelectedPluginDetails);
        Assert.False(_viewModel.IsDetailsOpen);
    }

    [Fact]
    public async Task SelectPluginCommand_WithNull_DoesNothing()
    {
        await _viewModel.SelectPluginCommand.ExecuteAsync(null);

        Assert.Null(_viewModel.SelectedPlugin);
        Assert.Null(_viewModel.SelectedPluginDetails);
        Assert.False(_viewModel.IsDetailsOpen);
    }

    [Fact]
    public async Task PluginDetailsCloseRequested_ClosesDetails()
    {
        var plugins = new List<Plugin>
        {
            new Plugin { Id = "1", Name = "Test Plugin", Type = "MCP_SERVER" }
        };
        _marketplaceService.GetPluginsAsync().Returns(plugins);
        await _viewModel.LoadPluginsCommand.ExecuteAsync(null);

        var pluginVm = _viewModel.Plugins.First();
        await _viewModel.SelectPluginCommand.ExecuteAsync(pluginVm);

        // Simulate close from details view
        _viewModel.SelectedPluginDetails!.CloseCommand.Execute(null);

        Assert.False(_viewModel.IsDetailsOpen);
        Assert.Null(_viewModel.SelectedPluginDetails);
    }

    #region Refresh Functionality Tests

    [Fact]
    public void IsRefreshing_InitiallyFalse()
    {
        Assert.False(_viewModel.IsRefreshing);
    }

    [Fact]
    public void RefreshError_InitiallyNull()
    {
        Assert.Null(_viewModel.RefreshError);
    }

    [Fact]
    public void LastSyncedAt_InitiallyNull()
    {
        Assert.Null(_viewModel.LastSyncedAt);
    }

    [Fact]
    public void LastSyncedText_WhenNull_ReturnsNeverSynced()
    {
        Assert.Equal("Never synced", _viewModel.LastSyncedText);
    }

    [Fact]
    public async Task RefreshCommand_SetsIsRefreshingTrue_DuringRefresh()
    {
        var tcs = new TaskCompletionSource();
        _marketplaceService.RefreshAsync().Returns(tcs.Task);
        _marketplaceService.GetPluginsAsync().Returns(new List<Plugin>());

        var refreshTask = _viewModel.RefreshCommand.ExecuteAsync(null);

        Assert.True(_viewModel.IsRefreshing);

        tcs.SetResult();
        await refreshTask;
    }

    [Fact]
    public async Task RefreshCommand_SetsIsRefreshingFalse_AfterRefresh()
    {
        _marketplaceService.RefreshAsync().Returns(Task.CompletedTask);
        _marketplaceService.GetPluginsAsync().Returns(new List<Plugin>());

        await _viewModel.RefreshCommand.ExecuteAsync(null);

        Assert.False(_viewModel.IsRefreshing);
    }

    [Fact]
    public async Task RefreshCommand_CallsMarketplaceServiceRefresh()
    {
        _marketplaceService.RefreshAsync().Returns(Task.CompletedTask);
        _marketplaceService.GetPluginsAsync().Returns(new List<Plugin>());

        await _viewModel.RefreshCommand.ExecuteAsync(null);

        await _marketplaceService.Received(1).RefreshAsync();
    }

    [Fact]
    public async Task RefreshCommand_ReloadsPluginsAfterRefresh()
    {
        var plugins = new List<Plugin>
        {
            new Plugin { Id = "1", Name = "Plugin 1", Type = "MCP_SERVER" }
        };
        _marketplaceService.RefreshAsync().Returns(Task.CompletedTask);
        _marketplaceService.GetPluginsAsync().Returns(plugins);

        await _viewModel.RefreshCommand.ExecuteAsync(null);

        Assert.Single(_viewModel.Plugins);
        Assert.Equal("Plugin 1", _viewModel.Plugins.First().Name);
    }

    [Fact]
    public async Task RefreshCommand_OnHttpRequestException_SetsNetworkError()
    {
        _marketplaceService.RefreshAsync().Returns(
            Task.FromException(new HttpRequestException("Connection refused")));

        await _viewModel.RefreshCommand.ExecuteAsync(null);

        Assert.NotNull(_viewModel.RefreshError);
        Assert.StartsWith("Network error:", _viewModel.RefreshError);
        Assert.Contains("Connection refused", _viewModel.RefreshError);
    }

    [Fact]
    public async Task RefreshCommand_OnException_SetsRefreshError()
    {
        _marketplaceService.RefreshAsync().Returns(
            Task.FromException(new Exception("Parse error")));

        await _viewModel.RefreshCommand.ExecuteAsync(null);

        Assert.NotNull(_viewModel.RefreshError);
        Assert.StartsWith("Failed to refresh:", _viewModel.RefreshError);
        Assert.Contains("Parse error", _viewModel.RefreshError);
    }

    [Fact]
    public async Task RefreshCommand_ClearsRefreshError_OnSuccess()
    {
        // First call fails
        _marketplaceService.RefreshAsync().Returns(
            Task.FromException(new Exception("Error")));
        await _viewModel.RefreshCommand.ExecuteAsync(null);
        Assert.NotNull(_viewModel.RefreshError);

        // Second call succeeds
        _marketplaceService.RefreshAsync().Returns(Task.CompletedTask);
        _marketplaceService.GetPluginsAsync().Returns(new List<Plugin>());
        await _viewModel.RefreshCommand.ExecuteAsync(null);

        Assert.Null(_viewModel.RefreshError);
    }

    [Fact]
    public async Task RefreshCommand_UpdatesLastSyncedAt()
    {
        var syncTime = DateTime.UtcNow;
        _marketplaceService.RefreshAsync().Returns(Task.CompletedTask);
        _marketplaceService.GetPluginsAsync().Returns(new List<Plugin>());
        _marketplaceService.GetLastSyncTime().Returns(syncTime);

        await _viewModel.RefreshCommand.ExecuteAsync(null);

        Assert.Equal(syncTime, _viewModel.LastSyncedAt);
    }

    [Fact]
    public async Task RefreshCommand_DoesNotRunConcurrently()
    {
        var tcs = new TaskCompletionSource();
        _marketplaceService.RefreshAsync().Returns(tcs.Task);
        _marketplaceService.GetPluginsAsync().Returns(new List<Plugin>());

        // Start first refresh
        var task1 = _viewModel.RefreshCommand.ExecuteAsync(null);

        // Try to start second refresh while first is running
        await _viewModel.RefreshCommand.ExecuteAsync(null);

        // Should only have called RefreshAsync once
        await _marketplaceService.Received(1).RefreshAsync();

        tcs.SetResult();
        await task1;
    }

    [Fact]
    public async Task LoadPluginsCommand_AutoRefreshes_WhenCacheStale()
    {
        var staleTime = DateTime.UtcNow.AddHours(-25);
        _marketplaceService.GetLastSyncTime().Returns(staleTime);
        _marketplaceService.RefreshAsync().Returns(Task.CompletedTask);
        _marketplaceService.GetPluginsAsync().Returns(new List<Plugin>());

        await _viewModel.LoadPluginsCommand.ExecuteAsync(null);

        await _marketplaceService.Received(1).RefreshAsync();
    }

    [Fact]
    public async Task LoadPluginsCommand_AutoRefreshes_WhenNeverSynced()
    {
        _marketplaceService.GetLastSyncTime().Returns((DateTime?)null);
        _marketplaceService.RefreshAsync().Returns(Task.CompletedTask);
        _marketplaceService.GetPluginsAsync().Returns(new List<Plugin>());

        await _viewModel.LoadPluginsCommand.ExecuteAsync(null);

        await _marketplaceService.Received(1).RefreshAsync();
    }

    [Fact]
    public async Task LoadPluginsCommand_DoesNotAutoRefresh_WhenCacheFresh()
    {
        var freshTime = DateTime.UtcNow.AddHours(-12);
        _marketplaceService.GetLastSyncTime().Returns(freshTime);
        _marketplaceService.GetPluginsAsync().Returns(new List<Plugin>());

        await _viewModel.LoadPluginsCommand.ExecuteAsync(null);

        await _marketplaceService.DidNotReceive().RefreshAsync();
    }

    [Fact]
    public async Task LoadPluginsCommand_ContinuesWithCache_WhenAutoRefreshFails()
    {
        var staleTime = DateTime.UtcNow.AddHours(-25);
        _marketplaceService.GetLastSyncTime().Returns(staleTime);
        _marketplaceService.RefreshAsync().Returns(
            Task.FromException(new HttpRequestException("Network error")));
        var cachedPlugins = new List<Plugin>
        {
            new Plugin { Id = "1", Name = "Cached Plugin", Type = "MCP_SERVER" }
        };
        _marketplaceService.GetPluginsAsync().Returns(cachedPlugins);

        await _viewModel.LoadPluginsCommand.ExecuteAsync(null);

        // Should still load cached plugins
        Assert.Single(_viewModel.Plugins);
        Assert.Equal("Cached Plugin", _viewModel.Plugins.First().Name);
        // Should not set error message for silent auto-refresh failure
        Assert.Null(_viewModel.ErrorMessage);
    }

    [Fact]
    public async Task LoadPluginsCommand_UpdatesLastSyncedAt()
    {
        var syncTime = DateTime.UtcNow.AddHours(-1);
        _marketplaceService.GetLastSyncTime().Returns(syncTime);
        _marketplaceService.GetPluginsAsync().Returns(new List<Plugin>());

        await _viewModel.LoadPluginsCommand.ExecuteAsync(null);

        Assert.Equal(syncTime, _viewModel.LastSyncedAt);
    }

    #endregion
}
