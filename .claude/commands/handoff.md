---
description: "Generate session handoff prompt for fresh session with full context"
---

# Session Handoff Generator

Generate a comprehensive session handoff prompt that the user can copy/paste to start a fresh Claude Code session.

**IMPORTANT:** Read `.claude/docs/session-handoff.md` for the full protocol and template format.

## Steps

1. **Gather current state**:
   - Current branch: `git branch --show-current`
   - Last commit: `git log -1 --format='%h %s'`
   - Check test/build status from recent session context (do NOT run commands directly)

2. **Review the conversation** for:
   - What major tasks/features were completed
   - What bugs were fixed
   - What files were significantly changed
   - Any blocked or in-progress work

3. **Check task status** via `mcp__tasks__status` for next priorities

4. **Output the handoff** using the template from `.claude/docs/session-handoff.md`:

```
# SecuRail FRMv2 - Session Handoff

## Just Completed
[Bullet list of what was finished this session]

## Current State
- **Branch:** [branch name]
- **Last commit:** [hash + message]
- **Tests:** [X passing]
- **Build:** [clean/warnings]

## Key Changes This Session
[Bullet list with file paths]

## Next Priority
[What should be worked on next from BACKLOG]

## Key Files
- Instructions: `CLAUDE.md` (READ FIRST)
- Project overview: `README.md`
- Backlog: `docs/BACKLOG.md`
```

## What NOT to Include

**NEVER include direct commands** like `dotnet build`, `dotnet test`, or `dotnet run`.

The project uses a subagent system. All operations must go through `.claude/agents/*.md` files. Reference `CLAUDE.md` as the entry point - the new session will follow proper protocols from there.

Present the handoff in a code block the user can copy.
