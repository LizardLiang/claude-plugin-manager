# Phase 2: Core Features (v0.5)

## Goal
Add per-project plugin management, multiple marketplace support, plugin groups, and batch operations.

## Prerequisites
- Phase 1 MVP completed

## Features

### 2.1 Per-Project Installation (FR-1.6, FR-2.3, FR-2.7)
- [ ] Detect and list Claude Code projects (folders with `.claude/`)
- [ ] Manually add project paths (FR-6.2)
- [ ] Remove project from manager (FR-6.3)
- [ ] Install plugin to specific project
- [ ] Uninstall plugin from specific project
- [ ] View project's installed plugins (FR-6.4)
- [ ] Handle config precedence (project overrides global)

### 2.2 Project Management UI (FR-6.1, FR-6.5)
- [ ] Projects tab in navigation
- [ ] Project list with quick-switch
- [ ] Project scanner for auto-detection
- [ ] "Install to" dropdown (Global / Project selection)

### 2.3 Multiple Marketplaces (FR-4.1, FR-4.2, FR-4.4)
- [ ] Add marketplace by GitHub repo URL (owner/repo format)
- [ ] Remove marketplace from list
- [ ] Enable/disable marketplace without removing
- [ ] Aggregate plugins from all enabled marketplaces
- [ ] Show marketplace source for each plugin

### 2.4 Private Marketplace Support (FR-4.3)
- [ ] Implement CredentialManager using OS keychain
- [ ] GitHub token input and secure storage
- [ ] Authenticate private repo API calls
- [ ] Token management UI in Settings

### 2.5 Marketplace Caching (FR-4.8)
- [ ] Cache marketplace data in SQLite
- [ ] Offline browsing from cache
- [ ] Cache invalidation strategy
- [ ] Show cached vs live data indicator

### 2.6 Plugin Groups - Basic (FR-5.1, FR-5.2, FR-5.3, FR-5.4)
- [ ] Create named plugin group
- [ ] Add plugins to group
- [ ] Remove plugins from group
- [ ] Delete group
- [ ] Groups tab in navigation
- [ ] Implement GroupService

### 2.7 Group Installation (FR-5.5, FR-5.6)
- [ ] Install all plugins in group to target (global/project)
- [ ] Uninstall all plugins in group from target
- [ ] Progress indicator for batch operations

### 2.8 Batch Operations (FR-1.7)
- [ ] Multi-select plugins in marketplace view
- [ ] Install selected plugins in batch
- [ ] Installation progress and status (FR-1.8)

### 2.9 Dependency Warnings (FR-1.9)
- [ ] Parse plugin dependencies from manifest
- [ ] Warn user about missing dependencies
- [ ] User decides whether to install dependencies

## Technical Requirements

### Data Models to Extend
```csharp
Group { Id, Name, Description, Plugins: PluginReference[], CreatedAt, UpdatedAt }
PluginReference { PluginId, VersionConstraint }
ProjectReference { Path, Name, LastAccessed }
AppConfig { Marketplaces[], Groups[], Projects[], GitHubTokens }
```

### New Services
- `GroupService` - Group CRUD and batch operations
- `CredentialManager` - OS keychain integration
- `ProjectService` - Project detection and management

### UI Updates
- ProjectsView - Manage projects
- GroupsView - Manage plugin groups
- SettingsView - Marketplace and token management
- Multi-select capability in plugin lists

## Acceptance Criteria
- Can add/remove multiple marketplaces
- Can authenticate with private GitHub repos
- Can install plugins to specific projects
- Can create groups and batch install
- Project config correctly overrides global
- Offline browsing works from cache
