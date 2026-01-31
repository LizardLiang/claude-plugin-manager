# Task: Implement MarketplaceService

## What To Do
Create a service to manage marketplace data - fetching the plugin index, parsing the marketplace.json format, and caching results.

## How To Do
1. Define marketplace data models:
   ```csharp
   public class Marketplace
   {
       public string Id { get; set; }
       public string Name { get; set; }
       public string GitHubOwner { get; set; }
       public string GitHubRepo { get; set; }
       public DateTime LastSynced { get; set; }
   }

   public class MarketplaceIndex
   {
       public string Name { get; set; }
       public string Version { get; set; }
       public List<PluginEntry> Plugins { get; set; }
   }
   ```
2. Create `IMarketplaceService` interface:
   ```csharp
   public interface IMarketplaceService
   {
       Task<IEnumerable<Plugin>> GetPluginsAsync();
       Task<IEnumerable<Plugin>> SearchPluginsAsync(string query);
       Task RefreshAsync();
       DateTime? GetLastSyncTime();
   }
   ```
3. Implement `MarketplaceService`:
   - Fetch `.claude-plugin/marketplace.json` from default marketplace
   - Parse JSON into plugin list
   - Cache results in SQLite
   - Return cached data when available
4. Configure default marketplace:
   - Owner: `claude-market`
   - Repo: `marketplace`
   - Path: `.claude-plugin/marketplace.json`
5. Handle missing/malformed marketplace.json gracefully

## Acceptance Criteria
- [ ] `Marketplace` and `MarketplaceIndex` models defined
- [ ] `IMarketplaceService` interface created
- [ ] Can fetch and parse marketplace.json from default marketplace
- [ ] Plugins are cached in SQLite after fetch
- [ ] Returns cached data when available
- [ ] `RefreshAsync()` forces fresh fetch from GitHub
- [ ] `GetLastSyncTime()` returns timestamp of last successful sync
- [ ] Graceful handling of network/parsing errors
