using ClaudePluginManager.ViewModels;

namespace ClaudePluginManager.Services;

public interface INavigationService
{
    ViewModelBase? CurrentViewModel { get; }
    void NavigateTo<TViewModel>() where TViewModel : ViewModelBase;
}
