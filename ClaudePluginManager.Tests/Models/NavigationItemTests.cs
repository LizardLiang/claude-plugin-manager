using ClaudePluginManager.Models;
using ClaudePluginManager.ViewModels;

namespace ClaudePluginManager.Tests.Models;

public class NavigationItemTests
{
    [Fact]
    public void Constructor_SetsLabel()
    {
        var item = new NavigationItem("Test", "icon", typeof(MarketplaceViewModel));

        Assert.Equal("Test", item.Label);
    }

    [Fact]
    public void Constructor_SetsIcon()
    {
        var item = new NavigationItem("Test", "M0 0", typeof(MarketplaceViewModel));

        Assert.Equal("M0 0", item.Icon);
    }

    [Fact]
    public void Constructor_SetsViewModelType()
    {
        var item = new NavigationItem("Test", "icon", typeof(MarketplaceViewModel));

        Assert.Equal(typeof(MarketplaceViewModel), item.ViewModelType);
    }

    [Fact]
    public void IsSelected_DefaultsFalse()
    {
        var item = new NavigationItem("Test", "icon", typeof(MarketplaceViewModel));

        Assert.False(item.IsSelected);
    }

    [Fact]
    public void IsSelected_CanBeSet()
    {
        var item = new NavigationItem("Test", "icon", typeof(MarketplaceViewModel));

        item.IsSelected = true;

        Assert.True(item.IsSelected);
    }

    [Fact]
    public void IsSelected_RaisesPropertyChanged()
    {
        var item = new NavigationItem("Test", "icon", typeof(MarketplaceViewModel));
        var raised = false;

        item.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(NavigationItem.IsSelected))
                raised = true;
        };

        item.IsSelected = true;

        Assert.True(raised);
    }
}
