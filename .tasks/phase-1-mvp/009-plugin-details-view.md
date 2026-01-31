# Task: Create Plugin Details View

## What To Do
Build a detailed view for individual plugins showing full information, README content, and installation options.

## How To Do
1. Create `PluginDetailsViewModel`:
   ```csharp
   public class PluginDetailsViewModel : ViewModelBase
   {
       public Plugin Plugin { get; }
       public string ReadmeContent { get; set; }
       public bool IsLoading { get; set; }
       public bool IsInstalled { get; set; }
       public ICommand InstallCommand { get; }
       public ICommand UninstallCommand { get; }
       public ICommand CloseCommand { get; }
   }
   ```
2. Create `PluginDetailsView.axaml`:
   - Header: Plugin name, version, author
   - Type badge and tags display
   - Repository link (clickable)
   - README content (rendered markdown or plain text)
   - Install/Uninstall button
3. Fetch README from GitHub:
   - Use GitHubClient.GetReadmeAsync()
   - Show loading state while fetching
   - Handle missing README gracefully
4. Integrate with navigation:
   - Clicking plugin in list opens details
   - Back/close button returns to list
5. Consider side panel or modal display

## Acceptance Criteria
- [ ] `PluginDetailsViewModel` created with required properties
- [ ] Plugin details view displays: name, version, author, type, tags
- [ ] Repository URL is displayed and clickable
- [ ] README content is fetched and displayed
- [ ] Loading state shown while fetching README
- [ ] Install button visible (functionality in later task)
- [ ] Close/back navigation works
- [ ] Graceful handling when README is missing
