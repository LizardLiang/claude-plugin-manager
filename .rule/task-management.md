# Task Management

Tasks are organized in `.tasks/[phase-name]/` directories.

## Directory Structure

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

## Task File Format

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

## Index File Format

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

## Rules

1. **Task Completion**: A task may ONLY be marked as `[x] Completed` when ALL acceptance criteria are fulfilled
2. **Progress Tracking**: Update `index.md` whenever a task status changes
3. **Sequential Work**: Complete tasks in order unless dependencies allow parallel work
4. **No Partial Completion**: If any acceptance criterion is not met, the task remains `In Progress` or `Not Started`
