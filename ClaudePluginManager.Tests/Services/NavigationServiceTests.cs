using System.ComponentModel;
using ClaudePluginManager.Services;
using ClaudePluginManager.Tests.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ClaudePluginManager.Tests.Services;

public class NavigationServiceTests
{
    private readonly ServiceProvider _serviceProvider;
    private readonly NavigationService _navigationService;

    public NavigationServiceTests()
    {
        var services = new ServiceCollection();
        services.AddTransient<TestViewModel>();
        _serviceProvider = services.BuildServiceProvider();
        _navigationService = new NavigationService(_serviceProvider);
    }

    [Fact]
    public void CurrentViewModel_InitiallyNull()
    {
        Assert.Null(_navigationService.CurrentViewModel);
    }

    [Fact]
    public void NavigateTo_SetsCurrentViewModel()
    {
        _navigationService.NavigateTo<TestViewModel>();

        Assert.NotNull(_navigationService.CurrentViewModel);
        Assert.IsType<TestViewModel>(_navigationService.CurrentViewModel);
    }

    [Fact]
    public void NavigateTo_ResolvesNewInstanceEachTime()
    {
        _navigationService.NavigateTo<TestViewModel>();
        var first = _navigationService.CurrentViewModel;

        _navigationService.NavigateTo<TestViewModel>();
        var second = _navigationService.CurrentViewModel;

        Assert.NotSame(first, second);
    }

    [Fact]
    public void NavigateTo_RaisesPropertyChanged()
    {
        var propertyChangedRaised = false;
        string? changedPropertyName = null;

        _navigationService.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
            changedPropertyName = args.PropertyName;
        };

        _navigationService.NavigateTo<TestViewModel>();

        Assert.True(propertyChangedRaised);
        Assert.Equal(nameof(INavigationService.CurrentViewModel), changedPropertyName);
    }

    [Fact]
    public void NavigateTo_ThrowsWhenViewModelNotRegistered()
    {
        var emptyServices = new ServiceCollection().BuildServiceProvider();
        var navigationService = new NavigationService(emptyServices);

        Assert.Throws<InvalidOperationException>(() => navigationService.NavigateTo<TestViewModel>());
    }
}
