// ============================================================================
// File: Models/DerivativeType.cs
// Purpose: Enum for file derivative types
// ============================================================================

namespace BriansLegacy.Models;

/// <summary>
/// Types of file derivatives generated from original files.
/// </summary>
public enum DerivativeType
{
    /// <summary>Thumbnail image for preview.</summary>
    Thumbnail = 1,

    /// <summary>Rasterized page image (for OCR).</summary>
    PageImage = 2,

    /// <summary>Extracted text file.</summary>
    ExtractedText = 3,

    /// <summary>Cropped title block from plan.</summary>
    TitleBlockCrop = 4
}
