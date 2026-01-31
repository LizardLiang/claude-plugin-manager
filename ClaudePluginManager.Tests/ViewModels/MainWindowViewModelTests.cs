using ClaudePluginManager.Models;
using ClaudePluginManager.Services;
using ClaudePluginManager.Tests.ViewModels;
using ClaudePluginManager.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace ClaudePluginManager.Tests.ViewModels;

public class MainWindowViewModelTests
{
    private static ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddSingleton(Substitute.For<IMarketplaceService>());
        services.AddSingleton(Substitute.For<IGitHubClient>());
        services.AddSingleton(Substitute.For<IPluginService>());
        services.AddSingleton(Substitute.For<IDialogService>());
        services.AddTransient<TestViewModel>();
        services.AddTransient<MarketplaceViewModel>();
        services.AddTransient<InstalledViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<PluginDetailsViewModel>();
        services.AddSingleton<Func<PluginDetailsViewModel>>(sp => () => sp.GetRequiredService<PluginDetailsViewModel>());
        return services.BuildServiceProvider();
    }

    [Fact]
    public void CurrentViewModel_ReturnsNavigationServiceCurrentViewModel()
    {
        var provider = CreateServiceProvider();
        var navigationService = new NavigationService(provider);
        var viewModel = new MainWindowViewModel(navigationService);

        navigationService.NavigateTo<TestViewModel>();

        Assert.Same(navigationService.CurrentViewModel, viewModel.CurrentViewModel);
    }

    [Fact]
    public void PropertyChanged_RaisedWhenNavigationServiceChanges()
    {
        var provider = CreateServiceProvider();
        var navigationService = new NavigationService(provider);
        var viewModel = new MainWindowViewModel(navigationService);

        var propertyChangedRaised = false;
        string? changedPropertyName = null;
        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
            changedPropertyName = args.PropertyName;
        };

        navigationService.NavigateTo<TestViewModel>();

        Assert.True(propertyChangedRaised);
        Assert.Equal(nameof(MainWindowViewModel.CurrentViewModel), changedPropertyName);
    }

    [Fact]
    public void Constructor_AcceptsNavigationService()
    {
        var navigationService = Substitute.For<INavigationService>();

        var viewModel = new MainWindowViewModel(navigationService);

        Assert.NotNull(viewModel);
    }

    [Fact]
    public void NavigationItems_ContainsThreeItems()
    {
        var navigationService = Substitute.For<INavigationService>();

        var viewModel = new MainWindowViewModel(navigationService);

        Assert.Equal(3, viewModel.NavigationItems.Count);
    }

    [Fact]
    public void NavigationItems_ContainsMarketplace()
    {
        var navigationService = Substitute.For<INavigationService>();

        var viewModel = new MainWindowViewModel(navigationService);

        Assert.Contains(viewModel.NavigationItems, item => item.Label == "Marketplace");
    }

    [Fact]
    public void NavigationItems_ContainsInstalled()
    {
        var navigationService = Substitute.For<INavigationService>();

        var viewModel = new MainWindowViewModel(navigationService);

        Assert.Contains(viewModel.NavigationItems, item => item.Label == "Installed");
    }

    [Fact]
    public void NavigationItems_ContainsSettings()
    {
        var navigationService = Substitute.For<INavigationService>();

        var viewModel = new MainWindowViewModel(navigationService);

        Assert.Contains(viewModel.NavigationItems, item => item.Label == "Settings");
    }

    [Fact]
    public void SelectedNavigationItem_InitiallyFirstItem()
    {
        var navigationService = Substitute.For<INavigationService>();

        var viewModel = new MainWindowViewModel(navigationService);

        Assert.Same(viewModel.NavigationItems[0], viewModel.SelectedNavigationItem);
    }

    [Fact]
    public void SelectedNavigationItem_FirstItemIsSelected()
    {
        var navigationService = Substitute.For<INavigationService>();

        var viewModel = new MainWindowViewModel(navigationService);

        Assert.True(viewModel.NavigationItems[0].IsSelected);
    }

    [Fact]
    public void NavigateCommand_ChangesSelectedItem()
    {
        var provider = CreateServiceProvider();
        var navigationService = new NavigationService(provider);
        var viewModel = new MainWindowViewModel(navigationService);

        var secondItem = viewModel.NavigationItems[1];
        viewModel.NavigateCommand.Execute(secondItem);

        Assert.Same(secondItem, viewModel.SelectedNavigationItem);
    }

    [Fact]
    public void NavigateCommand_UpdatesIsSelected()
    {
        var provider = CreateServiceProvider();
        var navigationService = new NavigationService(provider);
        var viewModel = new MainWindowViewModel(navigationService);

        var firstItem = viewModel.NavigationItems[0];
        var secondItem = viewModel.NavigationItems[1];
        viewModel.NavigateCommand.Execute(secondItem);

        Assert.False(firstItem.IsSelected);
        Assert.True(secondItem.IsSelected);
    }

    [Fact]
    public void NavigateCommand_NavigatesToCorrectViewModel()
    {
        var provider = CreateServiceProvider();
        var navigationService = new NavigationService(provider);
        var viewModel = new MainWindowViewModel(navigationService);

        var installedItem = viewModel.NavigationItems.First(i => i.Label == "Installed");
        viewModel.NavigateCommand.Execute(installedItem);

        Assert.IsType<InstalledViewModel>(viewModel.CurrentViewModel);
    }

    [Fact]
    public void Constructor_NavigatesToFirstItem()
    {
        var provider = CreateServiceProvider();
        var navigationService = new NavigationService(provider);

        var viewModel = new MainWindowViewModel(navigationService);

        Assert.IsType<MarketplaceViewModel>(viewModel.CurrentViewModel);
    }
}
