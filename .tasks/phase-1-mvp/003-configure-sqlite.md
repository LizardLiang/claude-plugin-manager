# Task: Configure SQLite for Local Caching

## What To Do
Set up SQLite database for local caching of marketplace data, plugins, and application state.

## How To Do
1. Add SQLite NuGet packages:
   - `Microsoft.Data.Sqlite`
   - `Dapper` (for lightweight ORM)
2. Create database context/connection manager:
   ```csharp
   public interface IDatabaseService
   {
       IDbConnection CreateConnection();
       Task InitializeAsync();
   }
   ```
3. Define database schema for tables:
   - `Plugins` - cached plugin metadata
   - `Marketplaces` - marketplace sources
   - `SyncHistory` - last sync timestamps
4. Create database initialization script:
   - Create tables if not exist
   - Handle schema migrations
5. Implement database path configuration:
   - Store database in user's app data folder
   - Path: `~/.claude-plugin-manager/cache.db`
6. Register database service in DI container

## Acceptance Criteria
- [ ] SQLite packages installed
- [ ] `IDatabaseService` interface and implementation created
- [ ] Database file created in appropriate location (`~/.claude-plugin-manager/cache.db`)
- [ ] Schema includes Plugins, Marketplaces, and SyncHistory tables
- [ ] Database initializes on first run without errors
- [ ] Connection pooling/management implemented
