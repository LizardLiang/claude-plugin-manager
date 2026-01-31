using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using ClaudePluginManager.Data;
using ClaudePluginManager.Services;
using ClaudePluginManager.ViewModels;
using ClaudePluginManager.Views;
using Microsoft.Extensions.DependencyInjection;

namespace ClaudePluginManager;

public partial class App : Application
{
    public static IServiceProvider? Services { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            DisableAvaloniaDataAnnotationValidation();

            var services = new ServiceCollection();
            services.AddServices();
            services.AddViewModels();

            Services = services.BuildServiceProvider();

            await InitializeDatabaseAsync();

            desktop.MainWindow = new MainWindow
            {
                DataContext = Services.GetRequiredService<MainWindowViewModel>(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static async Task InitializeDatabaseAsync()
    {
        if (Services is null)
        {
            return;
        }

        var databaseService = Services.GetRequiredService<IDatabaseService>();
        await databaseService.InitializeAsync();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}
