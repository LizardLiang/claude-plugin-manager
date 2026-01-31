# Phase 1: MVP (v0.1)

## Goal
Deliver a functional Windows application that allows users to browse, search, and install plugins from the default marketplace to global scope.

## Features

### 1.1 Project Setup
- [ ] Initialize Avalonia UI project with .NET 8+
- [ ] Set up MVVM architecture (ViewModels, Views, Services)
- [ ] Configure SQLite for local caching
- [ ] Create basic application shell with navigation

### 1.2 Marketplace Browsing (FR-1.1, FR-4.7)
- [ ] Implement GitHubClient service for API calls
- [ ] Fetch plugin index from `claude-market/marketplace` repository
- [ ] Parse `.claude-plugin/marketplace.json` format
- [ ] Display plugin list in UI
- [ ] Implement MarketplaceService

### 1.3 Plugin Search & Filter (FR-1.2, FR-1.4)
- [ ] Search plugins by name, description, tags
- [ ] Filter plugins by type (MCP Server, Hooks, Commands, Agents, Skills)
- [ ] Real-time search filtering in UI

### 1.4 Plugin Details (FR-1.3)
- [ ] View plugin details panel (name, description, version, author)
- [ ] Fetch and display plugin README from GitHub
- [ ] Show plugin type and tags

### 1.5 Global Installation (FR-1.5, FR-2.1, FR-2.2)
- [ ] Implement ConfigService for reading/writing `~/.claude/settings.json`
- [ ] Install plugin to global scope
- [ ] Uninstall plugin from global scope
- [ ] List all installed global plugins
- [ ] Implement PluginService

### 1.6 Version Detection (FR-3.1)
- [ ] Detect installed plugin versions from config
- [ ] Display version in installed plugins list
- [ ] Compare installed vs available versions

### 1.7 Marketplace Refresh (FR-4.6)
- [ ] Manual refresh button to fetch latest marketplace index
- [ ] Show last synced timestamp
- [ ] Handle network errors gracefully

## Technical Requirements

### Data Models to Implement
```csharp
Plugin { Id, Name, Description, Version, Author, Repository, Type, Tags, Components }
Marketplace { Id, Name, GitHubOwner, GitHubRepo, LastSynced }
```

### Services to Implement
- `GitHubClient` - GitHub API interactions
- `MarketplaceService` - Marketplace data management
- `PluginService` - Plugin install/uninstall operations
- `ConfigService` - Claude Code settings.json read/write

### UI Views
- MainWindow with navigation tabs
- MarketplaceView - Browse/search plugins
- InstalledView - List installed plugins
- PluginDetailView - Show plugin details

## Acceptance Criteria
- Application launches in < 3 seconds
- Can browse plugins from claude-market/marketplace
- Can search and filter plugins
- Can install a plugin globally in < 10 seconds
- Can uninstall a plugin globally
- Installed plugins persist after app restart
