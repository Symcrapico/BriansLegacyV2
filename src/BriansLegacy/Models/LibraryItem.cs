// ============================================================================
// File: Models/LibraryItem.cs
// Purpose: Base entity for all library items (TPH inheritance)
// ============================================================================

namespace BriansLegacy.Models;

/// <summary>
/// Base entity for library items using Table-Per-Hierarchy (TPH) inheritance.
/// Concrete types: BookDetails, DocumentDetails, PlanDetails.
/// </summary>
public abstract class LibraryItem
{
    /// <summary>Primary key.</summary>
    public Guid Id { get; set; }

    /// <summary>Item type discriminator for TPH.</summary>
    public LibraryItemType Type { get; set; }

    /// <summary>Title of the item.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Optional description or notes.</summary>
    public string? Description { get; set; }

    /// <summary>Current processing status.</summary>
    public LibraryItemStatus Status { get; set; } = LibraryItemStatus.Pending;

    /// <summary>AI's certainty about extracted metadata (0-100).</summary>
    public int ConfidenceScore { get; set; }

    /// <summary>Field presence/completeness score (0-100).</summary>
    public int CompletenessScore { get; set; }

    /// <summary>When the item was created.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>When the item was last updated.</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>User ID who created this item.</summary>
    public string? CreatedBy { get; set; }

    // Navigation properties
    public ICollection<LibraryFile> Files { get; set; } = new List<LibraryFile>();
    public ICollection<LibraryItemCategory> Categories { get; set; } = new List<LibraryItemCategory>();
    public ICollection<LibraryItemTag> Tags { get; set; } = new List<LibraryItemTag>();
    public ProcessingState? ProcessingState { get; set; }
    public ICollection<ProcessingLog> ProcessingLogs { get; set; } = new List<ProcessingLog>();
    public ReviewQueueItem? ReviewQueueItem { get; set; }
}
