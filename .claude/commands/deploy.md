---
description: "Deploy to production (www.securail.ca) via Web Deploy"
argument-hint: "[--check]"
---

# Deploy to Production

Deploy the SecuRail FRM application to www.securail.ca via Web Deploy.

**User input:** $ARGUMENTS

## Commands

| Command | Action |
|---------|--------|
| `/deploy` | Build and deploy to production |
| `/deploy --check` | Just verify the site is healthy |

## Deployment Process

If `--check` is passed, just run the health check:

```bash
curl -s -k https://www.securail.ca/health
```

Otherwise, run the full deployment:

### Step 1: Build and Deploy

Run the deployment using PowerShell (required for proper escaping):

```powershell
cd 'C:\Users\GK\Desktop\Code\FRMv2'
dotnet publish testCore.csproj -c Release '-p:PublishProfile=Properties\PublishProfiles\securail.ca.pubxml' '-p:Password=y5Uq0pG7Mr2#4SaM'
```

### Step 2: Verify Deployment

Wait 3-5 seconds for IIS to recycle, then check health:

```bash
curl -s -k https://www.securail.ca/health
```

Expected response: `Healthy`

### Step 3: Report Results

- If successful: Report "Deployment successful - site is healthy"
- If health check fails: Report the error and suggest checking Plesk logs

## Important Notes

- Deployment is self-contained (includes .NET 9.0 runtime)
- Uses Web Deploy (MSDeploy) via WMSVC on port 8172
- Site: securail.ca on kweb-host4.com hosting
- Production config: appsettings.Production.json overrides dev settings

## Troubleshooting

If deployment fails:
1. Check Plesk control panel logs at https://kweb-host4.com:8443
2. Verify `logs/stdout*.log` in site directory for startup errors
3. Common issues:
   - Stripe AllowUnsignedWebhooks must be false in production
   - Database connection uses www.kweb-host4.com
