# Product Requirements Document: Claude Code Plugin Manager

## Document Info
- **Version**: 1.0
- **Status**: Ready for Development
- **Last Updated**: 2026-01-31

---

## 1. Executive Summary

Claude Code Plugin Manager is a desktop application that provides users with a centralized, intuitive interface to discover, install, update, and organize Claude Code plugins. The application supports both global and per-project plugin management, multiple GitHub-based marketplace sources, and group-based batch operations.

---

## 2. Problem Statement

Currently, managing Claude Code plugins requires manual configuration and lacks:
- A unified interface for plugin discovery and installation
- Easy switching between global and project-specific configurations
- Batch operations for installing/removing multiple plugins
- Automatic version tracking and updates
- Organization tools for managing large plugin collections

---

## 3. Goals & Success Metrics

### Goals
1. Reduce time to install/configure plugins by 80%
2. Provide a single source of truth for plugin management
3. Enable reproducible plugin setups across projects via groups

### Success Metrics
| Metric | Target |
|--------|--------|
| Time to install a plugin | < 10 seconds |
| Time to setup a new project with a group | < 30 seconds |
| Plugin update detection accuracy | 100% |
| User satisfaction (post-launch survey) | > 4.5/5 |

---

## 4. User Personas

### Persona 1: Solo Developer (Primary)
- Uses Claude Code daily for personal projects
- Wants quick plugin setup without manual config editing
- Manages 5-15 plugins across 3-5 active projects

### Persona 2: Team Lead (Secondary)
- Needs consistent plugin configurations across team projects
- Wants to export/share plugin groups with team members
- Manages standardized setups for different project types

---

## 5. Functional Requirements

### 5.1 Plugin Discovery & Installation

**Supported Plugin Types:**
- MCP Servers (external servers providing tools, resources, prompts)
- Hooks (shell commands on events)
- Slash Commands (custom `/` commands)
- Agents (AI agent definitions)
- Skills (reusable capabilities)

| ID | Requirement | Priority |
|----|-------------|----------|
| FR-1.1 | Browse plugins from configured marketplaces | P0 |
| FR-1.2 | Search plugins by name, description, tags | P0 |
| FR-1.3 | View plugin details (description, version, author, readme) | P0 |
| FR-1.4 | Filter plugins by type (MCP, hooks, commands, agents, skills) | P0 |
| FR-1.5 | Install plugin globally (applies to all projects) | P0 |
| FR-1.6 | Install plugin to specific project | P0 |
| FR-1.7 | Select multiple plugins and install in batch | P0 |
| FR-1.8 | Show installation progress and status | P1 |
| FR-1.9 | Warn about plugin dependencies (user decides to install) | P1 |

### 5.2 Plugin Management

| ID | Requirement | Priority |
|----|-------------|----------|
| FR-2.1 | List all installed plugins (global and per-project) | P0 |
| FR-2.2 | Uninstall plugin from global scope | P0 |
| FR-2.3 | Uninstall plugin from specific project | P0 |
| FR-2.4 | Enable/disable plugin without uninstalling | P1 |
| FR-2.5 | View plugin configuration and settings | P1 |
| FR-2.6 | Edit plugin configuration | P2 |
| FR-2.7 | Project-level config overrides global (automatic) | P0 |

### 5.3 Version Management & Updates

| ID | Requirement | Priority |
|----|-------------|----------|
| FR-3.1 | Detect installed plugin versions | P0 |
| FR-3.2 | Check for available updates from marketplace | P0 |
| FR-3.3 | Update single plugin to latest version | P0 |
| FR-3.4 | Update all outdated plugins in batch | P0 |
| FR-3.5 | Show changelog/release notes before update | P1 |
| FR-3.6 | Rollback to previous version | P2 |
| FR-3.7 | Configure auto-update behavior (auto/notify/manual) | P1 |

### 5.4 Marketplace Management

