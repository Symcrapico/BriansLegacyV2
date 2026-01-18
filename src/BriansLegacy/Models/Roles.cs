// ============================================================================
// File: Models/Roles.cs
// Purpose: Application role constants for authorization
// ============================================================================

namespace BriansLegacy.Models;

/// <summary>
/// Role constants for Brian's Legacy application.
/// Used as claim values for role-based authorization.
/// </summary>
public static class Roles
{
    /// <summary>
    /// Administrator role - full access including upload, edit, review, and user management.
    /// </summary>
    public const string Admin = "Admin";

    /// <summary>
    /// Viewer role - read-only access to browse, search, and download.
    /// </summary>
    public const string Viewer = "Viewer";
}
