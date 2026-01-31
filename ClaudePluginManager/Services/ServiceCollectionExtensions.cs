using ClaudePluginManager.Data;
using ClaudePluginManager.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ClaudePluginManager.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IDatabaseService, DatabaseService>();
        services.AddSingleton<IProcessRunner, ProcessRunner>();
        services.AddSingleton<IGitHubClient, GitHubClient>();
        services.AddSingleton<IMarketplaceService, MarketplaceService>();
        services.AddSingleton<IConfigService, ConfigService>();
        services.AddSingleton<IPluginService, PluginService>();
        services.AddSingleton<IDialogService, DialogService>();
        return services;
    }

    public static IServiceCollection AddViewModels(this IServiceCollection services)
    {
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<MarketplaceViewModel>();
        services.AddTransient<InstalledViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<PluginDetailsViewModel>();
        services.AddSingleton<Func<PluginDetailsViewModel>>(sp => () => sp.GetRequiredService<PluginDetailsViewModel>());
        return services;
    }
}