**Marketplace Structure:**
- Marketplaces are GitHub repositories
- Plugin index at `.claude-plugin/marketplace.json`
- Each plugin has `.claude-plugin/plugin.json` manifest
- Default marketplace: `claude-market/marketplace`

| ID | Requirement | Priority |
|----|-------------|----------|
| FR-4.1 | Add marketplace by GitHub repo URL (owner/repo format) | P0 |
| FR-4.2 | Remove marketplace from list | P0 |
| FR-4.3 | Support private GitHub repos with token authentication | P0 |
| FR-4.4 | Enable/disable marketplace without removing | P1 |
| FR-4.5 | Set marketplace priority order | P2 |
| FR-4.6 | Refresh marketplace plugin index (fetch from GitHub) | P0 |
| FR-4.7 | Ship with `claude-market/marketplace` pre-configured | P0 |
| FR-4.8 | Cache marketplace data locally | P1 |

### 5.5 Plugin Groups

| ID | Requirement | Priority |
|----|-------------|----------|
| FR-5.1 | Create named plugin group | P0 |
| FR-5.2 | Add plugins to group (with optional version pinning) | P0 |
| FR-5.3 | Remove plugins from group | P0 |
| FR-5.4 | Delete group | P0 |
| FR-5.5 | Install all plugins in group to target (global/project) | P0 |
| FR-5.6 | Uninstall all plugins in group from target | P0 |
| FR-5.7 | Export group as JSON file | P0 |
| FR-5.8 | Import group from JSON file | P0 |
| FR-5.9 | Duplicate/clone group | P2 |

### 5.6 Project Management

| ID | Requirement | Priority |
|----|-------------|----------|
| FR-6.1 | Detect Claude Code projects on system (folders with `.claude/`) | P1 |
| FR-6.2 | Manually add project path | P0 |
| FR-6.3 | Remove project from manager | P0 |
| FR-6.4 | View project's installed plugins | P0 |
| FR-6.5 | Quick-switch between projects | P1 |

---

## 6. Non-Functional Requirements

### 6.1 Technical Stack
- **Framework**: Avalonia UI (.NET)
- **Language**: C#
- **Minimum .NET Version**: .NET 8+
- **Target Platforms**:
  - **Primary**: Windows (v1 focus)
  - **Secondary**: macOS, Linux (post-v1)

### 6.2 Configuration Integration

**Claude Code Config Locations:**
- Global: `~/.claude/settings.json`
- Project: `<project>/.claude/settings.json`
- Local: `<project>/.claude/settings.local.json`

**Integration Method:** Direct JSON file read/write (no CLI dependency)

**Config Precedence:** Project > Global (project overrides global)

### 6.3 Performance
| Requirement | Target |
|-------------|--------|
| Application startup time | < 3 seconds |
| Plugin list load time | < 2 seconds |
| Installation time (single plugin) | < 10 seconds |
| Memory usage (idle) | < 200 MB |

### 6.4 Security
- Validate GitHub repository URLs
- Verify plugin integrity via Git commit hashes
- Secure storage for GitHub tokens (OS credential manager)
- No storage of tokens in plain text config files
- HTTPS-only for all GitHub API calls

### 6.5 Reliability
- Graceful handling of network failures
- Transaction-based installations (rollback on failure)
- Local cache for offline marketplace browsing
- Retry logic for GitHub API rate limits

---

## 7. User Interface Wireframes (Conceptual)

