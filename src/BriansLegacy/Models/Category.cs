// ============================================================================
// File: Models/Category.cs
// Purpose: Hierarchical category taxonomy
// ============================================================================

namespace BriansLegacy.Models;

/// <summary>
/// Hierarchical category for organizing library items.
/// Unique constraint on (ParentId, Name) prevents duplicate categories.
/// </summary>
public class Category
{
    /// <summary>Primary key.</summary>
    public int Id { get; set; }

    /// <summary>Category name (unique within parent).</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Parent category ID (null for root categories).</summary>
    public int? ParentId { get; set; }

    // Navigation properties
    public Category? Parent { get; set; }
    public ICollection<Category> Children { get; set; } = new List<Category>();
    public ICollection<LibraryItemCategory> LibraryItems { get; set; } = new List<LibraryItemCategory>();
}
