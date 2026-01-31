# Task: Implement Version Detection and Comparison

## What To Do
Add functionality to detect installed plugin versions and compare them with available versions to identify updates.

## How To Do
1. Enhance `InstalledPlugin` model:
   ```csharp
   public class InstalledPlugin
   {
       // ... existing properties
       public string InstalledVersion { get; set; }
       public string AvailableVersion { get; set; }
       public bool HasUpdate => !string.IsNullOrEmpty(AvailableVersion)
           && InstalledVersion != AvailableVersion;
   }
   ```
2. Implement version detection:
   - Read version from settings.json if stored
   - Match against marketplace plugin by ID
   - Use semantic versioning comparison when possible
3. Update `PluginService`:
   ```csharp
   Task<IEnumerable<InstalledPlugin>> GetInstalledWithUpdatesAsync();
   bool HasUpdate(string pluginId);
   ```
4. Version comparison logic:
   - Parse version strings (handle semver and non-semver)
   - Compare major.minor.patch
   - Handle pre-release versions
5. Update Installed View:
   - Show "Update Available" badge when newer version exists
   - Display installed vs available version
6. Cross-reference with marketplace data

## Acceptance Criteria
- [x] Installed plugins show their version
- [x] Versions compared against marketplace data
- [x] "Update Available" indicator shown when update exists
- [x] Both installed and available versions displayed
- [x] Version comparison handles semver format
- [x] Version comparison handles non-standard versions gracefully
- [x] Works with cached marketplace data
