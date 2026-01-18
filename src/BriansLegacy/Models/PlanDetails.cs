// ============================================================================
// File: Models/PlanDetails.cs
// Purpose: Plan/drawing-specific details (extends LibraryItem)
// ============================================================================

namespace BriansLegacy.Models;

/// <summary>
/// Engineering drawing/plan with title block.
/// Completeness: drawingNumber + revision + title + date = 100%
/// </summary>
public class PlanDetails : LibraryItem
{
    /// <summary>Drawing number from title block.</summary>
    public string? DrawingNumber { get; set; }

    /// <summary>Drawing title from title block.</summary>
    public string? DrawingTitle { get; set; }

    /// <summary>Project name.</summary>
    public string? ProjectName { get; set; }

    /// <summary>Revision number/letter.</summary>
    public string? Revision { get; set; }

    /// <summary>Drawing scale (e.g., "1:100", "NTS").</summary>
    public string? Scale { get; set; }

    /// <summary>Sheet number (e.g., "1 of 5").</summary>
    public string? SheetNumber { get; set; }

    /// <summary>Engineering discipline (e.g., "Structural", "Civil").</summary>
    public string? Discipline { get; set; }

    /// <summary>Drawing date.</summary>
    public DateTime? Date { get; set; }
}
