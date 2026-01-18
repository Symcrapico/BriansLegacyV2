---
description: "Manage features - create features under epics"
argument-hint: "[command] [args...]"
---

# Feature Management

Handle feature management using the MCP tasks server.

**User input:** $ARGUMENTS

## Command Routing

| User Command | MCP Tool | Example |
|--------------|----------|---------|
| `add "Title" --epic EPIC-XXX` | `mcp__tasks__add_feature` | `/feature add "Dark Mode" --epic EPIC-001` |
| `list` | `mcp__tasks__list` (shows features) | `/feature list` |
| `list --epic EPIC-XXX` | `mcp__tasks__list` filtered by epic | `/feature list --epic EPIC-001` |

## Instructions

1. Parse the command from arguments
2. For `add`: Require --epic parameter, call `mcp__tasks__add_feature`
3. For `list`: Call `mcp__tasks__status` or `mcp__tasks__list`
4. Display results with feature ID and task count

## Feature ID Format

Features are auto-generated as: `FEAT-{epic#}-{letter}`
- Example: `FEAT-001-A`, `FEAT-001-B`, `FEAT-002-A`
