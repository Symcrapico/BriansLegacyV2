// ============================================================================
// File: Models/LibraryItemType.cs
// Purpose: Enum for library item types (TPH discriminator)
// ============================================================================

namespace BriansLegacy.Models;

/// <summary>
/// Types of library items stored in the system.
/// Used as TPH discriminator for LibraryItem inheritance.
/// </summary>
public enum LibraryItemType
{
    /// <summary>Physical book with cover photo.</summary>
    Book = 1,

    /// <summary>PDF document (reports, manuals, etc.).</summary>
    Document = 2,

    /// <summary>Engineering drawing/plan with title block.</summary>
    Plan = 3
}
