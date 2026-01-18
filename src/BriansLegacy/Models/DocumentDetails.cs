// ============================================================================
// File: Models/DocumentDetails.cs
// Purpose: Document-specific details (extends LibraryItem)
// ============================================================================

namespace BriansLegacy.Models;

/// <summary>
/// PDF document (reports, manuals, etc.).
/// Completeness: title + date + source + docNumber = 100%
/// </summary>
public class DocumentDetails : LibraryItem
{
    /// <summary>Document date.</summary>
    public DateTime? DocumentDate { get; set; }

    /// <summary>Document number/identifier.</summary>
    public string? DocumentNumber { get; set; }

    /// <summary>Source organization or author.</summary>
    public string? Source { get; set; }

    /// <summary>Number of pages.</summary>
    public int? PageCount { get; set; }
}
