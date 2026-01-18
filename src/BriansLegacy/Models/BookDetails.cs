// ============================================================================
// File: Models/BookDetails.cs
// Purpose: Book-specific details (extends LibraryItem)
// ============================================================================

namespace BriansLegacy.Models;

/// <summary>
/// Physical book with cover photo.
/// Completeness: title + author + year + publisher + ISBN = 100%
/// </summary>
public class BookDetails : LibraryItem
{
    /// <summary>Book author(s).</summary>
    public string? Author { get; set; }

    /// <summary>Publisher name.</summary>
    public string? Publisher { get; set; }

    /// <summary>ISBN (10 or 13 digit).</summary>
    public string? ISBN { get; set; }

    /// <summary>Edition (e.g., "2nd", "Revised").</summary>
    public string? Edition { get; set; }

    /// <summary>Publication year.</summary>
    public int? Year { get; set; }

    /// <summary>Number of pages.</summary>
    public int? PageCount { get; set; }
}
