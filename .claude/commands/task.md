---
description: "Manage development tasks - view status, start, complete, create tasks"
argument-hint: "[command] [args...]"
---

# Task Management

Handle the user's task management request using the MCP tasks server.

**User input:** $ARGUMENTS

## Command Routing

Parse the command and call the appropriate MCP tool:

| User Command | MCP Tool | Example |
|--------------|----------|---------|
| (empty) or `status` | `mcp__tasks__status` | `/task` |
| `list` | `mcp__tasks__list` | `/task list` |
| `list --ready` | `mcp__tasks__list` with status=ready | `/task list --ready` |
| `list --blocked` | `mcp__tasks__list` with status=blocked | `/task list --blocked` |
| `current` | `mcp__tasks__current` | `/task current` |
| `next` | `mcp__tasks__next` | `/task next` |
| `get TASK-XXX` | `mcp__tasks__get` | `/task get TASK-001-A-1` |
| `start` | `mcp__tasks__start` (no id) | `/task start` → starts next ready task |
| `start TASK-XXX` | `mcp__tasks__start` | `/task start TASK-001-A-1` |
| `done TASK-XXX` | `mcp__tasks__complete` | `/task done TASK-001-A-1` |
| `done TASK-XXX COMMIT` | `mcp__tasks__complete` with commit | `/task done TASK-001-A-1 abc123` |
| `add "desc" --feature FEAT-XXX` | `mcp__tasks__create` | `/task add "Fix bug" --feature FEAT-001-A` |
| `block TASK-XXX "reason"` | `mcp__tasks__block` | `/task block TASK-001-A-1 "waiting for design"` |
| `unblock TASK-XXX` | `mcp__tasks__unblock` | `/task unblock TASK-001-A-1` |
| `update TASK-XXX --desc "new"` | `mcp__tasks__update` | `/task update TASK-001-A-1 --desc "Updated"` |
| `delete TASK-XXX` | `mcp__tasks__delete` | `/task delete TASK-001-A-1` |
| `init` | Guide user through initialization | `/task init` |
| `inbox` | Process TODOs.md inbox items | `/task inbox` |

## Instructions

1. If no arguments or `status`: Call `mcp__tasks__status` and display formatted results
2. For other commands: Parse arguments and call the appropriate MCP tool
3. Display results clearly with formatting
4. For `init`: Guide user through creating BACKLOG.md with first epic and feature

## Init Flow

If user runs `/task init`:
1. Ask for first epic title (e.g., "Email Template Migration")
2. Ask for first feature title (e.g., "RequestEmailService")
3. Create BACKLOG.md with the epic and feature
4. Create tasks/ directory
5. Confirm setup complete

## Inbox Flow

If user runs `/task inbox`:

1. Read TODOs.md and get all list items (lines starting with `-`)
2. For each item, ask user which feature to add it to (show available features)
3. Create task via `mcp__tasks__create`
4. Remove processed line from TODOs.md
5. Show summary of created tasks
