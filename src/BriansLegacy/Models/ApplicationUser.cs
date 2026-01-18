// ============================================================================
// File: Models/ApplicationUser.cs
// Purpose: User entity for Google OAuth authenticated users
// ============================================================================

namespace BriansLegacy.Models;

/// <summary>
/// Application user authenticated via Google OAuth.
/// Users are authorized via email whitelist in configuration.
/// </summary>
public class ApplicationUser
{
    /// <summary>
    /// Primary key.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// User's email from Google (unique identifier).
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// User's display name from Google.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Whether this user has admin privileges.
    /// </summary>
    public bool IsAdmin { get; set; }

    /// <summary>
    /// When the user record was created (first login).
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the user last logged in.
    /// </summary>
    public DateTime? LastLoginAt { get; set; }
}
