# Subagent Reference

## Golden Rule

**NEVER run these commands directly in the main session:**
- `git *` → use `@git`
- `dotnet build/test/clean` → use `@build` / `@test`
- `mcp__playwright__*` → use `@browser`
- RESX file reads → use `@resx`

## When to Use Subagents

Use a subagent when the task:
- Takes >15 minutes
- Reads >3 files
- Could produce verbose output
- Could fail and need retry

When unsure, use a subagent.

---

## @git - Git Operations

```
Task: "@git commit: Add tax calculation service"
Task: "@git status"
Task: "@git push"
```

| Command | Purpose |
|---------|---------|
| `@git commit: [desc]` | Commit with message |
| `@git status` | Show repo state |
| `@git push` | Push to remote |
| `@git create branch [name]` | Create and checkout |
| `@git sync` | Pull with rebase |
| `@git log` | Recent commits |

**Exception:** Only run git directly when debugging git-specific issues with user's explicit request.

---

## @test - Test Execution

```
Task: "@test"
Task: "@test InvoiceService"
Task: "@test quick"
```

| Command | Purpose |
|---------|---------|
| `@test` | Run all tests |
| `@test [filter]` | Run filtered tests |
| `@test quick` | Skip rebuild |
| `@test coverage` | With coverage report |

**Workflow:** Make changes → `@build` → `@test` → `@git commit`

---

## @build - Build Operations

```
Task: "@build"
Task: "@build clean"
Task: "@build release"
```

| Command | Purpose |
|---------|---------|
| `@build` | Standard build |
| `@build clean` | Clean and rebuild |
| `@build release` | Release configuration |
| `@build check` | Syntax validation only |

---

## @browser - Browser Automation

```
Task: "@browser open https://localhost:5001"
Task: "@browser click 'Submit'"
Task: "@browser close"
```

| Command | Purpose |
|---------|---------|
| `@browser open [url]` | Navigate |
| `@browser click [element]` | Click |
| `@browser fill [form]` | Fill fields |
| `@browser screenshot [name]` | Capture |
| `@browser check [description]` | Verify state |
| `@browser login [role]` | Login as FRM/OP |
| `@browser close` | Close browser |

---

## @resx - Localization

```
Task: "@resx scan Pages/FRM/NewPage.cshtml"
Task: "@resx translate NewKey 'English text'"
Task: "@resx add Key 'EN' 'FR'"
```

| Command | Purpose |
|---------|---------|
| `@resx add KEY "EN" "FR"` | Add key |
| `@resx update KEY "EN" "FR"` | Update key |
| `@resx get KEY` | Check value |
| `@resx scan [path]` | Find missing keys |
| `@resx translate KEY "EN"` | Auto-translate to French |
| `@resx list [pattern]` | Search keys |
| `@resx audit` | Check conventions |

**Why:** RESX files are 8,400+ lines - reading them destroys context.

---

## @docs - Documentation

```
Task: "@docs readme update tests"
Task: "@docs readme-check"
```

| Command | Purpose |
|---------|---------|
| `@docs readme-check` | Quick check if README needs updates |
| `@docs readme update [section]` | Update README |
| `@docs todos complete [item]` | Mark task done |
| `@docs summary [scope]` | Work summary |

---

## @analyze - Code Analysis

```
Task: "@analyze patterns async"
Task: "@analyze security"
Task: "@analyze quality Services/"
```

| Command | Purpose |
|---------|---------|
| `@analyze patterns [type]` | Find patterns |
| `@analyze conventions` | Check naming |
| `@analyze quality [scope]` | Code metrics |
| `@analyze security` | Security scan |
| `@analyze dependencies` | Dependency graph |
| `@analyze find [pattern]` | Search with context |

---

## Skills Reference

**Use skills PROACTIVELY** - invoke them BEFORE starting work, not after.

| Trigger | Skill | Why |
|---------|-------|-----|
| Any creative/feature work | `superpowers:brainstorming` | Explores requirements before coding |
| Multi-step implementation | `superpowers:writing-plans` | Creates actionable plan first |
| Any feature with tests | `superpowers:test-driven-development` | Tests before implementation |
| Any bug/error | `superpowers:systematic-debugging` | Root cause before fix |
| Before claiming done | `superpowers:verification-before-completion` | Evidence before assertions |
| After completing branch | `superpowers:finishing-a-development-branch` | Guides merge/PR decision |

**Also available:**
- `superpowers:code-review` - Review completed work
- `superpowers:subagent-driven-development` - Execute plans with agents
- `feature-dev:feature-dev` - Guided feature development

---

## MCP Tools Reference

Use MCP tools directly (no subagent needed):

| MCP | Tools | Purpose |
|-----|-------|---------|
| Tasks | `mcp__tasks__status`, `mcp__tasks__start`, `mcp__tasks__complete`, etc. | Task/backlog management |
| Context7 | `mcp__plugin_context7_context7__resolve-library-id`, `mcp__plugin_context7_context7__query-docs` | Up-to-date library documentation |
