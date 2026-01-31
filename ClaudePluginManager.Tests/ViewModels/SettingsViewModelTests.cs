using ClaudePluginManager.ViewModels;

namespace ClaudePluginManager.Tests.ViewModels;

public class SettingsViewModelTests
{
    [Fact]
    public void InheritsFromViewModelBase()
    {
        var viewModel = new SettingsViewModel();

        Assert.IsAssignableFrom<ViewModelBase>(viewModel);
    }

    [Fact]
    public void Title_ReturnsSettings()
    {
        var viewModel = new SettingsViewModel();

        Assert.Equal("Settings", viewModel.Title);
    }
}
