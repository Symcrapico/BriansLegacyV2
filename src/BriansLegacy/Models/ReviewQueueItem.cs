// ============================================================================
// File: Models/ReviewQueueItem.cs
// Purpose: Items needing human review
// ============================================================================

namespace BriansLegacy.Models;

/// <summary>
/// Queue entry for items requiring human review.
/// Items enter the queue when confidence or completeness is low.
/// </summary>
public class ReviewQueueItem
{
    /// <summary>Primary key.</summary>
    public Guid Id { get; set; }

    /// <summary>Library item requiring review.</summary>
    public Guid LibraryItemId { get; set; }

    /// <summary>AI-extracted metadata as JSON.</summary>
    public string? AIExtractedData { get; set; }

    /// <summary>List of fields needing review as JSON.</summary>
    public string? FieldsNeedingReview { get; set; }

    /// <summary>User ID who reviewed this item.</summary>
    public string? ReviewedBy { get; set; }

    /// <summary>When the item was reviewed.</summary>
    public DateTime? ReviewedAt { get; set; }

    /// <summary>Reviewer notes.</summary>
    public string? Notes { get; set; }

    /// <summary>When the item was added to the queue.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public LibraryItem LibraryItem { get; set; } = null!;
}
