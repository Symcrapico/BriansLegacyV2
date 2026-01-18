# Dangerous Commands Policy

**YOU are the primary safety layer. The hook is a backup, not a substitute for judgment.**

A safety hook exists at `.claude/hooks/safety-check.py` that uses Haiku to evaluate risky bash commands. However, this hook can fail silently (API errors, missing package, network issues). **Never rely on the hook to catch dangerous commands.**

---

## Commands Claude Must NEVER Run

These commands are **ALWAYS FORBIDDEN** regardless of context:

| Pattern | Why |
|---------|-----|
| `rm -rf /`, `rm -rf /*` | Destroys entire filesystem |
| `rm -rf ~`, `rm -rf $HOME` | Destroys user's home directory |
| `rm -rf .` (in project root) | Destroys entire project |
| `rm` with `--no-preserve-root` | Bypasses safety checks |
| `mkfs.*` | Formats filesystems |
| `dd` to block devices | Overwrites disks |
| `chmod -R 777 /` | Destroys system security |
| `> /dev/sda` (or any block device) | Overwrites disk |

### Windows-Specific Forbidden Commands

| Pattern | Why |
|---------|-----|
| `del /s /q C:\` | Recursive system delete |
| `rd /s /q C:\Users` | Destroys user data |
| `rd /s /q C:\Windows` | Destroys OS |
| `Format C:` | Formats boot drive |
| `Remove-Item -Recurse -Force C:\` | PowerShell equivalent |
| `reg delete HKLM\*` | Registry destruction |
| `sc delete` (system services) | Service destruction |

---

## Commands That Require Explicit User Confirmation

Before running these, **ASK THE USER FIRST**:

```
"I need to run `[command]` which will [effect]. Is that okay?"
```

| Command Type | Example | Risk |
|--------------|---------|------|
| Recursive delete outside project | `rm -rf /var/log/*` | Data loss |
| Sudo anything | `sudo apt install` | System modification |
| Mass permission changes | `chmod -R 755 /etc` | Security risk |
| Process killing | `killall node` | Service disruption |
| System service changes | `systemctl stop nginx` | Service disruption |

---

## Safe Patterns (No Confirmation Needed)

These are normal development operations:

- `rm -rf node_modules` - Cleaning dependencies
- `rm -rf bin/ obj/` - Cleaning build artifacts
- `rm -rf .next/ .nuxt/` - Cleaning framework caches
- `rm *.log` - Cleaning log files in current directory
- `mv file1.txt file2.txt` - Renaming within project

---

## The Golden Rule

> **If a command could cause damage that takes more than 5 minutes to fix, ASK FIRST.**

---

## What If the Hook Blocks Something Legitimate?

If the safety hook incorrectly blocks a safe command:
1. Tell the user: "The safety hook blocked this command: `[cmd]`. It appears safe to me because [reason]. Should I ask you to run it manually?"
2. The user can run it themselves or temporarily disable the hook

---

## Verifying the Hook Is Working

The hook requires:
- `anthropic` Python package installed
- `ANTHROPIC_API_KEY` environment variable set
- Network access to Anthropic API

If any of these fail, the hook allows all commands (graceful degradation). **This is why you must be careful regardless.**
