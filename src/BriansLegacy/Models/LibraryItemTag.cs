// ============================================================================
// File: Models/LibraryItemTag.cs
// Purpose: Many-to-many junction table for LibraryItem-Tag
// ============================================================================

namespace BriansLegacy.Models;

/// <summary>
/// Junction table for many-to-many relationship between LibraryItem and Tag.
/// </summary>
public class LibraryItemTag
{
    /// <summary>Library item ID.</summary>
    public Guid LibraryItemId { get; set; }

    /// <summary>Tag ID.</summary>
    public int TagId { get; set; }

    // Navigation properties
    public LibraryItem LibraryItem { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}
