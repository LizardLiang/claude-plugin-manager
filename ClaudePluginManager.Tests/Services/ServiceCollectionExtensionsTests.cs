using ClaudePluginManager.Data;
using ClaudePluginManager.Services;
using ClaudePluginManager.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ClaudePluginManager.Tests.Services;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddServices_RegistersNavigationServiceAsSingleton()
    {
        var services = new ServiceCollection();

        services.AddServices();

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(INavigationService));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
        Assert.Equal(typeof(NavigationService), descriptor.ImplementationType);
    }

    [Fact]
    public void AddServices_RegistersDatabaseServiceAsSingleton()
    {
        var services = new ServiceCollection();

        services.AddServices();

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IDatabaseService));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
        Assert.Equal(typeof(DatabaseService), descriptor.ImplementationType);
    }

    [Fact]
    public void AddServices_RegistersConfigServiceAsSingleton()
    {
        var services = new ServiceCollection();

        services.AddServices();

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IConfigService));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
        Assert.Equal(typeof(ConfigService), descriptor.ImplementationType);
    }

    [Fact]
    public void AddServices_RegistersPluginServiceAsSingleton()
    {
        var services = new ServiceCollection();

        services.AddServices();

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IPluginService));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
        Assert.Equal(typeof(PluginService), descriptor.ImplementationType);
    }

    [Fact]
    public void AddServices_ReturnsSameCollection()
    {
        var services = new ServiceCollection();

        var result = services.AddServices();

        Assert.Same(services, result);
    }

    [Fact]
    public void AddViewModels_RegistersMainWindowViewModelAsTransient()
    {
        var services = new ServiceCollection();

        services.AddViewModels();

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(MainWindowViewModel));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Transient, descriptor.Lifetime);
    }

    [Fact]
    public void AddViewModels_RegistersMarketplaceViewModelAsTransient()
    {
        var services = new ServiceCollection();

        services.AddViewModels();

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(MarketplaceViewModel));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Transient, descriptor.Lifetime);
    }

    [Fact]
    public void AddViewModels_RegistersInstalledViewModelAsTransient()
    {
        var services = new ServiceCollection();

        services.AddViewModels();

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(InstalledViewModel));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Transient, descriptor.Lifetime);
    }

    [Fact]
    public void AddViewModels_RegistersSettingsViewModelAsTransient()
    {
        var services = new ServiceCollection();

        services.AddViewModels();

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(SettingsViewModel));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Transient, descriptor.Lifetime);
    }

    [Fact]
    public void AddViewModels_ReturnsSameCollection()
    {
        var services = new ServiceCollection();

        var result = services.AddViewModels();

        Assert.Same(services, result);
    }

    [Fact]
    public void FullRegistration_CanResolveMainWindowViewModel()
    {
        var services = new ServiceCollection();
        services.AddServices();
        services.AddViewModels();
        var provider = services.BuildServiceProvider();

        var viewModel = provider.GetService<MainWindowViewModel>();

        Assert.NotNull(viewModel);
    }

    [Fact]
    public void FullRegistration_NavigationServiceIsSingleton()
    {
        var services = new ServiceCollection();
        services.AddServices();
        services.AddViewModels();
        var provider = services.BuildServiceProvider();

        var first = provider.GetService<INavigationService>();
        var second = provider.GetService<INavigationService>();

        Assert.Same(first, second);
    }

    [Fact]
    public void FullRegistration_MainWindowViewModelIsTransient()
    {
        var services = new ServiceCollection();
        services.AddServices();
        services.AddViewModels();
        var provider = services.BuildServiceProvider();

        var first = provider.GetService<MainWindowViewModel>();
        var second = provider.GetService<MainWindowViewModel>();

        Assert.NotSame(first, second);
    }
}
