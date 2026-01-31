using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using ClaudePluginManager.Models;
using ClaudePluginManager.Services;
using CommunityToolkit.Mvvm.Input;

namespace ClaudePluginManager.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private NavigationItem? _selectedNavigationItem;

    public MainWindowViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;

        NavigationItems = new ObservableCollection<NavigationItem>
        {
            new("Marketplace", "M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10", typeof(MarketplaceViewModel)),
            new("Installed", "M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z", typeof(InstalledViewModel)),
            new("Settings", "M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z M15 12a3 3 0 11-6 0 3 3 0 016 0z", typeof(SettingsViewModel))
        };

        NavigateCommand = new RelayCommand<NavigationItem>(Navigate);

        if (_navigationService is INotifyPropertyChanged notifyPropertyChanged)
        {
            notifyPropertyChanged.PropertyChanged += OnNavigationServicePropertyChanged;
        }

        // Navigate to first item on startup
        if (NavigationItems.Count > 0)
        {
            Navigate(NavigationItems[0]);
        }
    }

    public ObservableCollection<NavigationItem> NavigationItems { get; }

    public NavigationItem? SelectedNavigationItem
    {
        get => _selectedNavigationItem;
        private set => SetProperty(ref _selectedNavigationItem, value);
    }

    public ICommand NavigateCommand { get; }

    public ViewModelBase? CurrentViewModel => _navigationService.CurrentViewModel;

    private void Navigate(NavigationItem? item)
    {
        if (item == null) return;

        // Update selection state
        foreach (var navItem in NavigationItems)
        {
            navItem.IsSelected = navItem == item;
        }

        SelectedNavigationItem = item;

        // Navigate using reflection to call the generic method
        var navigateMethod = typeof(INavigationService)
            .GetMethod(nameof(INavigationService.NavigateTo))!
            .MakeGenericMethod(item.ViewModelType);

        navigateMethod.Invoke(_navigationService, null);
    }

    private void OnNavigationServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(INavigationService.CurrentViewModel))
        {
            OnPropertyChanged(nameof(CurrentViewModel));
        }
    }
}