```
+------------------------------------------------------------------+
|  Claude Code Plugin Manager                            [_][O][X] |
+------------------------------------------------------------------+
| [Marketplace] [Installed] [Groups] [Projects] [Settings]         |
+------------------------------------------------------------------+
|  +------------------+  +--------------------------------------+  |
|  | MARKETPLACES     |  | PLUGINS                    [Search] |  |
|  | > claude-market  |  | +----------------------------------+ |  |
|  |   my-org/plugins |  | | [x] mcp-filesystem    v1.2.3    | |  |
|  |   + Add New      |  | |     MCP Server - File access     | |  |
|  |                  |  | +----------------------------------+ |  |
|  | FILTER BY TYPE   |  | | [ ] git-hooks         v2.0.0    | |  |
|  |   All            |  | |     Hooks - Git automation       | |  |
|  |   MCP Servers    |  | +----------------------------------+ |  |
|  |   Hooks          |  | | [ ] code-review       v0.5.1    | |  |
|  |   Commands       |  | |     Skill - PR review helper     | |  |
|  |   Agents         |  | +----------------------------------+ |  |
|  |   Skills         |  |                                      |  |
|  +------------------+  +--------------------------------------+  |
|                                                                  |
|  [Install Selected (1)]  [Install to: Global v]                  |
+------------------------------------------------------------------+
```

---

## 8. Data Models

### 8.1 Plugin
```csharp
Plugin {
  Id: string              // Unique identifier (marketplace/plugin-name)
  Name: string            // Display name
  Description: string     // Short description
  Version: string         // Semver (e.g., "1.2.3")
  Author: string          // Author name
  Repository: string      // GitHub URL
  MarketplaceSource: string // Which marketplace this came from
  Type: PluginType        // MCP_SERVER | HOOK | COMMAND | AGENT | SKILL
  Tags: string[]          // Searchable tags
  Dependencies: string[]  // Plugin IDs this depends on (warn only)
  ConfigSchema: JSON?     // Optional configuration schema
  Components: {           // What the plugin provides
    McpServers: string[]
    Hooks: string[]
    Commands: string[]
    Agents: string[]
    Skills: string[]
  }
}
```

### 8.2 Group
```csharp
Group {
  Id: string
  Name: string
  Description: string?
  Plugins: PluginReference[]
  CreatedAt: DateTime
  UpdatedAt: DateTime
}

PluginReference {
  PluginId: string
  VersionConstraint: string  // "1.2.3" (pinned), "^1.0.0", or "latest"
}
```

### 8.3 Marketplace
```csharp
Marketplace {
  Id: string              // Generated from owner/repo
  Name: string            // Display name
  GitHubOwner: string     // e.g., "claude-market"
  GitHubRepo: string      // e.g., "marketplace"
  Enabled: bool
  Priority: int           // Lower = higher priority
  RequiresAuth: bool      // Is this a private repo?
  LastSynced: DateTime?
}
```

### 8.4 App Configuration
```csharp
AppConfig {
  Marketplaces: Marketplace[]
  Groups: Group[]
  Projects: ProjectReference[]
  GitHubTokens: Dictionary<string, SecureString>  // owner -> token
  UpdateBehavior: AUTO | NOTIFY | MANUAL
  Theme: LIGHT | DARK | SYSTEM
}
```

---

## 9. Clarifications (Resolved)

| # | Question | Resolution |
|---|----------|------------|
| Q1 | Plugin types | All: MCP Servers, Hooks, Commands, Agents, Skills |
| Q2 | Marketplace format | GitHub repos with `.claude-plugin/marketplace.json` |
| Q3 | Default marketplace | `claude-market/marketplace` pre-configured |
| Q4 | Config storage | Direct JSON file modification |
| Q5 | Version conflicts | Project overrides global automatically |
| Q6 | Group version pinning | Optional (user choice: pinned or latest) |
| Q7 | Private marketplaces | Supported in v1 with GitHub token auth |
| Q8 | Platform priority | Windows first, macOS/Linux follow |
| Q9 | Group sharing | Export/import JSON files in v1 |
| Q10 | Dependencies | Warn only, user decides |
| Q11 | CLI companion | Planned for v2 |
| Q12 | Offline mode | Local caching for browsing |

---

## 10. Assumptions

1. Claude Code plugin system uses the documented `.claude-plugin/` structure
2. Users have GitHub access for marketplace repos
3. Users have write access to `~/.claude/` and project `.claude/` directories
4. Internet connectivity available (offline caching as enhancement)

---

## 11. Out of Scope (v1)

