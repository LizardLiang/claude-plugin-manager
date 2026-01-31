# Task: Implement PluginService

## What To Do
Create a service to handle plugin installation and uninstallation to the global scope.

## How To Do
1. Create `IPluginService` interface:
   ```csharp
   public interface IPluginService
   {
       Task<InstallResult> InstallGlobalAsync(Plugin plugin);
       Task<UninstallResult> UninstallGlobalAsync(string pluginId);
       Task<IEnumerable<InstalledPlugin>> GetInstalledGlobalAsync();
       bool IsInstalled(string pluginId);
   }

   public class InstalledPlugin
   {
       public string Id { get; set; }
       public string Name { get; set; }
       public string Version { get; set; }
       public PluginType Type { get; set; }
       public DateTime InstalledAt { get; set; }
   }
   ```
2. Implement installation logic by plugin type:
   - **MCP Servers**: Add entry to `mcpServers` in settings.json
   - **Hooks**: Add to hooks array
   - **Commands/Agents/Skills**: Add appropriate configuration
3. Implement `PluginService`:
   - Use ConfigService to read/write settings
   - Validate plugin before installing
   - Generate correct configuration for each type
4. Track installed plugins:
   - Store installation metadata in SQLite
   - Include install timestamp and source marketplace
5. Implement uninstall:
   - Remove configuration from settings.json
   - Remove from local tracking database
6. Add transaction-like behavior:
   - Backup settings before modification
   - Rollback on failure

## Acceptance Criteria
- [ ] `IPluginService` interface defined
- [ ] Can install MCP Server plugins to global settings
- [ ] Can install other plugin types (Hooks, Commands, etc.)
- [ ] Can uninstall plugins from global settings
- [ ] `GetInstalledGlobalAsync()` returns list of installed plugins
- [ ] `IsInstalled()` correctly identifies installed plugins
- [ ] Installation metadata tracked in local database
- [ ] Settings backed up before modification
- [ ] Rollback occurs on installation failure
