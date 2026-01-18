// ============================================================================
// File: Models/LibraryItemCategory.cs
// Purpose: Many-to-many junction table for LibraryItem-Category
// ============================================================================

namespace BriansLegacy.Models;

/// <summary>
/// Junction table for many-to-many relationship between LibraryItem and Category.
/// </summary>
public class LibraryItemCategory
{
    /// <summary>Library item ID.</summary>
    public Guid LibraryItemId { get; set; }

    /// <summary>Category ID.</summary>
    public int CategoryId { get; set; }

    // Navigation properties
    public LibraryItem LibraryItem { get; set; } = null!;
    public Category Category { get; set; } = null!;
}