- Plugin development/authoring tools
- Marketplace hosting service
- Cloud-synced team configurations
- Usage analytics/telemetry
- Plugin monetization
- macOS/Linux support (v1 is Windows-focused)
- CLI companion tool

---

## 12. Release Phases

### Phase 1: MVP (v0.1)
- Single marketplace support (claude-market/marketplace)
- Browse and search plugins
- Install/uninstall plugins (global only)
- Version detection
- Basic UI (Windows)

### Phase 2: Core Features (v0.5)
- Per-project installation
- Multiple marketplaces (public)
- Plugin groups (create, install, remove)
- Batch operations
- Private marketplace support (GitHub auth)

### Phase 3: Full Release (v1.0)
- Group export/import
- Auto-update detection and notifications
- Plugin configuration editor
- Performance optimizations
- Polished UI/UX

### Future (v2.0+)
- macOS and Linux support
- CLI companion tool (`ccpm`)
- Plugin dependency auto-resolution (optional)
- Marketplace analytics

---

## 13. Technical Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                    Avalonia UI (MVVM)                       │
├─────────────────────────────────────────────────────────────┤
│  ViewModels: Marketplace | Installed | Groups | Settings    │
├─────────────────────────────────────────────────────────────┤
│                     Services Layer                          │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐           │
│  │ Marketplace │ │   Plugin    │ │   Config    │           │
│  │   Service   │ │   Service   │ │   Service   │           │
│  └─────────────┘ └─────────────┘ └─────────────┘           │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐           │
│  │   GitHub    │ │    Group    │ │  Credential │           │
│  │    Client   │ │   Service   │ │   Manager   │           │
│  └─────────────┘ └─────────────┘ └─────────────┘           │
├─────────────────────────────────────────────────────────────┤
│                    Data Layer                               │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐           │
│  │   SQLite    │ │ JSON Files  │ │ OS Keychain │           │
│  │   (cache)   │ │  (config)   │ │  (tokens)   │           │
│  └─────────────┘ └─────────────┘ └─────────────┘           │
└─────────────────────────────────────────────────────────────┘
```

---

## 14. Appendix

### A. Claude Code Config Format Reference

**Global settings** (`~/.claude/settings.json`):
```json
{
  "mcpServers": {
    "server-name": {
      "type": "stdio",
      "command": "npx",
      "args": ["-y", "@org/mcp-server"]
    }
  },
  "hooks": {
    "pre-tool-execution": ["./hooks/validate.sh"]
  }
}
```

**Project settings** (`.claude/settings.json`):
```json
{
  "mcpServers": {
    "project-specific-server": {
      "command": "node",
      "args": ["./local-server.js"]
    }
  }
}
```

### B. Marketplace Index Format

**`.claude-plugin/marketplace.json`**:
```json
{
  "version": "1.0",
  "plugins": [
    {
      "id": "mcp-filesystem",
      "name": "Filesystem MCP Server",
      "version": "1.2.3",
      "type": "mcp_server",
      "path": "plugins/mcp-filesystem"
    }
  ]
}
```

### C. Group Export Format

```json
{
  "name": "Frontend Development",
  "description": "Plugins for frontend projects",
  "exportedAt": "2026-01-31T12:00:00Z",
  "plugins": [
    { "id": "mcp-filesystem", "version": "latest" },
    { "id": "prettier-hooks", "version": "2.0.0" }
  ]
}
```

### D. Glossary
- **Global Installation**: Plugin available to all Claude Code projects
- **Project Installation**: Plugin available only to specific project
- **Marketplace**: GitHub repository containing plugin registry
- **Group**: User-defined collection of plugins for batch operations
- **MCP Server**: Model Context Protocol server providing tools to Claude

---

## 15. References

- [Claude Market Marketplace](https://github.com/claude-market/marketplace)
- [Claude Code Settings Documentation](https://code.claude.com/docs/en/settings)
- [MCP Protocol Specification](https://modelcontextprotocol.io)

---

*Document finalized. Ready for technical design and development.*
