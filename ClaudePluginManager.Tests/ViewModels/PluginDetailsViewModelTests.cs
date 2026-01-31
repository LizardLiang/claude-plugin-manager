using ClaudePluginManager.Models;
using ClaudePluginManager.Services;
using ClaudePluginManager.ViewModels;
using NSubstitute;

namespace ClaudePluginManager.Tests.ViewModels;

public class PluginDetailsViewModelTests
{
    private readonly IGitHubClient _gitHubClient;
    private readonly PluginDetailsViewModel _viewModel;

    public PluginDetailsViewModelTests()
    {
        _gitHubClient = Substitute.For<IGitHubClient>();
        _viewModel = new PluginDetailsViewModel(_gitHubClient);
    }

    [Fact]
    public void InheritsFromViewModelBase()
    {
        Assert.IsAssignableFrom<ViewModelBase>(_viewModel);
    }

    [Fact]
    public void Constructor_AcceptsGitHubClient()
    {
        var client = Substitute.For<IGitHubClient>();
        var viewModel = new PluginDetailsViewModel(client);

        Assert.NotNull(viewModel);
    }

    [Fact]
    public void InitialState_AllPropertiesEmpty()
    {
        Assert.Equal(string.Empty, _viewModel.Name);
        Assert.Equal(string.Empty, _viewModel.Version);
        Assert.Equal(string.Empty, _viewModel.Author);
        Assert.Equal(string.Empty, _viewModel.Description);
        Assert.Equal(string.Empty, _viewModel.Type);
        Assert.Equal(string.Empty, _viewModel.DisplayType);
        Assert.Null(_viewModel.RepositoryUrl);
        Assert.Empty(_viewModel.Tags);
        Assert.Null(_viewModel.ReadmeContent);
        Assert.False(_viewModel.IsLoading);
        Assert.False(_viewModel.HasNoReadme);
    }

    [Fact]
    public async Task InitializeAsync_SetsPluginProperties()
    {
        var plugin = new Plugin
        {
            Id = "test-id",
            Name = "Test Plugin",
            Version = "1.0.0",
            Author = "Test Author",
            Description = "Test description",
            Type = "MCP_SERVER",
            Repository = "https://github.com/owner/repo",
            Tags = "[\"tag1\", \"tag2\"]"
        };

        await _viewModel.InitializeAsync(plugin);

        Assert.Equal("Test Plugin", _viewModel.Name);
        Assert.Equal("1.0.0", _viewModel.Version);
        Assert.Equal("Test Author", _viewModel.Author);
        Assert.Equal("Test description", _viewModel.Description);
        Assert.Equal("MCP_SERVER", _viewModel.Type);
        Assert.Equal("MCP Server", _viewModel.DisplayType);
        Assert.Equal("https://github.com/owner/repo", _viewModel.RepositoryUrl);
        Assert.Equal(2, _viewModel.Tags.Count);
        Assert.Contains("tag1", _viewModel.Tags);
        Assert.Contains("tag2", _viewModel.Tags);
    }

    [Fact]
    public async Task InitializeAsync_WithNullValues_UsesDefaults()
    {
        var plugin = new Plugin
        {
            Id = "test-id",
            Name = "Test Plugin",
            Type = "HOOK"
        };

        await _viewModel.InitializeAsync(plugin);

        Assert.Equal("Test Plugin", _viewModel.Name);
        Assert.Equal("Unknown", _viewModel.Version);
        Assert.Equal("Unknown", _viewModel.Author);
        Assert.Equal(string.Empty, _viewModel.Description);
        Assert.Equal("Hook", _viewModel.DisplayType);
        Assert.Null(_viewModel.RepositoryUrl);
        Assert.Empty(_viewModel.Tags);
    }

    [Fact]
    public async Task InitializeAsync_FetchesReadmeFromGitHub()
    {
        var plugin = new Plugin
        {
            Id = "test-id",
            Name = "Test Plugin",
            Type = "MCP_SERVER",
            Repository = "https://github.com/owner/repo"
        };
        _gitHubClient.GetReadmeAsync("owner", "repo")
            .Returns("# README\nThis is a test readme.");

        await _viewModel.InitializeAsync(plugin);

        Assert.Equal("# README\nThis is a test readme.", _viewModel.ReadmeContent);
        Assert.False(_viewModel.HasNoReadme);
    }

    [Fact]
    public async Task InitializeAsync_SetsIsLoadingDuringReadmeFetch()
    {
        var tcs = new TaskCompletionSource<string?>();
        var plugin = new Plugin
        {
            Id = "test-id",
            Name = "Test Plugin",
            Type = "MCP_SERVER",
            Repository = "https://github.com/owner/repo"
        };
        _gitHubClient.GetReadmeAsync("owner", "repo").Returns(tcs.Task);

        var initTask = _viewModel.InitializeAsync(plugin);

        Assert.True(_viewModel.IsLoading);

        tcs.SetResult("README content");
        await initTask;

        Assert.False(_viewModel.IsLoading);
    }

    [Fact]
    public async Task InitializeAsync_WhenNoReadme_SetsHasNoReadme()
    {
        var plugin = new Plugin
        {
            Id = "test-id",
            Name = "Test Plugin",
            Type = "MCP_SERVER",
            Repository = "https://github.com/owner/repo"
        };
        _gitHubClient.GetReadmeAsync("owner", "repo").Returns((string?)null);

        await _viewModel.InitializeAsync(plugin);

        Assert.True(_viewModel.HasNoReadme);
        Assert.Null(_viewModel.ReadmeContent);
    }

