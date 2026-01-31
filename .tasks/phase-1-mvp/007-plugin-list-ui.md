# Task: Create Plugin List UI Component

## What To Do
Build the UI component that displays a list of available plugins from the marketplace with essential information.

## How To Do
1. Create `Plugin` model (if not already complete):
   ```csharp
   public class Plugin
   {
       public string Id { get; set; }
       public string Name { get; set; }
       public string Description { get; set; }
       public string Version { get; set; }
       public string Author { get; set; }
       public string Repository { get; set; }
       public PluginType Type { get; set; }
       public List<string> Tags { get; set; }
   }

   public enum PluginType
   {
       McpServer, Hooks, Commands, Agents, Skills
   }
   ```
2. Create `PluginListItemViewModel`:
   - Wraps Plugin model
   - Exposes display properties
   - Commands for actions (view details, install)
3. Update `MarketplaceViewModel`:
   - `ObservableCollection<PluginListItemViewModel> Plugins`
   - Load plugins on initialization
   - Handle loading states
4. Create `MarketplaceView.axaml`:
   - ListBox/ItemsControl for plugin list
   - Plugin item template showing: name, description, author, type badge, version
5. Add loading indicator while fetching
6. Add empty state when no plugins found

## Acceptance Criteria
- [x] `Plugin` model with all required properties
- [x] `PluginListItemViewModel` created
- [x] `MarketplaceView` displays list of plugins
- [x] Each plugin item shows name, description, author, type, and version
- [x] Loading indicator displays while fetching plugins
- [x] Empty state message when no plugins available
- [x] List is scrollable for many plugins
