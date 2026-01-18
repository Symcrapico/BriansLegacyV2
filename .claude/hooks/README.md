# Claude Code Hooks

This directory contains hooks that extend Claude Code's functionality with safety checks and workflow automation.

## Hooks Overview

| Hook | Trigger | Purpose |
|------|---------|---------|
| `session-start.sh` | SessionStart | Displays backlog status at session start |
| `check-active-task.sh` | PreToolUse (Edit/Write) | Ensures a task is active before editing files |
| `safety-check.py` | PreToolUse (Bash) | Blocks dangerous bash commands using AI evaluation |

---

## Safety Check Hook (`safety-check.py`)

This hook protects against accidentally running destructive bash commands like `rm -rf /` or `dd` to block devices.

### How It Works

1. **Immediate Blocklist**: Catastrophic commands are blocked instantly without an API call:
   - `rm -rf /`, `rm -rf /*`, `rm -rf ~`, `rm -rf $HOME`, `rm -rf .`
   - `rm` with `--no-preserve-root`
   - `mkfs.*` commands
   - `dd` writing to block devices
   - `chmod -R 777 /`
   - Fork bombs

2. **AI Evaluation**: Commands containing risky patterns (rm, chmod, sudo, etc.) are evaluated by Claude Haiku:
   - Haiku assesses whether the command could cause unrecoverable damage
   - Returns a risk level: low, medium, high, or critical
   - High/critical risks are blocked; medium risks require user confirmation
   - Low risks and normal commands are allowed

### Requirements

- **Python Package**: `anthropic` must be installed
  ```bash
  pip install anthropic
  # or
  pip3 install anthropic
  ```

- **Environment Variable**: `ANTHROPIC_API_KEY` must be set
  ```bash
  export ANTHROPIC_API_KEY="sk-ant-..."
  ```

### Testing the Hook

Test the blocklist (these should be blocked):
```bash
# In Claude Code, try asking to run:
rm -rf /
rm -rf ~
dd if=/dev/zero of=/dev/sda
```

Test risky but safe commands (these should be allowed):
```bash
# These should pass Haiku evaluation:
rm -rf node_modules
rm -rf build/
mv file1.txt file2.txt
```

### Temporarily Disabling

To disable the safety hook temporarily:

1. **Edit `.claude/settings.json`**:
   Remove or comment out the Bash matcher block in `PreToolUse`

2. **Or rename the script**:
   ```bash
   mv .claude/hooks/safety-check.py .claude/hooks/safety-check.py.disabled
   ```

3. **Or set an environment variable** (if you modify the script):
   Add this at the top of `main()`:
   ```python
   if os.environ.get("SKIP_SAFETY_CHECK"):
       sys.exit(0)
   ```

### Customizing

#### Adding to the Immediate Blocklist

Edit the `IMMEDIATE_BLOCKLIST` list in `safety-check.py`:
```python
IMMEDIATE_BLOCKLIST = [
    # ... existing patterns ...
    r"your-new-pattern-here",
]
```

Patterns are Python regular expressions.

#### Adding Risky Patterns

Edit the `RISKY_PATTERNS` list to trigger Haiku evaluation:
```python
RISKY_PATTERNS = [
    # ... existing patterns ...
    r"\bnew-command\b",
]
```

#### Adjusting Haiku's Prompt

Modify the `evaluate_with_haiku()` function to change how commands are evaluated.

### Exit Codes

| Code | Meaning |
|------|---------|
| 0 | Allow the command |
| 2 | Block the command (stderr shown to Claude) |
| Other | Non-blocking error (command proceeds with warning) |

### Troubleshooting

**"anthropic package not installed"**
- Install with: `pip3 install anthropic`

**"ANTHROPIC_API_KEY not set"**
- Set the environment variable before starting Claude Code
- The hook will allow commands if the key is missing (graceful degradation)

**API errors**
- The hook allows commands on API failure to avoid blocking legitimate work
- Check your API key and network connection

---

## Other Hooks

### `session-start.sh`

Runs when a Claude Code session starts. Displays the current backlog status.

### `check-active-task.sh`

Runs before file edits. Ensures a task is marked as active in the task management system before allowing code changes. This enforces the project's task tracking policy.

---

## Configuration

Hooks are configured in `.claude/settings.json`:

```json
{
  "hooks": {
    "SessionStart": [...],
    "PreToolUse": [
      {
        "matcher": "Bash",
        "hooks": [
          {
            "type": "command",
            "command": "python3 \"$CLAUDE_PROJECT_DIR/.claude/hooks/safety-check.py\"",
            "timeout": 30000
          }
        ]
      }
    ]
  }
}
```

### Matcher Patterns

- `"Bash"` - Matches only the Bash tool
- `"Edit|Write|MultiEdit"` - Matches multiple tools with `|` separator
- `""` (empty) - Matches all tools/events

### Timeout

The `timeout` field is in milliseconds. The safety check uses 30 seconds (30000ms) to allow for API calls.
