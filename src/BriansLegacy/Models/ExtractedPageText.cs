// ============================================================================
// File: Models/ExtractedPageText.cs
// Purpose: Per-page text extraction storage
// ============================================================================

namespace BriansLegacy.Models;

/// <summary>
/// Stores extracted text per page for better citations and debugging.
/// Large text content can optionally be stored as a file (TextPath).
/// </summary>
public class ExtractedPageText
{
    /// <summary>Primary key.</summary>
    public Guid Id { get; set; }

    /// <summary>Source file.</summary>
    public Guid LibraryFileId { get; set; }

    /// <summary>Page number (1-based).</summary>
    public int PageNumber { get; set; }

    /// <summary>Extracted text content (inline for small pages).</summary>
    public string? TextContent { get; set; }

    /// <summary>Path to text file (for large text content).</summary>
    public string? TextPath { get; set; }

    /// <summary>OCR confidence score (0-100).</summary>
    public int Confidence { get; set; }

    /// <summary>Method used to extract this text.</summary>
    public OcrMethod Method { get; set; }

    /// <summary>When the text was extracted.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public LibraryFile LibraryFile { get; set; } = null!;
}
