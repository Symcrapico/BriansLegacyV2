// ============================================================================
// File: Models/LibraryItemStatus.cs
// Purpose: Enum for library item processing status
// ============================================================================

namespace BriansLegacy.Models;

/// <summary>
/// Processing status of a library item.
/// </summary>
public enum LibraryItemStatus
{
    /// <summary>Item uploaded but not yet processed.</summary>
    Pending = 1,

    /// <summary>Item currently being processed by background jobs.</summary>
    Processing = 2,

    /// <summary>Item needs human review (low confidence or completeness).</summary>
    Review = 3,

    /// <summary>Item published and visible to all users.</summary>
    Published = 4,

    /// <summary>Processing failed and requires manual intervention.</summary>
    Failed = 5
}
