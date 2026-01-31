using Avalonia.Controls;
using ClaudePluginManager.ViewModels;

namespace ClaudePluginManager.Views;

public partial class MarketplaceView : UserControl
{
    public MarketplaceView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is MarketplaceViewModel viewModel)
        {
            viewModel.LoadPluginsCommand.Execute(null);
        }
    }
}
