// ============================================================================
// File: Models/LibraryFile.cs
// Purpose: Original file storage tracking
// ============================================================================

namespace BriansLegacy.Models;

/// <summary>
/// Represents an original file uploaded to the library.
/// ContentHash used for duplicate detection (unique index).
/// </summary>
public class LibraryFile
{
    /// <summary>Primary key.</summary>
    public Guid Id { get; set; }

    /// <summary>Parent library item.</summary>
    public Guid LibraryItemId { get; set; }

    /// <summary>Path to the original file in storage.</summary>
    public string OriginalPath { get; set; } = string.Empty;

    /// <summary>File type/extension (e.g., "pdf", "jpg").</summary>
    public string FileType { get; set; } = string.Empty;

    /// <summary>File size in bytes.</summary>
    public long SizeBytes { get; set; }

    /// <summary>SHA256 hash for duplicate detection (unique index).</summary>
    public string ContentHash { get; set; } = string.Empty;

    /// <summary>When the file was uploaded.</summary>
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public LibraryItem LibraryItem { get; set; } = null!;
    public ICollection<FileDerivative> Derivatives { get; set; } = new List<FileDerivative>();
    public ICollection<ExtractedPageText> ExtractedPages { get; set; } = new List<ExtractedPageText>();
}
