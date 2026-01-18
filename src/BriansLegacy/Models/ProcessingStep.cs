// ============================================================================
// File: Models/ProcessingStep.cs
// Purpose: Enum for processing pipeline steps
// ============================================================================

namespace BriansLegacy.Models;

/// <summary>
/// Steps in the processing pipeline for library items.
/// </summary>
public enum ProcessingStep
{
    /// <summary>Extract text from native PDF layer.</summary>
    ExtractText = 1,

    /// <summary>Run local Tesseract OCR.</summary>
    OcrLocal = 2,

    /// <summary>Run cloud AI OCR (escalation).</summary>
    OcrCloud = 3,

    /// <summary>Chunk text for embedding.</summary>
    Chunk = 4,

    /// <summary>Generate embeddings.</summary>
    Embed = 5,

    /// <summary>Auto-categorize using AI.</summary>
    Categorize = 6,

    /// <summary>Processing complete.</summary>
    Complete = 7,

    /// <summary>Processing failed.</summary>
    Failed = 8
}
