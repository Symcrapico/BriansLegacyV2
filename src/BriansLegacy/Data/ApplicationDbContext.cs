// ============================================================================
// File: Data/ApplicationDbContext.cs
// Purpose: Entity Framework Core DbContext for SQL Server
// ============================================================================

using Microsoft.EntityFrameworkCore;

namespace BriansLegacy.Data;

/// <summary>
/// Main application database context for SQL Server.
/// Contains metadata, users, review queue, and job state.
/// Vector embeddings are stored separately in PostgreSQL.
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Entity DbSets will be added in TASK-001-B-2
    // - LibraryItem (base), BookDetails, DocumentDetails, PlanDetails
    // - LibraryFile, FileDerivative, ExtractedPageText
    // - Category, Tag, LibraryItemCategory, LibraryItemTag
    // - ProcessingState, ProcessingLog
    // - ReviewQueueItem

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Entity configurations will be added in TASK-001-B-2
        // Including:
        // - TPH inheritance for LibraryItem
        // - Uniqueness constraints (ContentHash, composite keys)
        // - Case-insensitive indexes for Category/Tag names
    }
}