    [Fact]
    public async Task InitializeAsync_WhenNoGitHubUrl_DoesNotFetchReadme()
    {
        var plugin = new Plugin
        {
            Id = "test-id",
            Name = "Test Plugin",
            Type = "MCP_SERVER",
            Repository = "https://gitlab.com/owner/repo"
        };

        await _viewModel.InitializeAsync(plugin);

        await _gitHubClient.DidNotReceive().GetReadmeAsync(Arg.Any<string>(), Arg.Any<string>());
        Assert.True(_viewModel.HasNoReadme);
    }

    [Fact]
    public async Task InitializeAsync_WhenNullRepository_DoesNotFetchReadme()
    {
        var plugin = new Plugin
        {
            Id = "test-id",
            Name = "Test Plugin",
            Type = "MCP_SERVER",
            Repository = null
        };

        await _viewModel.InitializeAsync(plugin);

        await _gitHubClient.DidNotReceive().GetReadmeAsync(Arg.Any<string>(), Arg.Any<string>());
        Assert.True(_viewModel.HasNoReadme);
    }

    [Fact]
    public async Task InitializeAsync_WhenGitHubThrows_SetsHasNoReadme()
    {
        var plugin = new Plugin
        {
            Id = "test-id",
            Name = "Test Plugin",
            Type = "MCP_SERVER",
            Repository = "https://github.com/owner/repo"
        };
        _gitHubClient.GetReadmeAsync("owner", "repo")
            .Returns<string?>(_ => throw new Exception("Network error"));

        await _viewModel.InitializeAsync(plugin);

        Assert.True(_viewModel.HasNoReadme);
        Assert.Null(_viewModel.ReadmeContent);
        Assert.False(_viewModel.IsLoading);
    }

    [Theory]
    [InlineData("MCP_SERVER", "MCP Server")]
    [InlineData("HOOK", "Hook")]
    [InlineData("SLASH_COMMAND", "Slash Command")]
    [InlineData("AGENT", "Agent")]
    [InlineData("SKILL", "Skill")]
    [InlineData("UNKNOWN_TYPE", "UNKNOWN TYPE")]
    public async Task InitializeAsync_FormatsDisplayType(string type, string expectedDisplay)
    {
        var plugin = new Plugin
        {
            Id = "test-id",
            Name = "Test Plugin",
            Type = type
        };

        await _viewModel.InitializeAsync(plugin);

        Assert.Equal(expectedDisplay, _viewModel.DisplayType);
    }

    [Fact]
    public async Task InitializeAsync_ParsesTags()
    {
        var plugin = new Plugin
        {
            Id = "test-id",
            Name = "Test Plugin",
            Type = "MCP_SERVER",
            Tags = "[\"cli\", \"productivity\", \"tools\"]"
        };

        await _viewModel.InitializeAsync(plugin);

        Assert.Equal(3, _viewModel.Tags.Count);
        Assert.Contains("cli", _viewModel.Tags);
        Assert.Contains("productivity", _viewModel.Tags);
        Assert.Contains("tools", _viewModel.Tags);
    }

    [Fact]
    public async Task InitializeAsync_WithInvalidTags_ReturnsEmptyList()
    {
        var plugin = new Plugin
        {
            Id = "test-id",
            Name = "Test Plugin",
            Type = "MCP_SERVER",
            Tags = "not valid json"
        };

        await _viewModel.InitializeAsync(plugin);

        Assert.Empty(_viewModel.Tags);
    }

    [Fact]
    public void CloseCommand_Exists()
    {
        Assert.NotNull(_viewModel.CloseCommand);
    }

    [Fact]
    public void CloseCommand_RaisesCloseRequested()
    {
        var raised = false;
        _viewModel.CloseRequested += (s, e) => raised = true;

        _viewModel.CloseCommand.Execute(null);

        Assert.True(raised);
    }

    [Fact]
    public void IsInstalled_InitiallyFalse()
    {
        Assert.False(_viewModel.IsInstalled);
    }

    [Fact]
    public void HasRepositoryUrl_WhenNull_ReturnsFalse()
    {
        Assert.False(_viewModel.HasRepositoryUrl);
    }

    [Fact]
    public async Task HasRepositoryUrl_WhenSet_ReturnsTrue()
    {
        var plugin = new Plugin
        {
            Id = "test-id",
            Name = "Test Plugin",
            Type = "MCP_SERVER",
            Repository = "https://github.com/owner/repo"
        };

        await _viewModel.InitializeAsync(plugin);

        Assert.True(_viewModel.HasRepositoryUrl);
    }

    [Fact]
    public async Task InitializeAsync_ClearsExistingReadme_BeforeLoading()
    {
        var plugin1 = new Plugin
        {
            Id = "test-id-1",
            Name = "Plugin 1",
            Type = "MCP_SERVER",
            Repository = "https://github.com/owner/repo1"
        };
        _gitHubClient.GetReadmeAsync("owner", "repo1").Returns("README 1");
        await _viewModel.InitializeAsync(plugin1);
        Assert.Equal("README 1", _viewModel.ReadmeContent);

        var tcs = new TaskCompletionSource<string?>();
        var plugin2 = new Plugin
        {
            Id = "test-id-2",
            Name = "Plugin 2",
            Type = "MCP_SERVER",
            Repository = "https://github.com/owner/repo2"
        };
        _gitHubClient.GetReadmeAsync("owner", "repo2").Returns(tcs.Task);

        var initTask = _viewModel.InitializeAsync(plugin2);

        // During loading, readme should be cleared
        Assert.Null(_viewModel.ReadmeContent);
        Assert.False(_viewModel.HasNoReadme);

        tcs.SetResult("README 2");
        await initTask;
    }
}
