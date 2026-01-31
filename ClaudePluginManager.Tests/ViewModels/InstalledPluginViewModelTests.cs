using ClaudePluginManager.Models;
using ClaudePluginManager.ViewModels;

namespace ClaudePluginManager.Tests.ViewModels;

public class InstalledPluginViewModelTests
{
    private InstalledPlugin CreateTestPlugin()
    {
        return new InstalledPlugin
        {
            Id = "test-plugin",
            Name = "Test Plugin",
            Version = "1.0.0",
            Type = PluginType.McpServer,
            MarketplaceId = "default",
            InstalledAt = new DateTime(2024, 6, 15, 10, 30, 0)
        };
    }

    [Fact]
    public void Constructor_AcceptsInstalledPlugin()
    {
        var plugin = CreateTestPlugin();

        var viewModel = new InstalledPluginViewModel(plugin);

        Assert.NotNull(viewModel);
    }

    [Fact]
    public void Id_ReturnsPluginId()
    {
        var plugin = CreateTestPlugin();
        plugin.Id = "my-plugin-id";

        var viewModel = new InstalledPluginViewModel(plugin);

        Assert.Equal("my-plugin-id", viewModel.Id);
    }

    [Fact]
    public void Name_ReturnsPluginName()
    {
        var plugin = CreateTestPlugin();
        plugin.Name = "My Plugin";

        var viewModel = new InstalledPluginViewModel(plugin);

        Assert.Equal("My Plugin", viewModel.Name);
    }

    [Fact]
    public void Version_ReturnsPluginVersion()
    {
        var plugin = CreateTestPlugin();
        plugin.Version = "2.3.1";

        var viewModel = new InstalledPluginViewModel(plugin);

        Assert.Equal("2.3.1", viewModel.Version);
    }

    [Fact]
    public void Version_ReturnsNull_WhenNoVersion()
    {
        var plugin = CreateTestPlugin();
        plugin.Version = null;

        var viewModel = new InstalledPluginViewModel(plugin);

        Assert.Null(viewModel.Version);
    }

    [Fact]
    public void Type_ReturnsPluginType()
    {
        var plugin = CreateTestPlugin();
        plugin.Type = PluginType.Hook;

        var viewModel = new InstalledPluginViewModel(plugin);

        Assert.Equal(PluginType.Hook, viewModel.Type);
    }

    [Fact]
    public void DisplayType_McpServer_ReturnsMcpServer()
    {
        var plugin = CreateTestPlugin();
        plugin.Type = PluginType.McpServer;

        var viewModel = new InstalledPluginViewModel(plugin);

        Assert.Equal("MCP Server", viewModel.DisplayType);
    }

    [Fact]
    public void DisplayType_Hook_ReturnsHook()
    {
        var plugin = CreateTestPlugin();
        plugin.Type = PluginType.Hook;

        var viewModel = new InstalledPluginViewModel(plugin);

        Assert.Equal("Hook", viewModel.DisplayType);
    }

    [Fact]
    public void DisplayType_SlashCommand_ReturnsSlashCommand()
    {
        var plugin = CreateTestPlugin();
        plugin.Type = PluginType.SlashCommand;

        var viewModel = new InstalledPluginViewModel(plugin);

        Assert.Equal("Slash Command", viewModel.DisplayType);
    }

    [Fact]
    public void DisplayType_Agent_ReturnsAgent()
    {
        var plugin = CreateTestPlugin();
        plugin.Type = PluginType.Agent;

        var viewModel = new InstalledPluginViewModel(plugin);

        Assert.Equal("Agent", viewModel.DisplayType);
    }

    [Fact]
    public void DisplayType_Skill_ReturnsSkill()
    {
        var plugin = CreateTestPlugin();
        plugin.Type = PluginType.Skill;

        var viewModel = new InstalledPluginViewModel(plugin);

        Assert.Equal("Skill", viewModel.DisplayType);
    }

    [Fact]
    public void InstalledAt_ReturnsPluginInstalledAt()
    {
        var plugin = CreateTestPlugin();
        var expectedDate = new DateTime(2024, 1, 15, 14, 30, 0);
        plugin.InstalledAt = expectedDate;

        var viewModel = new InstalledPluginViewModel(plugin);

        Assert.Equal(expectedDate, viewModel.InstalledAt);
    }

