---
description: "Manage epics - create and list development epics"
argument-hint: "[command] [args...]"
---

# Epic Management

Handle epic management using the MCP tasks server.

**User input:** $ARGUMENTS

## Command Routing

| User Command | MCP Tool | Example |
|--------------|----------|---------|
| `add "Title"` | `mcp__tasks__add_epic` | `/epic add "UI Improvements"` |
| `add "Title" --priority P0` | `mcp__tasks__add_epic` with priority | `/epic add "Critical Fix" --priority P0` |
| `list` | `mcp__tasks__status` (shows all epics) | `/epic list` |

## Instructions

1. Parse the command from arguments
2. Call the appropriate MCP tool
3. Display results with epic ID and next steps

## Priorities

- **P0**: Critical/Urgent
- **P1**: High (default)
- **P2**: Medium
- **P3**: Low
