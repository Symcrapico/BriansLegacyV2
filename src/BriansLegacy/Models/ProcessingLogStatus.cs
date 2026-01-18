// ============================================================================
// File: Models/ProcessingLogStatus.cs
// Purpose: Enum for processing log entry status
// ============================================================================

namespace BriansLegacy.Models;

/// <summary>
/// Status of a processing log entry.
/// </summary>
public enum ProcessingLogStatus
{
    /// <summary>Processing started.</summary>
    Started = 1,

    /// <summary>Processing completed successfully.</summary>
    Completed = 2,

    /// <summary>Processing failed.</summary>
    Failed = 3,

    /// <summary>Processing was skipped (idempotent).</summary>
    Skipped = 4
}