    [Fact]
    public void InstalledAtDisplay_ReturnsFormattedDate()
    {
        var plugin = CreateTestPlugin();
        plugin.InstalledAt = new DateTime(2024, 6, 15, 10, 30, 0);

        var viewModel = new InstalledPluginViewModel(plugin);

        Assert.Equal("Jun 15, 2024", viewModel.InstalledAtDisplay);
    }

    [Fact]
    public void IsUninstalling_InitiallyFalse()
    {
        var plugin = CreateTestPlugin();

        var viewModel = new InstalledPluginViewModel(plugin);

        Assert.False(viewModel.IsUninstalling);
    }

    [Fact]
    public void IsUninstalling_CanBeSet()
    {
        var plugin = CreateTestPlugin();
        var viewModel = new InstalledPluginViewModel(plugin);

        viewModel.IsUninstalling = true;

        Assert.True(viewModel.IsUninstalling);
    }

    [Fact]
    public void InstalledPlugin_ReturnsOriginalPlugin()
    {
        var plugin = CreateTestPlugin();

        var viewModel = new InstalledPluginViewModel(plugin);

        Assert.Same(plugin, viewModel.InstalledPlugin);
    }

    #region Update Properties Tests

    [Fact]
    public void AvailableVersion_InitiallyNull()
    {
        var plugin = CreateTestPlugin();

        var viewModel = new InstalledPluginViewModel(plugin);

        Assert.Null(viewModel.AvailableVersion);
    }

    [Fact]
    public void AvailableVersion_CanBeSet()
    {
        var plugin = CreateTestPlugin();
        var viewModel = new InstalledPluginViewModel(plugin);

        viewModel.AvailableVersion = "2.0.0";

        Assert.Equal("2.0.0", viewModel.AvailableVersion);
    }

    [Fact]
    public void HasUpdate_InitiallyFalse()
    {
        var plugin = CreateTestPlugin();

        var viewModel = new InstalledPluginViewModel(plugin);

        Assert.False(viewModel.HasUpdate);
    }

    [Fact]
    public void HasUpdate_CanBeSet()
    {
        var plugin = CreateTestPlugin();
        var viewModel = new InstalledPluginViewModel(plugin);

        viewModel.HasUpdate = true;

        Assert.True(viewModel.HasUpdate);
    }

    [Fact]
    public void VersionDisplay_WhenNoUpdate_ReturnsVersionOnly()
    {
        var plugin = CreateTestPlugin();
        plugin.Version = "1.0.0";
        var viewModel = new InstalledPluginViewModel(plugin)
        {
            HasUpdate = false,
            AvailableVersion = null
        };

        Assert.Equal("1.0.0", viewModel.VersionDisplay);
    }

    [Fact]
    public void VersionDisplay_WhenHasUpdate_ReturnsVersionWithArrow()
    {
        var plugin = CreateTestPlugin();
        plugin.Version = "1.0.0";
        var viewModel = new InstalledPluginViewModel(plugin)
        {
            HasUpdate = true,
            AvailableVersion = "2.0.0"
        };

        Assert.Equal("1.0.0 → 2.0.0", viewModel.VersionDisplay);
    }

    [Fact]
    public void VersionDisplay_WhenVersionNull_ReturnsEmptyString()
    {
        var plugin = CreateTestPlugin();
        plugin.Version = null;
        var viewModel = new InstalledPluginViewModel(plugin)
        {
            HasUpdate = false
        };

        Assert.Equal("", viewModel.VersionDisplay);
    }

    [Fact]
    public void VersionDisplay_NotifiesPropertyChanged_WhenHasUpdateChanges()
    {
        var plugin = CreateTestPlugin();
        plugin.Version = "1.0.0";
        var viewModel = new InstalledPluginViewModel(plugin)
        {
            AvailableVersion = "2.0.0"
        };

        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(viewModel.VersionDisplay))
                propertyChangedRaised = true;
        };

        viewModel.HasUpdate = true;

        Assert.True(propertyChangedRaised);
    }

    [Fact]
    public void VersionDisplay_NotifiesPropertyChanged_WhenAvailableVersionChanges()
    {
        var plugin = CreateTestPlugin();
        plugin.Version = "1.0.0";
        var viewModel = new InstalledPluginViewModel(plugin)
        {
            HasUpdate = true
        };

        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(viewModel.VersionDisplay))
                propertyChangedRaised = true;
        };

        viewModel.AvailableVersion = "2.0.0";

        Assert.True(propertyChangedRaised);
    }

    #endregion
}
