using ClaudePluginManager.Models;
using ClaudePluginManager.ViewModels;

namespace ClaudePluginManager.Tests.ViewModels;

public class PluginListItemViewModelTests
{
    private static Plugin CreatePlugin(
        string id = "test-id",
        string name = "Test Plugin",
        string? description = "Test description",
        string? version = "1.0.0",
        string? author = "Test Author",
        string type = "MCP_SERVER")
    {
        return new Plugin
        {
            Id = id,
            Name = name,
            Description = description,
            Version = version,
            Author = author,
            Type = type
        };
    }

    [Fact]
    public void InheritsFromViewModelBase()
    {
        var plugin = CreatePlugin();
        var viewModel = new PluginListItemViewModel(plugin);

        Assert.IsAssignableFrom<ViewModelBase>(viewModel);
    }

    [Fact]
    public void Id_ReturnsPluginId()
    {
        var plugin = CreatePlugin(id: "my-plugin-id");
        var viewModel = new PluginListItemViewModel(plugin);

        Assert.Equal("my-plugin-id", viewModel.Id);
    }

    [Fact]
    public void Name_ReturnsPluginName()
    {
        var plugin = CreatePlugin(name: "My Plugin");
        var viewModel = new PluginListItemViewModel(plugin);

        Assert.Equal("My Plugin", viewModel.Name);
    }

    [Fact]
    public void Description_ReturnsPluginDescription()
    {
        var plugin = CreatePlugin(description: "My description");
        var viewModel = new PluginListItemViewModel(plugin);

        Assert.Equal("My description", viewModel.Description);
    }

    [Fact]
    public void Description_WhenNull_ReturnsEmptyString()
    {
        var plugin = CreatePlugin(description: null);
        var viewModel = new PluginListItemViewModel(plugin);

        Assert.Equal(string.Empty, viewModel.Description);
    }

    [Fact]
    public void Version_ReturnsPluginVersion()
    {
        var plugin = CreatePlugin(version: "2.0.0");
        var viewModel = new PluginListItemViewModel(plugin);

        Assert.Equal("2.0.0", viewModel.Version);
    }

    [Fact]
    public void Version_WhenNull_ReturnsUnknown()
    {
        var plugin = CreatePlugin(version: null);
        var viewModel = new PluginListItemViewModel(plugin);

        Assert.Equal("Unknown", viewModel.Version);
    }

    [Fact]
    public void Author_ReturnsPluginAuthor()
    {
        var plugin = CreatePlugin(author: "John Doe");
        var viewModel = new PluginListItemViewModel(plugin);

        Assert.Equal("John Doe", viewModel.Author);
    }

    [Fact]
    public void Author_WhenNull_ReturnsUnknown()
    {
        var plugin = CreatePlugin(author: null);
        var viewModel = new PluginListItemViewModel(plugin);

        Assert.Equal("Unknown", viewModel.Author);
    }

    [Theory]
    [InlineData("MCP_SERVER", "MCP Server")]
    [InlineData("HOOK", "Hook")]
    [InlineData("SLASH_COMMAND", "Slash Command")]
    [InlineData("AGENT", "Agent")]
    [InlineData("SKILL", "Skill")]
    public void DisplayType_FormatsTypeForDisplay(string type, string expected)
    {
        var plugin = CreatePlugin(type: type);
        var viewModel = new PluginListItemViewModel(plugin);

        Assert.Equal(expected, viewModel.DisplayType);
    }

    [Fact]
    public void DisplayType_UnknownType_ReplacesUnderscoresWithSpaces()
    {
        var plugin = CreatePlugin(type: "SOME_UNKNOWN_TYPE");
        var viewModel = new PluginListItemViewModel(plugin);

        Assert.Equal("SOME UNKNOWN TYPE", viewModel.DisplayType);
    }
}
