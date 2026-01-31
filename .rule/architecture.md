# Architecture

## Layer Structure

```
UI Layer (Avalonia MVVM)
    ↓
ViewModels: Marketplace | Installed | Groups | Settings
    ↓
Services Layer: MarketplaceService, PluginService, ConfigService, GitHubClient, GroupService, CredentialManager
    ↓
Data Layer: SQLite cache | JSON config files | OS Keychain
```

## Plugin Types

Five supported plugin types: MCP Servers, Hooks, Slash Commands, Agents, Skills.

## Configuration Paths

- Global: `~/.claude/settings.json`
- Project: `<project>/.claude/settings.json`
- Local: `<project>/.claude/settings.local.json`
- Precedence: Project overrides Global

## Marketplace Structure

Marketplaces are GitHub repositories with:
- Index file: `.claude-plugin/marketplace.json`
- Plugin manifests: `.claude-plugin/plugin.json`
- Default marketplace: `claude-market/marketplace`

## Data Models

Key entities: Plugin, Group (plugin collections), Marketplace (GitHub repo source), AppConfig.

Plugin IDs use format: `marketplace/plugin-name`
