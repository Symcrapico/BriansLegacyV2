// ============================================================================
// File: Models/FileDerivative.cs
// Purpose: Processed file outputs (thumbnails, crops, etc.)
// ============================================================================

namespace BriansLegacy.Models;

/// <summary>
/// Tracks processed outputs from original files.
/// Unique constraint on (LibraryFileId, DerivativeType, GeneratorVersion, InputHash)
/// enables safe reprocessing.
/// </summary>
public class FileDerivative
{
    /// <summary>Primary key.</summary>
    public Guid Id { get; set; }

    /// <summary>Source file.</summary>
    public Guid LibraryFileId { get; set; }

    /// <summary>Type of derivative.</summary>
    public DerivativeType DerivativeType { get; set; }

    /// <summary>Path to the derivative file in storage.</summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>When this derivative was generated.</summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Version of the generator that created this.</summary>
    public string GeneratorVersion { get; set; } = string.Empty;

    /// <summary>Hash of the input used (for idempotency).</summary>
    public string InputHash { get; set; } = string.Empty;

    // Navigation properties
    public LibraryFile LibraryFile { get; set; } = null!;
}
