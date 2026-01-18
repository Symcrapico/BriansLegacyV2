# Claude Code Subagents

This directory contains specialized subagents that handle specific domains of work, keeping the main conversation context clean and efficient.

## Available Agents

| Agent | File | Purpose |
|-------|------|---------|
| **git** | `git.md` | All version control operations |
| **test** | `test.md` | Test execution and reporting |
| **build** | `build.md` | Project compilation and error reporting |
| **server** | `server.md` | Development server lifecycle management |
| **browser** | `browser.md` | Playwright browser automation |
| **resx** | `resx.md` | Localization key management and scanning |

## How to Invoke

Use the Task tool with `subagent_type="general-purpose"` and reference the agent in your prompt:

```
Task: "@git commit: Add user authentication feature"
Task: "@test all"
Task: "@build"
Task: "@server start"
Task: "@browser open https://localhost:5001"
Task: "@resx scan Pages/FRM/"
```

Or directly describe what you need:

```
Task: "Use the git agent to commit the billing changes"
Task: "Run the invoice service tests and report failures"
```

## Git Agent Commands

| Command | Description | Example |
|---------|-------------|---------|
| `@git commit: [desc]` | Stage and commit with description | `@git commit: fix null reference in invoice service` |
| `@git status` | Brief status summary | Returns: "3 modified, 1 staged" |
| `@git push` | Push current branch | Returns: "Pushed main to origin" |
| `@git create branch [name]` | Create and switch to branch | `@git create branch feature/billing-v2` |
| `@git sync` | Pull and rebase from origin | Returns: "Synced, 5 commits pulled" |
| `@git log` | Show recent commits | Returns last 10 commits, one-line format |

## Test Agent Commands

| Command | Description | Example |
|---------|-------------|---------|
| `@test` or `@test all` | Run all tests | Returns: "✓ 253 passed (14.2s)" |
| `@test [filter]` | Run tests matching filter | `@test InvoiceService` |
| `@test quick` | Run without rebuild | Faster for iterative testing |
| `@test coverage` | Run with coverage | Returns coverage percentages |

## Build Agent Commands

| Command | Description | Example |
|---------|-------------|---------|
| `@build` or `@build all` | Build the project | Returns: "✓ Build succeeded (6.3s)" |
| `@build clean` | Clean and rebuild | Full rebuild from scratch |
| `@build release` | Build Release config | For production builds |
| `@build check` | Syntax check only | Quick validation without output |

## Server Agent Commands

| Command | Description | Example |
|---------|-------------|---------|
| `@server start` | Start dev server | Returns: "✓ Server started at https://localhost:5001" |
| `@server stop` | Stop dev server | Returns: "✓ Server stopped" |
| `@server restart` | Restart dev server | Stop then start |
| `@server status` | Check if running | Returns running/stopped + health |
| `@server logs` | Show recent logs | Last 30 lines, condensed |
| `@server logs errors` | Show only errors | Error/exception entries |

## Browser Agent Commands

| Command | Description | Example |
|---------|-------------|---------|
| `@browser open [url]` | Navigate to URL | Returns: "✓ Opened /FRM/Dashboard" |
| `@browser click [element]` | Click element | Returns: "✓ Clicked 'Submit' button" |
| `@browser fill [form]` | Fill form fields | Returns: "✓ Filled login form" |
| `@browser screenshot [name]` | Capture screenshot | Returns: "✓ Screenshot saved: tmp/page.png" |
| `@browser check [description]` | Verify page state | Returns: "✓ Verified: Success message visible" |
| `@browser console` | Get console errors | Returns error/warning summary |
| `@browser login [role]` | Login as FRM/OP | Returns: "✓ Logged in as FRM user" |
| `@browser close` | Close browser | Returns: "✓ Browser closed" |

## RESX Agent Commands

| Command | Description | Example |
|---------|-------------|---------|
| `@resx add KEY "EN" "FR"` | Add new key | Returns: "✓ Added: KEY" |
| `@resx batch {...}` | Add multiple keys | Returns: "✓ Added 5 keys" |
| `@resx update KEY "EN" "FR"` | Update existing key | Returns: "✓ Updated: KEY" |
| `@resx get KEY` | Get key value | Returns EN and FR values |
| `@resx list [pattern]` | List matching keys | Returns: "Found 12 keys matching..." |
| `@resx delete KEY` | Delete a key | Returns: "✓ Deleted: KEY" |
| `@resx count` | Count total keys | Returns: "Total keys: 1,287" |
| `@resx diff` | Find missing between locales | Returns sync status |
| `@resx scan [path]` | Scan views for missing keys | Returns missing keys with line numbers |
| `@resx translate KEY "EN"` | Auto-translate to French | Returns: "✓ Translated: KEY" |
| `@resx audit` | Check naming conventions | Returns issues and suggestions |
| `@resx unused` | Find unreferenced keys | Returns potentially unused keys |
| `@resx bulk-translate [pattern]` | Translate all matching | Batch translate missing French |

## Why Subagents?

1. **Context preservation** - Verbose output (diffs, test logs) stays out of main session
2. **Consistency** - Subagents follow the same patterns every time
3. **Clean responses** - Main session only sees concise summaries
4. **Parallel execution** - Multiple subagents can run simultaneously

## Adding New Agents

1. Create `[name].md` in this directory
2. Include: purpose, commands, response format, safety rules
3. Update this README
4. Add policy section to `/CLAUDE.md` if needed

## Best Practices

- Let agents handle their entire domain
- Provide clear context for operations
- Check agent responses for warnings/errors
- Use agents even for "quick" operations to maintain consistency
