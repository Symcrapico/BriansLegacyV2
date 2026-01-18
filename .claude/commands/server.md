---
description: "Manage development server (start/stop/restart/status/logs)"
argument-hint: "<command> [args]"
allowed-tools: ["Bash"]
---

# Server Management

Run the appropriate server script based on the current operating system.

## Cross-Platform Execution

**On Windows:**
```powershell
powershell -ExecutionPolicy Bypass -File scripts/server.ps1 $ARGUMENTS
```

**On macOS/Linux:**
```bash
bash scripts/server.sh $ARGUMENTS
```

## Available Commands

The script is self-sufficient and handles:
- **restart**: Stop, increment DEV tag, clean build, start, verify health
- **start**: Increment DEV tag, build, start, verify health
- **stop**: Kill process on port 5001
- **status**: Show running state, health, DEV tag
- **logs**: Show server log (add `errors` for filtered view)

## Log File Location

- **Windows**: `%TEMP%\server.log`
- **macOS/Linux**: `/tmp/server.log`

If the script fails, review the error output and check the log file for details.
