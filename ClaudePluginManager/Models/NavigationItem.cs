using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ClaudePluginManager.Models;

public partial class NavigationItem : ObservableObject
{
    public string Label { get; }
    public string Icon { get; }
    public Type ViewModelType { get; }

    [ObservableProperty]
    private bool _isSelected;

    public NavigationItem(string label, string icon, Type viewModelType)
    {
        Label = label;
        Icon = icon;
        ViewModelType = viewModelType;
    }
}
