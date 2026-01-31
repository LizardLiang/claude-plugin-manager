# CLAUDE.md

This file provides guidance to Claude Code when working with code in this repository.

## Quick Reference

- **Build**: `dotnet build ClaudePluginManager`
- **Run**: `dotnet run --project ClaudePluginManager`
- **Test**: `dotnet test ClaudePluginManager.Tests`

## Rules (Read When Needed)

The following rule files contain detailed guidance. Read the relevant file when working on related tasks:

| Rule File | When to Read |
|-----------|--------------|
| `.rule/project-overview.md` | Starting work, understanding the project, checking tech stack |
| `.rule/development-workflow.md` | Writing new code, implementing features, working with tests |
| `.rule/architecture.md` | Understanding codebase structure, adding new components, working with plugins/config |
| `.rule/security.md` | Handling credentials, tokens, GitHub API calls, plugin installations |
| `.rule/task-management.md` | Creating/updating tasks, working with `.tasks/` directory |

## Always Remember

1. **TDD**: Write tests first, no production code without tests
2. **Context7**: Check API specs before implementing external API code
3. **Security**: Never store tokens in plain text, use OS credential manager
4. **Task Tracking**: When implementing a task from `.tasks/`, use `TaskCreate` to add "Update task files" as the final item in your task list. When done, update BOTH the task file (mark acceptance criteria as `[x]`) AND the `index.md` (update task status and summary counts)
