#!/bin/bash
# ============================================================================
# session-start.sh
# Purpose: Display backlog status at session start
# Dependencies: BACKLOG.md
# ============================================================================

# Check if BACKLOG.md exists
if [ ! -f "BACKLOG.md" ]; then
    echo "┌─────────────────────────────────────────────────────────────┐"
    echo "│ ⚠️  BACKLOG.md not found                                    │"
    echo "│    Run: /task init  to initialize task management          │"
    echo "└─────────────────────────────────────────────────────────────┘"
    exit 0
fi

# Read active task
ACTIVE=$(grep "^> Active:" BACKLOG.md 2>/dev/null | sed 's/> Active: //')

# Get active task description if there is one
if [ "$ACTIVE" != "None" ] && [ -n "$ACTIVE" ]; then
    ACTIVE_DESC=$(grep "| $ACTIVE |" BACKLOG.md 2>/dev/null | head -1 | sed 's/.*| '"$ACTIVE"' | \([^|]*\) |.*/\1/' | xargs)
fi

# Count blocked tasks
BLOCKED_COUNT=$(grep -c "| 🟥 |" BACKLOG.md 2>/dev/null || echo "0")

# Get blocked task IDs
if [ "$BLOCKED_COUNT" -gt 0 ]; then
    BLOCKED_TASKS=$(grep "| 🟥 |" BACKLOG.md 2>/dev/null | head -3 | sed 's/.*| \(TASK-[^ ]*\) |.*/   \1/')
fi

# Get next 3 ready tasks
NEXT_TASKS=$(grep "| 🟦 |" BACKLOG.md 2>/dev/null | head -3 | sed 's/.*| \(TASK-[^ ]*\) | \([^|]*\) |.*/   \1: \2/' | head -3)

# Count totals (rough estimate from task rows)
TOTAL_TASKS=$(grep -c "^| TASK-" BACKLOG.md 2>/dev/null || echo "0")
DONE_TASKS=$(grep -c "| ✅ |" BACKLOG.md 2>/dev/null || echo "0")
IN_PROGRESS=$(grep -c "| 🟨 |" BACKLOG.md 2>/dev/null || echo "0")
READY_TASKS=$(grep -c "| 🟦 |" BACKLOG.md 2>/dev/null || echo "0")

# Display status
echo "┌─────────────────────────────────────────────────────────────┐"
echo "│ 📋 BACKLOG STATUS                                           │"
echo "├─────────────────────────────────────────────────────────────┤"

if [ "$ACTIVE" != "None" ] && [ -n "$ACTIVE" ]; then
    echo "│ 🟨 ACTIVE: $ACTIVE"
    if [ -n "$ACTIVE_DESC" ]; then
        echo "│    $ACTIVE_DESC"
    fi
else
    echo "│ ⬜ No active task                                           │"
fi

echo "├─────────────────────────────────────────────────────────────┤"

if [ "$BLOCKED_COUNT" -gt 0 ]; then
    echo "│ 🟥 BLOCKED: $BLOCKED_COUNT task(s)                          │"
    echo "├─────────────────────────────────────────────────────────────┤"
fi

echo "│ 📌 NEXT READY:                                              │"
if [ -n "$NEXT_TASKS" ]; then
    echo "$NEXT_TASKS" | while IFS= read -r line; do
        if [ -n "$line" ]; then
            echo "│ $line"
        fi
    done
else
    echo "│    (no ready tasks)                                        │"
fi

echo "├─────────────────────────────────────────────────────────────┤"
echo "│ 📊 Ready: $READY_TASKS | In Progress: $IN_PROGRESS | Done: $DONE_TASKS │"
echo "└─────────────────────────────────────────────────────────────┘"
echo ""
echo "💡 Commands: /task start TASK-XXX | /task done TASK-XXX | /task"
