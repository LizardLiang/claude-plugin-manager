# Task: Implement ConfigService

## What To Do
Create a service for reading and writing Claude Code's `settings.json` configuration file to manage plugin installations.

## How To Do
1. Define settings models:
   ```csharp
   public class ClaudeSettings
   {
       public Dictionary<string, McpServerConfig> McpServers { get; set; }
       public List<string> Hooks { get; set; }
       // Other settings as needed
   }

   public class McpServerConfig
   {
       public string Command { get; set; }
       public List<string> Args { get; set; }
       public Dictionary<string, string> Env { get; set; }
   }
   ```
2. Create `IConfigService` interface:
   ```csharp
   public interface IConfigService
   {
       Task<ClaudeSettings> ReadGlobalSettingsAsync();
       Task WriteGlobalSettingsAsync(ClaudeSettings settings);
       Task<ClaudeSettings> ReadProjectSettingsAsync(string projectPath);
       Task WriteProjectSettingsAsync(string projectPath, ClaudeSettings settings);
       string GetGlobalSettingsPath();
   }
   ```
3. Implement `ConfigService`:
   - Global path: `~/.claude/settings.json`
   - Read/parse JSON with System.Text.Json
   - Write with proper formatting (indented)
   - Create file/directory if not exists
4. Handle file locking and concurrent access
5. Backup existing settings before write
6. Validate JSON structure before writing

## Acceptance Criteria
- [x] `ClaudeSettings` and related models defined
- [x] `IConfigService` interface created
- [x] Can read global settings from `~/.claude/settings.json`
- [x] Can write settings back to file with proper formatting
- [x] Creates settings file if it doesn't exist
- [x] Creates `.claude` directory if it doesn't exist
- [x] Handles malformed JSON gracefully
- [x] Preserves unknown properties in settings (doesn't delete them)
