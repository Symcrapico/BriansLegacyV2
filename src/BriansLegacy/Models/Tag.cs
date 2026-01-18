// ============================================================================
// File: Models/Tag.cs
// Purpose: Flat tag for cross-cutting details
// ============================================================================

namespace BriansLegacy.Models;

/// <summary>
/// Flat tag for cross-cutting details (years, companies, bridge names).
/// Unique constraint on Name (case-insensitive).
/// </summary>
public class Tag
{
    /// <summary>Primary key.</summary>
    public int Id { get; set; }

    /// <summary>Tag name (unique, case-insensitive).</summary>
    public string Name { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<LibraryItemTag> LibraryItems { get; set; } = new List<LibraryItemTag>();
}
