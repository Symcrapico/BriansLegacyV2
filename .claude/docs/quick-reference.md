# Quick Reference

## Subagent Cheat Sheet

| Need to... | Command |
|------------|---------|
| Commit code | `Task: "@git commit: description"` |
| Run tests | `Task: "@test"` |
| Build | `Task: "@build"` |
| Open browser | `Task: "@browser open https://localhost:5001"` |
| Add localization | `Task: "@resx translate Key 'English'"` |
| Generate handoff | `/handoff` |

## Slash Commands

| Command | Purpose |
|---------|---------|
| `/task` | Show backlog status |
| `/task start TASK-XXX` | Start working on task |
| `/task done TASK-XXX` | Complete a task |
| `/server restart` | Restart local server |
| `/server status` | Check server health |
| `/deploy` | Deploy to production |

## Common Workflows

### Start Work Session
1. `/task` - Review backlog status
2. `/task start TASK-XXX` - Activate a task
3. Code...
4. `@build` then `@test`
5. `@git commit: description`

### End Work Session
1. `@git commit: description` - Commit any remaining work
2. `/handoff` - Generate handoff prompt
3. Copy handoff to new session

### Fix a Bug
1. `/task start TASK-XXX` (or create task first)
2. Read relevant code
3. Make minimal fix
4. `@test [filter]` - Test the fix
5. `@git commit: Fix [description]`
6. `/task done TASK-XXX`

### Add a Feature
1. `/task start TASK-XXX`
2. Use `superpowers:brainstorming` skill
3. Implement incrementally
4. `@build` → `@test` after each change
5. `@git commit` for each logical unit
6. `/task done TASK-XXX`

## Task States

```
backlog → ready → in_progress → completed
                  ↑
            (ACTIVE = in_progress only)
```

## Golden Rules

1. **Safety:** If damage takes >5 min to fix → ASK FIRST
2. **Tasks:** Must have active task before writing code
3. **Delegation:** Use subagents for git/build/test/browser
4. **Changes:** Keep changes minimal (<50 lines/commit)
5. **Context:** Generate handoff when context gets heavy
