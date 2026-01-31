using System;
using CommunityToolkit.Mvvm.ComponentModel;
using ClaudePluginManager.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ClaudePluginManager.Services;

public partial class NavigationService : ObservableObject, INavigationService
{
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty]
    private ViewModelBase? _currentViewModel;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase
    {
        CurrentViewModel = _serviceProvider.GetRequiredService<TViewModel>();
    }
}
