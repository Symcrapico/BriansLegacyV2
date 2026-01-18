// ============================================================================
// File: Data/ApplicationDbContext.cs
// Purpose: Entity Framework Core DbContext for SQL Server
// ============================================================================

using BriansLegacy.Models;
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

    // Library Items (TPH inheritance)
    public DbSet<LibraryItem> LibraryItems => Set<LibraryItem>();
    public DbSet<BookDetails> Books => Set<BookDetails>();
    public DbSet<DocumentDetails> Documents => Set<DocumentDetails>();
    public DbSet<PlanDetails> Plans => Set<PlanDetails>();

    // Files
    public DbSet<LibraryFile> LibraryFiles => Set<LibraryFile>();
    public DbSet<FileDerivative> FileDerivatives => Set<FileDerivative>();
    public DbSet<ExtractedPageText> ExtractedPageTexts => Set<ExtractedPageText>();

    // Categories & Tags
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<LibraryItemCategory> LibraryItemCategories => Set<LibraryItemCategory>();
    public DbSet<LibraryItemTag> LibraryItemTags => Set<LibraryItemTag>();

    // Processing
    public DbSet<ProcessingState> ProcessingStates => Set<ProcessingState>();
    public DbSet<ProcessingLog> ProcessingLogs => Set<ProcessingLog>();

    // Review
    public DbSet<ReviewQueueItem> ReviewQueueItems => Set<ReviewQueueItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // =====================================================================
        // LibraryItem - TPH Inheritance
        // =====================================================================
        modelBuilder.Entity<LibraryItem>(entity =>
        {
            entity.ToTable("LibraryItems");
            entity.HasKey(e => e.Id);

            // TPH discriminator
            entity.HasDiscriminator(e => e.Type)
                .HasValue<BookDetails>(LibraryItemType.Book)
                .HasValue<DocumentDetails>(LibraryItemType.Document)
                .HasValue<PlanDetails>(LibraryItemType.Plan);

            entity.Property(e => e.Title).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(4000);
            entity.Property(e => e.CreatedBy).HasMaxLength(450);

            // Indexes
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.CreatedAt);
        });

        // BookDetails - specific properties
        modelBuilder.Entity<BookDetails>(entity =>
        {
            entity.Property(e => e.Author).HasMaxLength(500);
            entity.Property(e => e.Publisher).HasMaxLength(500);
            entity.Property(e => e.ISBN).HasMaxLength(20);
            entity.Property(e => e.Edition).HasMaxLength(100);
        });

        // DocumentDetails - specific properties
        modelBuilder.Entity<DocumentDetails>(entity =>
        {
            entity.Property(e => e.DocumentNumber).HasMaxLength(100);
            entity.Property(e => e.Source).HasMaxLength(500);
        });

        // PlanDetails - specific properties
        modelBuilder.Entity<PlanDetails>(entity =>
        {
            entity.Property(e => e.DrawingNumber).HasMaxLength(100);
            entity.Property(e => e.DrawingTitle).HasMaxLength(500);
            entity.Property(e => e.ProjectName).HasMaxLength(500);
            entity.Property(e => e.Revision).HasMaxLength(50);
            entity.Property(e => e.Scale).HasMaxLength(50);
            entity.Property(e => e.SheetNumber).HasMaxLength(50);
            entity.Property(e => e.Discipline).HasMaxLength(100);
        });

        // =====================================================================
        // LibraryFile
        // =====================================================================
        modelBuilder.Entity<LibraryFile>(entity =>
        {
            entity.ToTable("LibraryFiles");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.OriginalPath).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.FileType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ContentHash).HasMaxLength(64).IsRequired();

            // UNIQUE: ContentHash for duplicate detection
            entity.HasIndex(e => e.ContentHash).IsUnique();

            entity.HasOne(e => e.LibraryItem)
                .WithMany(e => e.Files)
                .HasForeignKey(e => e.LibraryItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // =====================================================================
        // FileDerivative
        // =====================================================================
        modelBuilder.Entity<FileDerivative>(entity =>
        {
            entity.ToTable("FileDerivatives");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Path).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.GeneratorVersion).HasMaxLength(50).IsRequired();
            entity.Property(e => e.InputHash).HasMaxLength(64).IsRequired();

            // UNIQUE: (LibraryFileId, DerivativeType, GeneratorVersion, InputHash)
            entity.HasIndex(e => new { e.LibraryFileId, e.DerivativeType, e.GeneratorVersion, e.InputHash })
                .IsUnique();

            entity.HasOne(e => e.LibraryFile)
                .WithMany(e => e.Derivatives)
                .HasForeignKey(e => e.LibraryFileId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // =====================================================================
        // ExtractedPageText
        // =====================================================================
        modelBuilder.Entity<ExtractedPageText>(entity =>
        {
            entity.ToTable("ExtractedPageTexts");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.TextPath).HasMaxLength(1000);

            // Index for page lookup
            entity.HasIndex(e => new { e.LibraryFileId, e.PageNumber });

            entity.HasOne(e => e.LibraryFile)
                .WithMany(e => e.ExtractedPages)
                .HasForeignKey(e => e.LibraryFileId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // =====================================================================
        // Category
        // =====================================================================
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Categories");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();

            // UNIQUE: (ParentId, Name) - case-insensitive
            entity.HasIndex(e => new { e.ParentId, e.Name }).IsUnique();

            entity.HasOne(e => e.Parent)
                .WithMany(e => e.Children)
                .HasForeignKey(e => e.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // =====================================================================
        // Tag
        // =====================================================================
        modelBuilder.Entity<Tag>(entity =>
        {
            entity.ToTable("Tags");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();

            // UNIQUE: Name (case-insensitive)
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // =====================================================================
        // LibraryItemCategory (Junction)
        // =====================================================================
        modelBuilder.Entity<LibraryItemCategory>(entity =>
        {
            entity.ToTable("LibraryItemCategories");
            entity.HasKey(e => new { e.LibraryItemId, e.CategoryId });

            entity.HasOne(e => e.LibraryItem)
                .WithMany(e => e.Categories)
                .HasForeignKey(e => e.LibraryItemId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Category)
                .WithMany(e => e.LibraryItems)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // =====================================================================
        // LibraryItemTag (Junction)
        // =====================================================================
        modelBuilder.Entity<LibraryItemTag>(entity =>
        {
            entity.ToTable("LibraryItemTags");
            entity.HasKey(e => new { e.LibraryItemId, e.TagId });

            entity.HasOne(e => e.LibraryItem)
                .WithMany(e => e.Tags)
                .HasForeignKey(e => e.LibraryItemId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Tag)
                .WithMany(e => e.LibraryItems)
                .HasForeignKey(e => e.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // =====================================================================
        // ProcessingState
        // =====================================================================
        modelBuilder.Entity<ProcessingState>(entity =>
        {
            entity.ToTable("ProcessingStates");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.LastError).HasMaxLength(4000);

            // UNIQUE: LibraryItemId (one state per item)
            entity.HasIndex(e => e.LibraryItemId).IsUnique();

            entity.HasOne(e => e.LibraryItem)
                .WithOne(e => e.ProcessingState)
                .HasForeignKey<ProcessingState>(e => e.LibraryItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // =====================================================================
        // ProcessingLog
        // =====================================================================
        modelBuilder.Entity<ProcessingLog>(entity =>
        {
            entity.ToTable("ProcessingLogs");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.ProcessorName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.ProcessorVersion).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ErrorMessage).HasMaxLength(4000);
            entity.Property(e => e.InputHash).HasMaxLength(64);
            entity.Property(e => e.CostEstimate).HasPrecision(18, 6);

            // Indexes for querying
            entity.HasIndex(e => e.LibraryItemId);
            entity.HasIndex(e => e.RunId);
            entity.HasIndex(e => e.StartedAt);

            entity.HasOne(e => e.LibraryItem)
                .WithMany(e => e.ProcessingLogs)
                .HasForeignKey(e => e.LibraryItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // =====================================================================
        // ReviewQueueItem
        // =====================================================================
        modelBuilder.Entity<ReviewQueueItem>(entity =>
        {
            entity.ToTable("ReviewQueueItems");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.ReviewedBy).HasMaxLength(450);
            entity.Property(e => e.Notes).HasMaxLength(4000);

            // Index for queue queries
            entity.HasIndex(e => e.ReviewedAt);
            entity.HasIndex(e => e.CreatedAt);

            entity.HasOne(e => e.LibraryItem)
                .WithOne(e => e.ReviewQueueItem)
                .HasForeignKey<ReviewQueueItem>(e => e.LibraryItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
