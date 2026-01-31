# Task: Implement Plugin Search and Filter

## What To Do
Add search and filter functionality to the marketplace view, allowing users to find plugins by name, description, tags, and filter by plugin type.

## How To Do
1. Add search UI to MarketplaceView:
   - TextBox for search input
   - Type filter dropdown/checkboxes
2. Update `MarketplaceViewModel`:
   ```csharp
   public string SearchQuery { get; set; }
   public PluginType? SelectedTypeFilter { get; set; }
   public ObservableCollection<PluginListItemViewModel> FilteredPlugins { get; }
   ```
3. Implement filtering logic:
   - Search matches against: Name, Description, Tags, Author
   - Case-insensitive matching
   - Type filter narrows results to selected type
   - Combine search and type filters
4. Make search reactive:
   - Filter updates as user types (with debounce)
   - Use delay of ~300ms before applying filter
5. Add clear search button
6. Show result count ("Showing X of Y plugins")

## Acceptance Criteria
- [x] Search textbox present in MarketplaceView
- [x] Type filter dropdown/selector present
- [x] Typing in search filters plugins in real-time
- [x] Search matches name, description, tags, and author
- [x] Type filter limits results to selected plugin type
- [x] Search and type filter work together (AND logic)
- [x] Clear button resets search
- [x] Result count displayed
- [x] Search is debounced (not filtering on every keystroke)
