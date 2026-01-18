using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BriansLegacy.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ParentId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Categories_Categories_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LibraryItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ConfidenceScore = table.Column<int>(type: "int", nullable: false),
                    CompletenessScore = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Author = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Publisher = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ISBN = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Edition = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Year = table.Column<int>(type: "int", nullable: true),
                    BookDetails_PageCount = table.Column<int>(type: "int", nullable: true),
                    DocumentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DocumentNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Source = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PageCount = table.Column<int>(type: "int", nullable: true),
                    DrawingNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DrawingTitle = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ProjectName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Revision = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Scale = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SheetNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Discipline = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LibraryItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsAdmin = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LibraryFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LibraryItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OriginalPath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    FileType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    ContentHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LibraryFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LibraryFiles_LibraryItems_LibraryItemId",
                        column: x => x.LibraryItemId,
                        principalTable: "LibraryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LibraryItemCategories",
                columns: table => new
                {
                    LibraryItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LibraryItemCategories", x => new { x.LibraryItemId, x.CategoryId });
                    table.ForeignKey(
                        name: "FK_LibraryItemCategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LibraryItemCategories_LibraryItems_LibraryItemId",
                        column: x => x.LibraryItemId,
                        principalTable: "LibraryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProcessingLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LibraryItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProcessorName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ProcessorVersion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Step = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    InputHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    CostEstimate = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessingLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcessingLogs_LibraryItems_LibraryItemId",
                        column: x => x.LibraryItemId,
                        principalTable: "LibraryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProcessingStates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LibraryItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentStep = table.Column<int>(type: "int", nullable: false),
                    LastRunId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastError = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    NextRetryAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    LockedUntil = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessingStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcessingStates_LibraryItems_LibraryItemId",
                        column: x => x.LibraryItemId,
                        principalTable: "LibraryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReviewQueueItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LibraryItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AIExtractedData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FieldsNeedingReview = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReviewedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewQueueItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReviewQueueItems_LibraryItems_LibraryItemId",
                        column: x => x.LibraryItemId,
                        principalTable: "LibraryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LibraryItemTags",
                columns: table => new
                {
                    LibraryItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TagId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LibraryItemTags", x => new { x.LibraryItemId, x.TagId });
                    table.ForeignKey(
                        name: "FK_LibraryItemTags_LibraryItems_LibraryItemId",
                        column: x => x.LibraryItemId,
                        principalTable: "LibraryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LibraryItemTags_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExtractedPageTexts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LibraryFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PageNumber = table.Column<int>(type: "int", nullable: false),
                    TextContent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TextPath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Confidence = table.Column<int>(type: "int", nullable: false),
                    Method = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExtractedPageTexts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExtractedPageTexts_LibraryFiles_LibraryFileId",
                        column: x => x.LibraryFileId,
                        principalTable: "LibraryFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FileDerivatives",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LibraryFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DerivativeType = table.Column<int>(type: "int", nullable: false),
                    Path = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GeneratorVersion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    InputHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileDerivatives", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileDerivatives_LibraryFiles_LibraryFileId",
                        column: x => x.LibraryFileId,
                        principalTable: "LibraryFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ParentId_Name",
                table: "Categories",
                columns: new[] { "ParentId", "Name" },
                unique: true,
                filter: "[ParentId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ExtractedPageTexts_LibraryFileId_PageNumber",
                table: "ExtractedPageTexts",
                columns: new[] { "LibraryFileId", "PageNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_FileDerivatives_LibraryFileId_DerivativeType_GeneratorVersion_InputHash",
                table: "FileDerivatives",
                columns: new[] { "LibraryFileId", "DerivativeType", "GeneratorVersion", "InputHash" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LibraryFiles_ContentHash",
                table: "LibraryFiles",
                column: "ContentHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LibraryFiles_LibraryItemId",
                table: "LibraryFiles",
                column: "LibraryItemId");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryItemCategories_CategoryId",
                table: "LibraryItemCategories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryItems_CreatedAt",
                table: "LibraryItems",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryItems_Status",
                table: "LibraryItems",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryItems_Type",
                table: "LibraryItems",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryItemTags_TagId",
                table: "LibraryItemTags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessingLogs_LibraryItemId",
                table: "ProcessingLogs",
                column: "LibraryItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessingLogs_RunId",
                table: "ProcessingLogs",
                column: "RunId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessingLogs_StartedAt",
                table: "ProcessingLogs",
                column: "StartedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessingStates_LibraryItemId",
                table: "ProcessingStates",
                column: "LibraryItemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReviewQueueItems_CreatedAt",
                table: "ReviewQueueItems",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewQueueItems_LibraryItemId",
                table: "ReviewQueueItems",
                column: "LibraryItemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReviewQueueItems_ReviewedAt",
                table: "ReviewQueueItems",
                column: "ReviewedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Name",
                table: "Tags",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExtractedPageTexts");

            migrationBuilder.DropTable(
                name: "FileDerivatives");

            migrationBuilder.DropTable(
                name: "LibraryItemCategories");

            migrationBuilder.DropTable(
                name: "LibraryItemTags");

            migrationBuilder.DropTable(
                name: "ProcessingLogs");

            migrationBuilder.DropTable(
                name: "ProcessingStates");

            migrationBuilder.DropTable(
                name: "ReviewQueueItems");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "LibraryFiles");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "LibraryItems");
        }
    }
}
