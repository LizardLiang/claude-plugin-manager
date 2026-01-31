# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Claude Code Plugin Manager - a desktop application for discovering, installing, updating, and organizing Claude Code plugins. Provides a centralized UI for managing plugins across global and per-project scopes.

## Technology Stack

- **Framework**: Avalonia UI with MVVM pattern
- **Language**: C# on .NET 8+
- **Target Platform**: Windows (primary), macOS/Linux planned for v2
- **Data Storage**: SQLite (cache), JSON files (config), OS keychain (tokens)

## Build Commands

```bash
dotnet build
dotnet run
dotnet test
```

## Development Workflow

- **Always use Context7 to check API specifications** before implementing or modifying code that interacts with external APIs (GitHub API, MCP protocol, etc.)

## Architecture

### Layer Structure

```
UI Layer (Avalonia MVVM)
    ↓
ViewModels: Marketplace | Installed | Groups | Settings
    ↓
Services Layer: MarketplaceService, PluginService, ConfigService, GitHubClient, GroupService, CredentialManager
    ↓
Data Layer: SQLite cache | JSON config files | OS Keychain
```

### Plugin Types

Five supported plugin types: MCP Servers, Hooks, Slash Commands, Agents, Skills.

### Configuration Paths

- Global: `~/.claude/settings.json`
- Project: `<project>/.claude/settings.json`
- Local: `<project>/.claude/settings.local.json`
- Precedence: Project overrides Global

### Marketplace Structure

Marketplaces are GitHub repositories with:
- Index file: `.claude-plugin/marketplace.json`
- Plugin manifests: `.claude-plugin/plugin.json`
- Default marketplace: `claude-market/marketplace`

### Data Models

Key entities: Plugin, Group (plugin collections), Marketplace (GitHub repo source), AppConfig.

Plugin IDs use format: `marketplace/plugin-name`

## Security Requirements

- GitHub tokens stored in OS credential manager (never plain text)
- HTTPS-only for GitHub API calls
- Validate all GitHub repository URLs
- Verify plugin integrity via Git commit hashes
- Transaction-based installations with rollback on failure

## Task Management

Tasks are organized in `.tasks/[phase-name]/` directories.

### Directory Structure

```
.tasks/
├── phase-1-mvp/
│   ├── index.md          # Progress tracking for this phase
│   ├── 001-task-name.md
│   ├── 002-task-name.md
│   └── ...
├── phase-2-core-features/
│   ├── index.md
│   └── ...
└── ...
```

### Task File Format

Each task file must contain three sections:

```markdown
# Task: [Task Name]

## What To Do
[Clear description of the task objective]

## How To Do
[Step-by-step implementation approach]

## Acceptance Criteria
- [ ] Criterion 1
- [ ] Criterion 2
- [ ] ...
```

### Index File Format

Each phase directory must have an `index.md` to track progress:

```markdown
# Phase [N]: [Phase Name] - Progress

## Status: [Not Started | In Progress | Completed]

## Tasks

| ID | Task | Status |
|----|------|--------|
| 001 | Task name | [ ] Not Started |
| 002 | Task name | [~] In Progress |
| 003 | Task name | [x] Completed |

## Summary
- Total: X tasks
- Completed: Y
- In Progress: Z
- Not Started: W
```

### Rules

1. **Task Completion**: A task may ONLY be marked as `[x] Completed` when ALL acceptance criteria are fulfilled
2. **Progress Tracking**: Update `index.md` whenever a task status changes
3. **Sequential Work**: Complete tasks in order unless dependencies allow parallel work
4. **No Partial Completion**: If any acceptance criterion is not met, the task remains `In Progress` or `Not Started`
