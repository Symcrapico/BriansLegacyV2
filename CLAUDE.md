# Claude Code Project Instructions — BriansLegacyV2

> **Read this file completely before starting any work. Violations of these protocols cause real problems.**

---

## Project Overview

**BriansLegacyV2** is a private online library for railway structural engineering materials. It enables a team of engineers to search, browse, and ask questions about books, documents, and plans using AI-powered cataloging and semantic search.

**Tech Stack:**
- ASP.NET Core 9.0 + Razor Pages
- SQL Server + Entity Framework Core 9.0
- PostgreSQL + pgvector (vector search)
- Tailwind CSS
- Hangfire (background jobs)
- Claude + Gemini APIs (AI processing)

**Design Document:** `docs/plans/2026-01-18-brianslegacy-design.md`

---

## CRITICAL: STOP BEFORE ACTING

Before executing ANY of the following, you MUST read the corresponding agent file:

| Operation | STOP & Read First |
|-----------|-------------------|
| Any `git` command | `.claude/agents/git.md` |
| `dotnet build` | `.claude/agents/build.md` |
| `dotnet test` | `.claude/agents/test.md` |
| Browser/UI work | `.claude/agents/browser.md` |
| Documentation/handoffs | `.claude/agents/docs.md` |
| Large code search | `.claude/agents/analyze.md` |

**This is not optional.** These files contain exact protocols, response formats, and safety rules you must follow. Do not rely on memory—re-read the file each session.

---

## Safety Policy

**The 5-Minute Rule**: If a mistake would take more than 5 minutes to fix, ASK FIRST.

This includes:
- Deleting or moving files
- Changing database schemas
- Modifying Docker/IIS configuration
- Any git force operations
- Bulk file modifications
- API key changes

Full policy: `.claude/docs/safety-policy.md`

---

## Task Management (Mandatory)

**No code changes without an active task.**

```
/task          → View backlog status
/task start    → Begin a task (REQUIRED before any code changes)
/task complete → Mark task done
```

Use MCP tools directly: `mcp__tasks__status`, `mcp__tasks__start`, `mcp__tasks__complete`

Full workflow: `.claude/docs/task-management.md`

---

## README Maintenance (Mandatory)

**The README.md must accurately reflect the current state of the project. Documentation drift is unacceptable.**

### What Must Be Kept Current

The README has these sections that MUST be updated as features are implemented:

| Section | Update When |
|---------|-------------|
| **Project Status** | Phase changes, major milestones |
| **Implemented Features** | Any new feature is working |
| **API Endpoints** | New endpoints added |
| **Configuration** | New appsettings keys added |
| **Project Structure** | New directories/significant files |

### Update Workflow

**After EVERY `feat()` commit, you MUST:**

1. **Check README.md** — Does it reflect the new feature?
2. **Update "Implemented Features"** — Add the feature with brief description
3. **Update "API Endpoints"** — If new endpoints were added
4. **Update "Configuration"** — If new config keys were added
5. **Commit separately** — `docs(readme): add [feature name]`

**Example after implementing file serving:**
```
## Implemented Features
### File Management
- Secure file serving at `/files/{fileId}` (download) and `/files/{fileId}/view` (inline)  <- ADD THIS
```

### Trigger Checklist

| Trigger | Action Required |
|---------|-----------------|
| `feat()` commit | Update Implemented Features section |
| New endpoint | Add to API Endpoints table |
| New config key | Add to Configuration section |
| Phase completion | Update Project Status |
| `/task complete` | Verify README reflects completed work |

### Quick Check Command

```
@docs readme-check
```

This scans recent commits and flags README sections that may need updates.

---

## Subagent System — How It Works

This project uses **file-based subagents**. They are NOT built-in—they are instruction files you must read and follow.

### Execution Protocol

```
1. RECOGNIZE   →  "I need to do git/build/test/etc."
2. STOP        →  Do NOT run commands directly
3. READ        →  Open `.claude/agents/{agent}.md`
4. EXECUTE     →  Follow the protocols in that file exactly
5. RESPOND     →  Use ONLY the response format from that file
```

### Self-Check Before Delegated Operations

- [ ] Did I read the agent file this session?
- [ ] Am I following its exact procedure?
- [ ] Am I using its response format?
- [ ] Am I respecting its safety rules?

**If any answer is "no" → STOP and read the agent file.**

---

## Skills — Invoke BEFORE Starting Work

Use the Skill tool proactively for these triggers:

| Trigger | Invoke This Skill |
|---------|-------------------|
| Creative work, new features | `superpowers:brainstorming` |
| Multi-step implementation | `superpowers:writing-plans` |
| Investigating bugs | `superpowers:systematic-debugging` |
| Before claiming "done" | `superpowers:verification-before-completion` |
| Finishing a branch | `superpowers:finishing-a-development-branch` |

---

## MCP Tools — Use Directly

| Need | MCP Tool |
|------|----------|
| Task management | `mcp__tasks__*` |
| Library documentation | `mcp__plugin_context7_context7__*` |

---

## Quick Commands

