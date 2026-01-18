// ============================================================================
// File: Models/ProcessingState.cs
// Purpose: Current processing state per library item
// ============================================================================

namespace BriansLegacy.Models;

/// <summary>
/// Tracks the current processing state of a library item.
/// Unique constraint on LibraryItemId ensures one state per item.
/// </summary>
public class ProcessingState
{
    /// <summary>Primary key.</summary>
    public Guid Id { get; set; }

    /// <summary>Library item being processed (unique).</summary>
    public Guid LibraryItemId { get; set; }

    /// <summary>Current step in the processing pipeline.</summary>
    public ProcessingStep CurrentStep { get; set; }

    /// <summary>Unique ID for the current processing run.</summary>
    public Guid? LastRunId { get; set; }

    /// <summary>Last error message if processing failed.</summary>
    public string? LastError { get; set; }

    /// <summary>When to next retry (for failed items).</summary>
    public DateTime? NextRetryAt { get; set; }

    /// <summary>Number of retry attempts.</summary>
    public int RetryCount { get; set; }

    /// <summary>Lock expiration time (prevents concurrent processing).</summary>
    public DateTime? LockedUntil { get; set; }

    // Navigation properties
    public LibraryItem LibraryItem { get; set; } = null!;
}
