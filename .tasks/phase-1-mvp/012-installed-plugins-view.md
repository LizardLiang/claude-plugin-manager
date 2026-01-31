# Task: Create Installed Plugins View

## What To Do
Build the UI for viewing and managing installed plugins in the global scope.

## How To Do
1. Update `InstalledViewModel`:
   ```csharp
   public class InstalledViewModel : ViewModelBase
   {
       public ObservableCollection<InstalledPluginViewModel> Plugins { get; }
       public bool IsLoading { get; set; }
       public ICommand RefreshCommand { get; }
   }

   public class InstalledPluginViewModel : ViewModelBase
   {
       public InstalledPlugin Plugin { get; }
       public bool HasUpdate { get; set; }
       public string AvailableVersion { get; set; }
       public ICommand UninstallCommand { get; }
       public ICommand ViewDetailsCommand { get; }
   }
   ```
2. Create `InstalledView.axaml`:
   - List of installed plugins
   - Each item shows: name, version, type, installed date
   - Uninstall button per plugin
   - View details button
3. Load installed plugins on view activation:
   - Use PluginService.GetInstalledGlobalAsync()
   - Show loading state
4. Implement uninstall flow:
   - Confirmation dialog before uninstall
   - Progress indicator during uninstall
   - Remove from list on success
   - Error message on failure
5. Add empty state for no installed plugins

## Acceptance Criteria
- [x] `InstalledViewModel` and `InstalledPluginViewModel` created
- [x] `InstalledView` displays list of installed plugins
- [x] Each plugin shows name, version, type, and install date
- [x] Uninstall button present for each plugin
- [x] Confirmation required before uninstall
- [x] Plugin removed from list after successful uninstall
- [x] Error handling for failed uninstalls
- [x] Empty state when no plugins installed
- [x] Loading indicator while fetching installed plugins