| Command | Purpose |
|---------|---------|
| `/task` | View backlog status |
| `/task start TASK-XXX` | Start working on a task |
| `/task complete` | Complete current task |

---

## Technical Standards

### .NET Version
- **.NET 9.0 ONLY**
- Never use `net10.0`
- All packages must be `9.0.x` versions

### Code Standards
- File headers required: `.claude/docs/file-headers.md`
- Design document: `docs/plans/2026-01-18-brianslegacy-design.md`

### Commit Standards
Handled by git subagent, but for reference:
```
type(scope): description

Co-Authored-By: Claude Opus 4.5 <noreply@anthropic.com>
```
Types: `feat`, `fix`, `docs`, `refactor`, `test`, `chore`, `perf`, `style`

### Terminology
- **LibraryItem** — Base entity for books, documents, and plans
- **Book** — Physical book with cover photo
- **Document** — PDF document (reports, manuals, etc.)
- **Plan** — Engineering drawing/plan with title block

### Project Root Cleanliness
**Keep the project root as clean as possible.** Only essential files belong at the root level.

**Allowed at root:**
- `CLAUDE.md`, `README.md` — Project documentation
- `.gitignore`, `.editorconfig` — Git and editor config
- `docker-compose.yml` — Container orchestration
- `*.sln` — Solution file
- Standard dotfiles (`.claude/`, `.github/`, etc.)

**Move elsewhere:**
- Plans and design docs → `docs/plans/`
- Scripts → `scripts/` or `.claude/`
- Temporary files → Never commit
- Config files for specific tools → Appropriate subdirectory when possible

**Before adding a file to root, ask:** "Does this NEED to be at root level, or can it live in a subdirectory?"

---

## Methodology

1. **Read before claiming** — Always read code before making statements about it
2. **Check in before big changes** — New files, API changes, or >100 lines? Ask first.
3. **Minimal changes** — Prefer <50 lines per commit
4. **Document plans** — Save plans to `docs/plans/`
5. **High-level explanations** — 2-3 sentences per change, not essays

---

## Session Management

### Handoff Triggers
Initiate handoff when ANY of these occur:
- More than 40 tool calls
- More than 20 messages
- 2+ features completed

### Creating a Handoff

```
1. @docs readme-check        ← Ensure docs are current first
2. /handoff                  ← Generate handoff document
```

Full protocol: `.claude/docs/session-handoff.md`

---

## Project Structure Reference

```
BriansLegacyV2/
├── .claude/              ← Claude Code configuration
│   ├── agents/           ← Subagent instruction files (READ THESE)
│   ├── commands/         ← Custom commands
│   ├── docs/             ← Policy and workflow documentation
│   ├── hooks/            ← Claude Code hooks
│   └── mcp-servers/      ← Custom MCP servers (tasks)
├── docs/
│   ├── plans/            ← Design and implementation plans
│   ├── BACKLOG.md        ← Task backlog
│   └── backup-restore.md ← Backup procedures
├── src/
│   └── BriansLegacy/     ← Main ASP.NET Core application
│       ├── Data/         ← DbContext and migrations
│       ├── Models/       ← Domain models
│       ├── Services/     ← Business logic
│       ├── Jobs/         ← Hangfire background jobs
│       ├── Pages/        ← Razor Pages
│       └── wwwroot/      ← Static assets
├── docker-compose.yml    ← PostgreSQL + pgvector
├── CLAUDE.md             ← This file
└── README.md             ← Project documentation
```

---

## Common Mistakes to Avoid

| Mistake | Why It's Wrong | Do This Instead |
|---------|----------------|-----------------|
| `git add .` or `git add -A` | Stages unrelated files | Stage specific files only |
| `git commit -m "fix"` | Meaningless history | Use conventional commit format |
| Skipping agent files | Violates protocols | Always read agent file first |
| Starting code without `/task start` | Untracked work | Start task first |
| Long explanations after git | Subagent says minimal | Follow response format |
| Using `net10.0` | Wrong framework | Use `net9.0` only |
| Skipping `@docs readme-check` | Documentation drift | Always check before task complete |
| Adding files to project root | Clutters root directory | Use appropriate subdirectory |

---

## Quick Decision Tree

```
Need to do something?
│
├─ Is it git/build/test/browser/docs/analyze?
│  └─ YES → Read .claude/agents/{type}.md FIRST, then proceed
│
├─ Is it a code change?
│  └─ YES → Is there an active task?
│           ├─ NO → Run /task start first
│           └─ YES → Proceed with change
│
├─ Am I completing a task / feat commit / branch?
│  └─ YES → Run @docs readme-check FIRST
│
├─ Could this take >5 min to undo?
│  └─ YES → ASK before doing it
│
└─ Unsure about anything?
   └─ ASK — it's always better to ask
```

---

## Remember

1. **Subagent files are mandatory reading** — not suggestions
2. **Response formats matter** — use what the agent file specifies
3. **README must stay current** — check on every task/feature completion
4. **When in doubt, ask** — the 5-minute rule exists for a reason
5. **Tasks track everything** — no untracked work
6. **Re-read agent files each session** — don't rely on memory
