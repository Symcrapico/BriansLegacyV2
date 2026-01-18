# Task Management Documentation

**CRITICAL:** ALL development work MUST be tracked in the task management system via `BACKLOG.md`.

---

## Session Start

At the beginning of EVERY session:
1. The session start hook automatically displays current backlog status
2. Review the active task, blocked items, and next ready tasks
3. Either continue the active task or start a new one with `/task start TASK-XXX`

---

## Before Writing Code

> **ABSOLUTELY IMPERATIVE - NO EXCEPTIONS**
>
> You MUST have an active task before writing ANY code. This is NON-NEGOTIABLE.
> Violating this rule wastes the user's time and breaks project tracking.
> DO NOT rationalize skipping this step. DO NOT assume the user wants you to "just do it."

1. **STOP** - Check if there is an active task (`mcp__tasks__current`)
2. **If no active task exists:**
   - Check if the work is in the backlog
   - If NOT in backlog → **ASK USER FIRST**: "This isn't in the backlog. Should I create a task for it?"
   - **WAIT for user confirmation** before proceeding
   - Create the feature/task with `mcp__tasks__add_feature` / `mcp__tasks__create`
   - Start the task with `mcp__tasks__start`
3. **Only AFTER a task is active** may you write code

### Task States

```
backlog → ready → in_progress → completed
                  ↑
            ACTIVE (only this status)
```

**A task is ACTIVE only when its status is `in_progress`.**

- `backlog` = not ready to work on
- `ready` = can be started, but NOT active
- `in_progress` = **ACTIVE** - this is the only state that allows code edits
- `completed` = done

Use `mcp__tasks__start` to move a task to `in_progress` (active).

---

## After Completing Work

1. Commit with task ID: `TASK-XXX-X-N: description`
2. Call `mcp__tasks__complete` with the task ID and commit hash
3. Call `mcp__tasks__next` to see what's next

---

## Available MCP Tools

| Tool | Purpose |
|------|---------|
| `mcp__tasks__status` | Full backlog summary (active, blocked, next, progress) |
| `mcp__tasks__current` | Get the currently active task |
| `mcp__tasks__next` | Get next N ready tasks |
| `mcp__tasks__list` | List tasks with filters (status, epic, feature) |
| `mcp__tasks__get` | Get details of a specific task |
| `mcp__tasks__start` | Begin working on a task (creates detail file) |
| `mcp__tasks__complete` | Finish a task (moves to completed section) |
| `mcp__tasks__create` | Create a new task in a feature |
| `mcp__tasks__block` | Mark a task as blocked with reason |
| `mcp__tasks__unblock` | Remove block from a task |
| `mcp__tasks__update` | Update task description/estimate |
| `mcp__tasks__delete` | Archive/cancel a task |
| `mcp__tasks__add_epic` | Create a new epic |
| `mcp__tasks__add_feature` | Create a new feature under an epic |

---

## Slash Commands

| Command | Purpose |
|---------|---------|
| `/task` | Show current backlog status |
| `/task start TASK-XXX` | Start working on a task |
| `/task done TASK-XXX` | Complete a task |
| `/task done TASK-XXX abc123` | Complete with commit hash |
| `/task add "desc" --feature FEAT-XXX` | Create a new task |
| `/task list` | List all tasks |
| `/task list --ready` | List ready tasks |
| `/task block TASK-XXX "reason"` | Block a task |
| `/task init` | Initialize task management system |
| `/epic add "title"` | Create a new epic |
| `/feature add "title" --epic EPIC-XXX` | Create a new feature |

---

## Task Hierarchy

```
EPIC-001: Major initiative (e.g., "Email Template Migration")
├── FEAT-001-A: Feature (e.g., "RequestEmailService Templates")
│   ├── TASK-001-A-1: Individual task (~30 min)
│   ├── TASK-001-A-2: Individual task
│   └── ...
├── FEAT-001-B: Another feature
│   └── ...
```

---

## What Claude Must NEVER Do

- Edit code without an active task
- Commit without task ID in message (unless [no-task] bypass)
- Ignore the backlog status at session start
- Work on untracked tasks without asking to create them first
- Skip updating task status after completing work
