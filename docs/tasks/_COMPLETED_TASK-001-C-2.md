# TASK-001-C-2: Set up ASP.NET Core Identity with Admin/Viewer roles

**Status:** 🟨
**Started:** 2026-01-18T23:43:55.823Z
**Feature:** FEAT-001-C
**Epic:** EPIC-001
**Estimate:** Not set

## Notes

### Implementation Summary

Changed from ASP.NET Core Identity to **Google OAuth** (matching VAL_TODO project pattern):

**Files Created:**
- `Models/ApplicationUser.cs` - Simple user entity with Email, DisplayName, IsAdmin, timestamps
- `Models/Roles.cs` - Role constants (Admin, Viewer)
- `Infrastructure/HangfireAuthorizationFilter.cs` - Admin-only access to Hangfire dashboard

**Files Modified:**
- `BriansLegacy.csproj` - Added Microsoft.AspNetCore.Authentication.Google 9.0.0
- `Data/ApplicationDbContext.cs` - Simple DbContext with Users DbSet
- `Program.cs` - Google OAuth with email whitelist + role assignment
- `appsettings.json` / `appsettings.Development.json` - Configuration placeholders

**Configuration Required:**
```json
{
  "Authentication": {
    "Google": {
      "ClientId": "...",
      "ClientSecret": "..."
    }
  },
  "AllowedEmails": ["user1@example.com", "user2@example.com"],
  "AdminEmails": ["admin@example.com"]
}
```

**Setup:**
1. Create Google OAuth credentials in Google Cloud Console
2. Store ClientId/ClientSecret in user secrets:
   ```bash
   dotnet user-secrets set "Authentication:Google:ClientId" "your-client-id"
   dotnet user-secrets set "Authentication:Google:ClientSecret" "your-client-secret"
   ```
3. Add authorized emails to AllowedEmails array
4. Add admin emails to AdminEmails array

**Endpoints:**
- `/login` - Initiates Google OAuth flow
- `/logout` - Signs out and redirects to home

**Session:**
- 30 min sliding expiration (aligned with FRMv2)

**Migration:**
- `20260118235550_InitialCreate` - Fresh migration with simplified Users table

## Acceptance Criteria

- [x] Implementation complete
- [ ] Tests passing
- [ ] Code reviewed
