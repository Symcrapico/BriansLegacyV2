# Server & Deployment Documentation

---

## Server Management

**Use `/server` for all server management.** This runs a self-sufficient shell script that requires no AI interpretation.

### Commands

```
/server restart   # Stop, increment DEV tag, clean build, start, verify health
/server start     # Increment DEV tag, build, start, verify health
/server stop      # Kill process on port 5001
/server status    # Show running state, health, DEV tag
/server logs      # Show last 50 lines of server log
/server logs errors  # Show only error lines
```

### How It Works

The script (`scripts/server.sh`) handles everything automatically:
1. Kills any process on port 5001 (by port, not name)
2. Increments DEV tag in both layout files
3. Runs clean build
4. Starts server
5. Waits for health check to pass
6. Verifies correct DEV tag is being served

### DEV Tag

The script automatically increments the DEV tag (displayed bottom-right of every page). User visually confirms the tag to verify fresh code is running.

### When to Use AI

Only if the script reports an error. The error output includes:
- Which step failed
- Last lines of `/tmp/server.log`
- Suggested next action

For complex debugging, use `Task: "@server diagnose"` to have an agent investigate.

---

## Browser Management

**Always use the `@browser` subagent** for browser automation. Never use Playwright MCP tools directly.

```
Task: "@browser open https://localhost:5001"
Task: "@browser close"
Task: "@browser click 'Submit'"
```

See `.claude/docs/subagents.md` for full `@browser` documentation.

### Common Issues

- **"Address already in use"**: Server still running, run `/server stop` first
- **"Browser already in use"**: Use `@browser close` first

---

## Deploy Policy

**Use `/deploy` to deploy to production (www.securail.ca).**

### The Command

```
/deploy           # Build and deploy to production
/deploy --check   # Just verify the site is healthy
```

### What It Does

1. Builds the project in Release configuration
2. Deploys via Web Deploy to securail.ca
3. Verifies health check passes

### Manual Deployment

If needed, run directly:

```powershell
cd 'C:\Users\GK\Desktop\Code\FRMv2'
dotnet publish testCore.csproj -c Release '-p:PublishProfile=Properties\PublishProfiles\securail.ca.pubxml' '-p:Password=y5Uq0pG7Mr2#4SaM'
```

### Production Details

| Setting | Value |
|---------|-------|
| **URL** | https://www.securail.ca |
| **IIS Site** | securail.ca |
| **Hosting** | kweb-host4.com (Plesk) |
| **Deploy Method** | Web Deploy (WMSVC port 8172) |
| **Self-Contained** | Yes (includes .NET 9.0 runtime) |

### Key Files

- `Properties/PublishProfiles/securail.ca.pubxml` - Publish profile
- `appsettings.Production.json` - Production overrides (Stripe security, logging)
- `web.config` - IIS configuration
- `scripts/deploy.ps1` - PowerShell deployment script

### Troubleshooting

If deployment fails:
1. Check Plesk logs at https://kweb-host4.com:8443
2. Look for `logs/stdout*.log` in site directory
3. Common issues:
   - `AllowUnsignedWebhooks` must be `false` in production
   - Database: www.kweb-host4.com with valsc_frm credentials
