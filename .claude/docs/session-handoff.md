# Session Handoff Protocol

**CRITICAL:** At the end of every major task (feature implementation, phase completion, etc.), generate a **Session Handoff Prompt** for the user.

---

## Why

- Context grows large during implementation
- Fresh sessions are more efficient
- User can copy/paste to start new session with full context

---

## Format

Generate a markdown code block the user can copy:

```markdown
# SecuRail FRMv2 - Session Handoff

## Just Completed
[What was just finished - feature name, key changes]

## Current State
- **Branch:** [branch name]
- **Last commit:** [hash + message]
- **Tests:** [X passing]
- **Build:** [clean/warnings]

## Key Changes This Session
[Bullet list of major changes with file paths]

## Next Priority
[Next task from backlog with brief description]

## Key Files
- Instructions: `CLAUDE.md` (READ FIRST)
- Project overview: `README.md`
- Backlog: `docs/BACKLOG.md`
```

---

## What NOT to Include

**NEVER include direct commands** like `dotnet build` or `dotnet test` in handoffs.

The project uses a subagent system defined in CLAUDE.md. All operations (git, build, test, etc.) must go through reading `.claude/agents/*.md` files first. Including direct commands:
- Bypasses safety protocols
- Encourages bad habits in future sessions
- Undermines the entire instruction system

Instead, reference `CLAUDE.md` as the entry point. The new session will follow the proper protocols from there.

---

## When to Generate

- After completing a PH2-X phase task
- After completing any multi-commit feature
- When user says "let's wrap up" or similar
- Before any natural stopping point

---

## Context Self-Monitoring

### Heuristic Triggers

Generate a session handoff (via `/handoff`) when ANY of these occur:

| Signal | Threshold | Action |
|--------|-----------|--------|
| User messages | >20 | Consider handoff |
| Tool calls | >40 | Consider handoff |
| Large file reads | >5 | Consider handoff |
| Major features completed | >2 | Definitely handoff |
| Response feels slow | Any | Definitely handoff |
| Multi-file refactors | >1 | Definitely handoff |

### Self-Assessment Questions

Ask yourself periodically:
1. Have I read many large files this session?
2. Have I made many tool calls with verbose output?
3. Has the user completed multiple major tasks?
4. Am I starting to lose track of earlier context?

If yes to 2+ questions → Generate handoff.

### User-Assisted Check

When uncertain, ask:

> "I may be running low on context. Would you like me to generate a session handoff, or check with `/context` first?"

The user can:
- Type `/context` to see actual usage
- Say "generate handoff" to proceed
- Say "keep going" if plenty of context remains

### Handoff Workflow

```
1. Notice context pressure signals
2. Run /handoff (slash command)
3. Present handoff to user
4. User starts new session with handoff
```

### Why This Matters

- Context exhaustion causes degraded responses
- Fresh sessions are more efficient
- Handoffs preserve critical state
- User can continue seamlessly
