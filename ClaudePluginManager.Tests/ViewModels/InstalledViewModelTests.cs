using ClaudePluginManager.Models;
using ClaudePluginManager.Services;
using ClaudePluginManager.ViewModels;
using NSubstitute;
using System.Linq;

namespace ClaudePluginManager.Tests.ViewModels;

public class InstalledViewModelTests
{
    private readonly IPluginService _pluginService;
    private readonly IDialogService _dialogService;
    private readonly InstalledViewModel _viewModel;

    public InstalledViewModelTests()
    {
        _pluginService = Substitute.For<IPluginService>();
        _dialogService = Substitute.For<IDialogService>();
        _viewModel = new InstalledViewModel(_pluginService, _dialogService);
    }

    [Fact]
    public void InheritsFromViewModelBase()
    {
        Assert.IsAssignableFrom<ViewModelBase>(_viewModel);
    }

    [Fact]
    public void Title_ReturnsInstalled()
    {
        Assert.Equal("Installed", _viewModel.Title);
    }

    [Fact]
    public void Constructor_AcceptsServices()
    {
        var pluginService = Substitute.For<IPluginService>();
        var dialogService = Substitute.For<IDialogService>();
        var viewModel = new InstalledViewModel(pluginService, dialogService);

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
    public void ErrorMessage_InitiallyNull()
    {
        Assert.Null(_viewModel.ErrorMessage);
    }

    [Fact]
    public void IsEmpty_WhenNoPlugins_ReturnsTrue()
    {
        Assert.True(_viewModel.IsEmpty);
    }

    [Fact]
    public async Task IsEmpty_WhenPluginsExist_ReturnsFalse()
    {
        var pluginsWithUpdates = new List<InstalledPluginWithUpdate>
        {
            new InstalledPluginWithUpdate
            {
                InstalledPlugin = new InstalledPlugin { Id = "1", Name = "Plugin 1", Type = PluginType.McpServer }
            }
        };
        _pluginService.GetInstalledWithUpdatesAsync().Returns(pluginsWithUpdates);

        await _viewModel.LoadPluginsCommand.ExecuteAsync(null);

        Assert.False(_viewModel.IsEmpty);
    }

    [Fact]
    public async Task LoadPluginsCommand_SetsIsLoadingTrue_DuringLoad()
    {
        var tcs = new TaskCompletionSource<IReadOnlyList<InstalledPluginWithUpdate>>();
        _pluginService.GetInstalledWithUpdatesAsync().Returns(tcs.Task);

        var loadTask = _viewModel.LoadPluginsCommand.ExecuteAsync(null);

        Assert.True(_viewModel.IsLoading);

        tcs.SetResult(new List<InstalledPluginWithUpdate>());
        await loadTask;
    }

    [Fact]
    public async Task LoadPluginsCommand_SetsIsLoadingFalse_AfterLoad()
    {
        _pluginService.GetInstalledWithUpdatesAsync().Returns(new List<InstalledPluginWithUpdate>());

        await _viewModel.LoadPluginsCommand.ExecuteAsync(null);

        Assert.False(_viewModel.IsLoading);
    }

    [Fact]
    public async Task LoadPluginsCommand_PopulatesPlugins()
    {
        var pluginsWithUpdates = new List<InstalledPluginWithUpdate>
        {
            new InstalledPluginWithUpdate
            {
                InstalledPlugin = new InstalledPlugin { Id = "1", Name = "Plugin 1", Type = PluginType.McpServer }
            },
            new InstalledPluginWithUpdate
            {
                InstalledPlugin = new InstalledPlugin { Id = "2", Name = "Plugin 2", Type = PluginType.Hook }
            }
        };
        _pluginService.GetInstalledWithUpdatesAsync().Returns(pluginsWithUpdates);

        await _viewModel.LoadPluginsCommand.ExecuteAsync(null);

        Assert.Equal(2, _viewModel.Plugins.Count);
    }

    [Fact]
    public async Task LoadPluginsCommand_WrapsPluginsInViewModel()
    {
        var pluginsWithUpdates = new List<InstalledPluginWithUpdate>
        {
            new InstalledPluginWithUpdate
            {
                InstalledPlugin = new InstalledPlugin { Id = "test-id", Name = "Test Plugin", Type = PluginType.McpServer }
            }
        };
        _pluginService.GetInstalledWithUpdatesAsync().Returns(pluginsWithUpdates);

        await _viewModel.LoadPluginsCommand.ExecuteAsync(null);

        var pluginVm = _viewModel.Plugins.First();
        Assert.IsType<InstalledPluginViewModel>(pluginVm);
        Assert.Equal("test-id", pluginVm.Id);
        Assert.Equal("Test Plugin", pluginVm.Name);
    }

    [Fact]
    public async Task LoadPluginsCommand_OnException_SetsErrorMessage()
    {
        _pluginService.GetInstalledWithUpdatesAsync().Returns<IReadOnlyList<InstalledPluginWithUpdate>>(
            _ => throw new Exception("Database error"));

        await _viewModel.LoadPluginsCommand.ExecuteAsync(null);

        Assert.NotNull(_viewModel.ErrorMessage);
        Assert.Contains("Database error", _viewModel.ErrorMessage);
    }

    [Fact]
    public async Task LoadPluginsCommand_OnException_SetsIsLoadingFalse()
    {
        _pluginService.GetInstalledWithUpdatesAsync().Returns<IReadOnlyList<InstalledPluginWithUpdate>>(
            _ => throw new Exception("Database error"));

        await _viewModel.LoadPluginsCommand.ExecuteAsync(null);

        Assert.False(_viewModel.IsLoading);
    }

    [Fact]
    public async Task LoadPluginsCommand_ClearsErrorMessage_OnSuccessfulLoad()
    {
        var callCount = 0;
        _pluginService.GetInstalledWithUpdatesAsync().Returns(_ =>
        {
            callCount++;
            if (callCount == 1)
                throw new Exception("Database error");
            return Task.FromResult<IReadOnlyList<InstalledPluginWithUpdate>>(new List<InstalledPluginWithUpdate>());
        });

        await _viewModel.LoadPluginsCommand.ExecuteAsync(null);
        Assert.NotNull(_viewModel.ErrorMessage);

        await _viewModel.LoadPluginsCommand.ExecuteAsync(null);
        Assert.Null(_viewModel.ErrorMessage);
    }

    [Fact]
    public async Task IsEmpty_WhenErrorMessage_ReturnsFalse()
    {
        _pluginService.GetInstalledWithUpdatesAsync().Returns<IReadOnlyList<InstalledPluginWithUpdate>>(
            _ => throw new Exception("Database error"));

        await _viewModel.LoadPluginsCommand.ExecuteAsync(null);

        Assert.False(_viewModel.IsEmpty);
    }

    [Fact]
    public async Task LoadPluginsCommand_ClearsPreviousPlugins_BeforeLoading()
    {
        var plugins1 = new List<InstalledPluginWithUpdate>
        {
            new InstalledPluginWithUpdate
            {
                InstalledPlugin = new InstalledPlugin { Id = "1", Name = "Plugin 1", Type = PluginType.McpServer }
            }
        };
        var plugins2 = new List<InstalledPluginWithUpdate>
        {
            new InstalledPluginWithUpdate
            {
                InstalledPlugin = new InstalledPlugin { Id = "2", Name = "Plugin 2", Type = PluginType.Hook }
            }
        };

        _pluginService.GetInstalledWithUpdatesAsync().Returns(plugins1);
        await _viewModel.LoadPluginsCommand.ExecuteAsync(null);

        _pluginService.GetInstalledWithUpdatesAsync().Returns(plugins2);
        await _viewModel.LoadPluginsCommand.ExecuteAsync(null);

        Assert.Single(_viewModel.Plugins);
        Assert.Equal("2", _viewModel.Plugins.First().Id);
    }

    [Fact]
    public async Task UninstallCommand_ShowsConfirmationDialog()
    {
        var pluginsWithUpdates = new List<InstalledPluginWithUpdate>
        {
            new InstalledPluginWithUpdate
            {
                InstalledPlugin = new InstalledPlugin { Id = "1", Name = "Test Plugin", Type = PluginType.McpServer }
            }
        };
        _pluginService.GetInstalledWithUpdatesAsync().Returns(pluginsWithUpdates);
        await _viewModel.LoadPluginsCommand.ExecuteAsync(null);

        var pluginVm = _viewModel.Plugins.First();
        _dialogService.ConfirmAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        await _viewModel.UninstallCommand.ExecuteAsync(pluginVm);

        await _dialogService.Received(1).ConfirmAsync(
            Arg.Is<string>(s => s.Contains("Uninstall")),
            Arg.Is<string>(s => s.Contains("Test Plugin")));
    }

    [Fact]
    public async Task UninstallCommand_WhenConfirmed_CallsUninstallGlobalAsync()
    {
        var pluginsWithUpdates = new List<InstalledPluginWithUpdate>
        {
            new InstalledPluginWithUpdate
            {
                InstalledPlugin = new InstalledPlugin { Id = "plugin-1", Name = "Test Plugin", Type = PluginType.McpServer }
            }
        };
        _pluginService.GetInstalledWithUpdatesAsync().Returns(pluginsWithUpdates);
        _pluginService.UninstallGlobalAsync("plugin-1").Returns(UninstallResult.Ok());
        _dialogService.ConfirmAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(true);

        await _viewModel.LoadPluginsCommand.ExecuteAsync(null);
        var pluginVm = _viewModel.Plugins.First();

        await _viewModel.UninstallCommand.ExecuteAsync(pluginVm);

        await _pluginService.Received(1).UninstallGlobalAsync("plugin-1");
    }

    [Fact]
    public async Task UninstallCommand_WhenNotConfirmed_DoesNotCallUninstall()
    {
        var pluginsWithUpdates = new List<InstalledPluginWithUpdate>
        {
            new InstalledPluginWithUpdate
            {
                InstalledPlugin = new InstalledPlugin { Id = "plugin-1", Name = "Test Plugin", Type = PluginType.McpServer }
            }
        };
        _pluginService.GetInstalledWithUpdatesAsync().Returns(pluginsWithUpdates);
        _dialogService.ConfirmAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        await _viewModel.LoadPluginsCommand.ExecuteAsync(null);
        var pluginVm = _viewModel.Plugins.First();

        await _viewModel.UninstallCommand.ExecuteAsync(pluginVm);

        await _pluginService.DidNotReceive().UninstallGlobalAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task UninstallCommand_WhenSuccessful_RemovesPluginFromList()
    {
        var pluginsWithUpdates = new List<InstalledPluginWithUpdate>
        {
            new InstalledPluginWithUpdate
            {
                InstalledPlugin = new InstalledPlugin { Id = "plugin-1", Name = "Plugin 1", Type = PluginType.McpServer }
            },
            new InstalledPluginWithUpdate
            {
                InstalledPlugin = new InstalledPlugin { Id = "plugin-2", Name = "Plugin 2", Type = PluginType.Hook }
            }
        };
        _pluginService.GetInstalledWithUpdatesAsync().Returns(pluginsWithUpdates);
        _pluginService.UninstallGlobalAsync("plugin-1").Returns(UninstallResult.Ok());
        _dialogService.ConfirmAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(true);

        await _viewModel.LoadPluginsCommand.ExecuteAsync(null);
        var pluginVm = _viewModel.Plugins.First(p => p.Id == "plugin-1");

        await _viewModel.UninstallCommand.ExecuteAsync(pluginVm);

        Assert.Single(_viewModel.Plugins);
        Assert.DoesNotContain(_viewModel.Plugins, p => p.Id == "plugin-1");
    }

    [Fact]
    public async Task UninstallCommand_SetsIsUninstalling_DuringUninstall()
    {
        var pluginsWithUpdates = new List<InstalledPluginWithUpdate>
        {
            new InstalledPluginWithUpdate
            {
                InstalledPlugin = new InstalledPlugin { Id = "plugin-1", Name = "Test Plugin", Type = PluginType.McpServer }
            }
        };
        _pluginService.GetInstalledWithUpdatesAsync().Returns(pluginsWithUpdates);
        _dialogService.ConfirmAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(true);

        var tcs = new TaskCompletionSource<UninstallResult>();
        _pluginService.UninstallGlobalAsync("plugin-1").Returns(tcs.Task);

        await _viewModel.LoadPluginsCommand.ExecuteAsync(null);
        var pluginVm = _viewModel.Plugins.First();

        var uninstallTask = _viewModel.UninstallCommand.ExecuteAsync(pluginVm);

        Assert.True(pluginVm.IsUninstalling);

        tcs.SetResult(UninstallResult.Ok());
        await uninstallTask;
    }

    [Fact]
    public async Task UninstallCommand_OnFailure_SetsErrorMessage()
    {
        var pluginsWithUpdates = new List<InstalledPluginWithUpdate>
        {
            new InstalledPluginWithUpdate
            {
                InstalledPlugin = new InstalledPlugin { Id = "plugin-1", Name = "Test Plugin", Type = PluginType.McpServer }
            }
        };
        _pluginService.GetInstalledWithUpdatesAsync().Returns(pluginsWithUpdates);
        _pluginService.UninstallGlobalAsync("plugin-1").Returns(UninstallResult.Fail("Permission denied"));
        _dialogService.ConfirmAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(true);

        await _viewModel.LoadPluginsCommand.ExecuteAsync(null);
        var pluginVm = _viewModel.Plugins.First();

        await _viewModel.UninstallCommand.ExecuteAsync(pluginVm);

        Assert.NotNull(_viewModel.ErrorMessage);
        Assert.Contains("Permission denied", _viewModel.ErrorMessage);
    }

    [Fact]
    public async Task UninstallCommand_OnFailure_KeepsPluginInList()
    {
        var pluginsWithUpdates = new List<InstalledPluginWithUpdate>
        {
            new InstalledPluginWithUpdate
            {
                InstalledPlugin = new InstalledPlugin { Id = "plugin-1", Name = "Test Plugin", Type = PluginType.McpServer }
            }
        };
        _pluginService.GetInstalledWithUpdatesAsync().Returns(pluginsWithUpdates);
        _pluginService.UninstallGlobalAsync("plugin-1").Returns(UninstallResult.Fail("Failed"));
        _dialogService.ConfirmAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(true);

        await _viewModel.LoadPluginsCommand.ExecuteAsync(null);
        var pluginVm = _viewModel.Plugins.First();

        await _viewModel.UninstallCommand.ExecuteAsync(pluginVm);

        Assert.Single(_viewModel.Plugins);
        Assert.Contains(_viewModel.Plugins, p => p.Id == "plugin-1");
    }

    [Fact]
    public async Task UninstallCommand_OnFailure_ClearsIsUninstalling()
    {
        var pluginsWithUpdates = new List<InstalledPluginWithUpdate>
        {
            new InstalledPluginWithUpdate
            {
                InstalledPlugin = new InstalledPlugin { Id = "plugin-1", Name = "Test Plugin", Type = PluginType.McpServer }
            }
        };
        _pluginService.GetInstalledWithUpdatesAsync().Returns(pluginsWithUpdates);
        _pluginService.UninstallGlobalAsync("plugin-1").Returns(UninstallResult.Fail("Failed"));
        _dialogService.ConfirmAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(true);

        await _viewModel.LoadPluginsCommand.ExecuteAsync(null);
        var pluginVm = _viewModel.Plugins.First();

        await _viewModel.UninstallCommand.ExecuteAsync(pluginVm);

        Assert.False(pluginVm.IsUninstalling);
    }

    [Fact]
    public async Task UninstallCommand_WithNullPlugin_DoesNothing()
    {
        await _viewModel.UninstallCommand.ExecuteAsync(null);

        await _dialogService.DidNotReceive().ConfirmAsync(Arg.Any<string>(), Arg.Any<string>());
        await _pluginService.DidNotReceive().UninstallGlobalAsync(Arg.Any<string>());
    }

    #region Update Detection Tests

    [Fact]
    public async Task LoadPluginsCommand_UsesGetInstalledWithUpdatesAsync()
    {
        var pluginsWithUpdates = new List<InstalledPluginWithUpdate>
        {
            new InstalledPluginWithUpdate
            {
                InstalledPlugin = new InstalledPlugin { Id = "1", Name = "Plugin 1", Type = PluginType.McpServer, Version = "1.0.0" },
                HasUpdate = true,
                AvailableVersion = "2.0.0"
            }
        };
        _pluginService.GetInstalledWithUpdatesAsync().Returns(pluginsWithUpdates);

        await _viewModel.LoadPluginsCommand.ExecuteAsync(null);

        await _pluginService.Received(1).GetInstalledWithUpdatesAsync();
    }

    [Fact]
    public async Task LoadPluginsCommand_SetsHasUpdateOnViewModel()
    {
        var pluginsWithUpdates = new List<InstalledPluginWithUpdate>
        {
            new InstalledPluginWithUpdate
            {
                InstalledPlugin = new InstalledPlugin { Id = "1", Name = "Plugin 1", Type = PluginType.McpServer, Version = "1.0.0" },
                HasUpdate = true,
                AvailableVersion = "2.0.0"
            }
        };
        _pluginService.GetInstalledWithUpdatesAsync().Returns(pluginsWithUpdates);

        await _viewModel.LoadPluginsCommand.ExecuteAsync(null);

        var pluginVm = _viewModel.Plugins.First();
        Assert.True(pluginVm.HasUpdate);
        Assert.Equal("2.0.0", pluginVm.AvailableVersion);
    }

    [Fact]
    public async Task LoadPluginsCommand_SetsHasUpdateFalse_WhenNoUpdate()
    {
        var pluginsWithUpdates = new List<InstalledPluginWithUpdate>
        {
            new InstalledPluginWithUpdate
            {
                InstalledPlugin = new InstalledPlugin { Id = "1", Name = "Plugin 1", Type = PluginType.McpServer, Version = "1.0.0" },
                HasUpdate = false,
                AvailableVersion = null
            }
        };
        _pluginService.GetInstalledWithUpdatesAsync().Returns(pluginsWithUpdates);

        await _viewModel.LoadPluginsCommand.ExecuteAsync(null);

        var pluginVm = _viewModel.Plugins.First();
        Assert.False(pluginVm.HasUpdate);
        Assert.Null(pluginVm.AvailableVersion);
    }

    [Fact]
    public void UpdatesAvailableCount_InitiallyZero()
    {
        Assert.Equal(0, _viewModel.UpdatesAvailableCount);
    }

    [Fact]
    public async Task UpdatesAvailableCount_ReturnsCorrectCount()
    {
        var pluginsWithUpdates = new List<InstalledPluginWithUpdate>
        {
            new InstalledPluginWithUpdate
            {
                InstalledPlugin = new InstalledPlugin { Id = "1", Name = "Plugin 1", Type = PluginType.McpServer },
                HasUpdate = true,
                AvailableVersion = "2.0.0"
            },
            new InstalledPluginWithUpdate
            {
                InstalledPlugin = new InstalledPlugin { Id = "2", Name = "Plugin 2", Type = PluginType.Hook },
                HasUpdate = false,
                AvailableVersion = null
            },
            new InstalledPluginWithUpdate
            {
                InstalledPlugin = new InstalledPlugin { Id = "3", Name = "Plugin 3", Type = PluginType.Agent },
                HasUpdate = true,
                AvailableVersion = "3.0.0"
            }
        };
        _pluginService.GetInstalledWithUpdatesAsync().Returns(pluginsWithUpdates);

        await _viewModel.LoadPluginsCommand.ExecuteAsync(null);

        Assert.Equal(2, _viewModel.UpdatesAvailableCount);
    }

    [Fact]
    public async Task UpdatesAvailableCount_IsZero_WhenNoUpdates()
    {
        var pluginsWithUpdates = new List<InstalledPluginWithUpdate>
        {
            new InstalledPluginWithUpdate
            {
                InstalledPlugin = new InstalledPlugin { Id = "1", Name = "Plugin 1", Type = PluginType.McpServer },
                HasUpdate = false
            }
        };
        _pluginService.GetInstalledWithUpdatesAsync().Returns(pluginsWithUpdates);

        await _viewModel.LoadPluginsCommand.ExecuteAsync(null);

        Assert.Equal(0, _viewModel.UpdatesAvailableCount);
    }

    #endregion
}
