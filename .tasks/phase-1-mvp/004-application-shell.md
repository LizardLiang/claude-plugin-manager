# Task: Create Application Shell with Navigation

## What To Do
Build the main application shell with a navigation system that allows users to switch between Marketplace, Installed Plugins, and Settings views.

## How To Do
1. Design MainWindow layout:
   - Left sidebar for navigation tabs/buttons
   - Main content area for active view
   - Status bar at bottom (optional for Phase 1)
2. Create navigation tab items:
   - Marketplace (browse/search plugins)
   - Installed (manage installed plugins)
   - Settings (future, placeholder for now)
3. Implement tab-based navigation in MainWindowViewModel:
   ```csharp
   public ObservableCollection<NavigationItem> NavigationItems { get; }
   public ViewModelBase CurrentView { get; set; }
   public ICommand NavigateCommand { get; }
   ```
4. Create placeholder ViewModels for each section:
   - `MarketplaceViewModel`
   - `InstalledViewModel`
   - `SettingsViewModel`
5. Style the navigation using Fluent theme
6. Add application icon and window title

## Acceptance Criteria
- [x] MainWindow displays navigation sidebar and content area
- [x] Navigation tabs for Marketplace, Installed, and Settings visible
- [x] Clicking navigation tabs switches the content area
- [x] Current tab is visually highlighted
- [x] Placeholder ViewModels created for each section
- [x] Application has proper window title ("Claude Code Plugin Manager")
