#!/bin/bash
# ============================================================================
# check-active-task.sh
# Purpose: BLOCK editing code without an active task
# Dependencies: BACKLOG.md
# ============================================================================

# Skip check if no BACKLOG.md (new project setup)
if [ ! -f "BACKLOG.md" ]; then
    exit 0
fi

# Check for active task
ACTIVE=$(grep "^> Active:" BACKLOG.md 2>/dev/null | sed 's/> Active: //')

if [ "$ACTIVE" = "None" ] || [ -z "$ACTIVE" ]; then
    echo "" >&2
    echo "❌ BLOCKED: No active task" >&2
    echo "" >&2
    echo "You must have an active task before editing code." >&2
    echo "Run: /task start TASK-XXX" >&2
    echo "" >&2
    exit 2  # Block the operation
fi

# Task is active, allow edit
exit 0
