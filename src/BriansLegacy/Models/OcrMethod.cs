// ============================================================================
// File: Models/OcrMethod.cs
// Purpose: Enum for OCR extraction methods
// ============================================================================

namespace BriansLegacy.Models;

/// <summary>
/// Method used to extract text from a page.
/// </summary>
public enum OcrMethod
{
    /// <summary>Text extracted from native PDF text layer.</summary>
    Native = 1,

    /// <summary>Text extracted using local Tesseract OCR.</summary>
    Tesseract = 2,

    /// <summary>Text extracted using cloud AI (Gemini Vision).</summary>
    CloudAI = 3
}
