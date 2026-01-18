// ============================================================================
// File: Infrastructure/HangfireAuthorizationFilter.cs
// Purpose: Authorization filter for Hangfire dashboard (Admin only)
// ============================================================================

using BriansLegacy.Models;
using Hangfire.Dashboard;

namespace BriansLegacy.Infrastructure;

/// <summary>
/// Authorization filter for Hangfire dashboard.
/// Restricts access to Admin role only.
/// </summary>
public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        // User must be authenticated and in Admin role
        return httpContext.User.Identity?.IsAuthenticated == true
            && httpContext.User.IsInRole(Roles.Admin);
    }
}
