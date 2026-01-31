# Phase 3: Full Release (v1.0)

## Goal
Polish the application with group sharing, auto-updates, configuration editing, and optimized UX.

## Prerequisites
- Phase 2 Core Features completed

## Features

### 3.1 Group Export/Import (FR-5.7, FR-5.8)
- [ ] Export group as JSON file
- [ ] Import group from JSON file
- [ ] Validate imported group format
- [ ] Handle missing plugins on import (warn user)
- [ ] File picker dialogs for export/import

### 3.2 Group Cloning (FR-5.9)
- [ ] Duplicate/clone existing group
- [ ] Rename cloned group

### 3.3 Version Pinning in Groups (FR-5.2 enhancement)
- [ ] Optional version pinning when adding to group
- [ ] Support version constraints: pinned ("1.2.3"), range ("^1.0.0"), or "latest"
- [ ] UI for selecting version constraint

### 3.4 Update Detection (FR-3.2)
- [ ] Check for available updates from marketplace
- [ ] Show update indicator on plugins
- [ ] "Updates Available" section in Installed view

### 3.5 Plugin Updates (FR-3.3, FR-3.4)
- [ ] Update single plugin to latest version
- [ ] Update all outdated plugins in batch
- [ ] Show changelog/release notes before update (FR-3.5)

### 3.6 Auto-Update Configuration (FR-3.7)
- [ ] Configure update behavior: AUTO | NOTIFY | MANUAL
- [ ] Background update check on app start
- [ ] Notification for available updates

### 3.7 Version Rollback (FR-3.6)
- [ ] Track previous installed versions
- [ ] Rollback to previous version
- [ ] Version history per plugin

### 3.8 Plugin Enable/Disable (FR-2.4)
- [ ] Enable/disable plugin without uninstalling
- [ ] Disabled plugins remain in config but inactive
- [ ] Visual indicator for disabled state

### 3.9 Plugin Configuration (FR-2.5, FR-2.6)
- [ ] View plugin configuration and settings
- [ ] Edit plugin configuration
- [ ] JSON schema-based config editor
- [ ] Validate config against plugin's ConfigSchema

### 3.10 Marketplace Priority (FR-4.5)
- [ ] Set marketplace priority order
- [ ] Drag-and-drop reordering
- [ ] Priority affects conflict resolution

### 3.11 Performance Optimizations
- [ ] Application startup time < 3 seconds
- [ ] Plugin list load time < 2 seconds
- [ ] Memory usage (idle) < 200 MB
- [ ] Lazy loading for plugin details
- [ ] Virtualized lists for large plugin collections

### 3.12 UI/UX Polish
- [ ] Theme support: Light | Dark | System
- [ ] Consistent loading states and spinners
- [ ] Error messages with recovery actions
- [ ] Keyboard navigation
- [ ] Accessibility improvements

## Technical Requirements

### Group Export Format
```json
{
  "name": "Group Name",
  "description": "Optional description",
  "exportedAt": "2026-01-31T12:00:00Z",
  "plugins": [
    { "id": "marketplace/plugin-name", "version": "latest" },
    { "id": "marketplace/other-plugin", "version": "2.0.0" }
  ]
}
```

### Update Service
- `UpdateService` - Check and apply plugin updates
- Background update checker
- Changelog fetching from GitHub releases

### Config Editor
- JSON editor component
- Schema validation
- Syntax highlighting

## Acceptance Criteria
- Groups can be exported and imported between users
- Update detection works reliably (100% accuracy)
- Config editor validates against schema
- Application meets all performance targets
- Theme switching works correctly
- All P0 and P1 requirements completed
