# Phase 4: Future (v2.0+)

## Goal
Expand platform support, add CLI companion, and enhance plugin ecosystem features.

## Prerequisites
- Phase 3 Full Release (v1.0) completed and stable

## Features

### 4.1 Cross-Platform Support
- [ ] macOS support
  - [ ] Test and fix Avalonia rendering on macOS
  - [ ] macOS keychain integration for credentials
  - [ ] macOS-specific file paths (`~/Library/Application Support/`)
  - [ ] App bundle and notarization
- [ ] Linux support
  - [ ] Test on common distros (Ubuntu, Fedora)
  - [ ] Linux secret service integration (libsecret)
  - [ ] XDG directory compliance
  - [ ] AppImage/Flatpak packaging

### 4.2 CLI Companion Tool (`ccpm`)
- [ ] Create separate CLI project
- [ ] Share core services with GUI app
- [ ] Commands:
  - [ ] `ccpm search <query>` - Search plugins
  - [ ] `ccpm install <plugin>` - Install plugin
  - [ ] `ccpm uninstall <plugin>` - Uninstall plugin
  - [ ] `ccpm list` - List installed plugins
  - [ ] `ccpm update [plugin]` - Update plugins
  - [ ] `ccpm marketplace add <repo>` - Add marketplace
  - [ ] `ccpm group install <group>` - Install group
  - [ ] `ccpm group export <group> <file>` - Export group
- [ ] Global tool installation (`dotnet tool install -g ccpm`)
- [ ] Shell completions (bash, zsh, PowerShell)

### 4.3 Plugin Dependency Auto-Resolution
- [ ] Optional automatic dependency installation
- [ ] Dependency tree visualization
- [ ] Conflict detection and resolution
- [ ] User preference: auto-install | prompt | manual

### 4.4 Marketplace Analytics
- [ ] Download/install counts per plugin
- [ ] Trending plugins section
- [ ] Plugin ratings and reviews (if marketplace supports)
- [ ] Usage statistics dashboard

### 4.5 Advanced Features
- [ ] Plugin development templates
- [ ] Local plugin development mode
- [ ] Plugin validation/linting tool
- [ ] Marketplace submission helper

### 4.6 Team Features (Future consideration)
- [ ] Cloud-synced configurations
- [ ] Team plugin collections
- [ ] Shared marketplace subscriptions
- [ ] Organization-level settings

## Technical Considerations

### CLI Architecture
```
ccpm (CLI)
    ↓
Shared Services Library (ClaudePluginManager.Core)
    ↓
Data Layer
```

### Cross-Platform Abstractions
- `IPlatformService` - Platform-specific operations
- `ICredentialStore` - Abstract credential storage
- `IPathProvider` - Platform-specific paths

### Package Distribution
- Windows: MSIX, WinGet
- macOS: DMG, Homebrew
- Linux: AppImage, Flatpak, Snap
- CLI: NuGet global tool

## Success Metrics
- macOS/Linux user adoption
- CLI tool download count
- Reduced support tickets for cross-platform issues
- Community marketplace contributions
