// ============================================================================
// File: Models/ProcessingLog.cs
// Purpose: Processing history for library items
// ============================================================================

namespace BriansLegacy.Models;

/// <summary>
/// Historical log of processing operations on library items.
/// Useful for debugging and auditing.
/// </summary>
public class ProcessingLog
{
    /// <summary>Primary key.</summary>
    public Guid Id { get; set; }

    /// <summary>Library item that was processed.</summary>
    public Guid LibraryItemId { get; set; }

    /// <summary>Unique ID for this processing run.</summary>
    public Guid RunId { get; set; }

    /// <summary>When processing started.</summary>
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    /// <summary>When processing completed (or failed).</summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>Name of the processor/job that ran.</summary>
    public string ProcessorName { get; set; } = string.Empty;

    /// <summary>Version of the processor.</summary>
    public string ProcessorVersion { get; set; } = string.Empty;

    /// <summary>Processing step that was executed.</summary>
    public ProcessingStep Step { get; set; }

    /// <summary>Status of this processing run.</summary>
    public ProcessingLogStatus Status { get; set; }

    /// <summary>Error message if processing failed.</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Hash of the input (for idempotency checks).</summary>
    public string? InputHash { get; set; }

    /// <summary>Estimated cost of this processing run (API costs).</summary>
    public decimal? CostEstimate { get; set; }

    /// <summary>Which retry attempt this was.</summary>
    public int RetryCount { get; set; }

    // Navigation properties
    public LibraryItem LibraryItem { get; set; } = null!;
}
