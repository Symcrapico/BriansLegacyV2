# BriansLegacyV2 - Private Railway Engineering Library

## Overview

A private online library for railway structural engineering materials inherited from a retired senior engineer. Enables a team of engineers to search, browse, and ask questions about books, documents, and plans using AI-powered cataloging and semantic search.

## Key Requirements

- **Collection**: ~100-500 books (cover photos) + ~500-2000 documents/plans (PDFs)
- **Quality**: Mixed - some handwritten, old scans (60-70% process cleanly)
- **Access**: Role-based (Admin: add/edit/review, Viewer: browse/search)
- **Team Size**: Small (2-5 engineers)
- **AI**: Cloud APIs (Claude + Gemini) - acceptable for all documents
- **Language**: English only (simpler than FRMv2's bilingual setup)
- **Hosting**: Same IIS server as FRMv2 (securail.ca)
- **Tech Stack**: Match FRMv2 (ASP.NET Core 9.0, Razor Pages, SQL Server, EF Core, Tailwind CSS)

## Architecture

```
Users (Engineers / Admins)
        │
        ▼
ASP.NET Core 9.0 Razor Pages
(Browse, Search, Q&A, Admin Panel)
        │
        ▼
Service Layer
├── AI Service (Claude + Gemini abstraction)
├── Search Service (hybrid keyword + semantic)
├── Document Processor (OCR + chunking)
└── Background Job Service (Hangfire)
        │
        ├──► SQL Server (metadata, users, review queue, job state)
        ├──► PostgreSQL + pgvector (embeddings, chunks)
        └──► Local Filesystem (originals, derivatives, thumbnails)
```

### Key Architectural Decisions

1. **Background Jobs from Day 1**: All AI processing runs via Hangfire, never in web requests
2. **Layered OCR Strategy**: Embedded text → Tesseract (local) → Cloud AI (escalation)
3. **File Derivatives**: Track all processed outputs separately from originals
4. **Embedding Versioning**: Store model/params to enable safe re-embedding
5. **Idempotent Processing**: Jobs safe to re-run without creating duplicates
6. **Page-Level Text Storage**: Per-page extraction for better citations and debugging

## Data Model

### SQL Server (EF Core)

**LibraryItem** (base entity with TPH inheritance)
- Id (GUID), Type (Book/Document/Plan), Title, Description
- Status (Pending/Processing/Review/Published/Failed)
- ConfidenceScore (0-100) - AI's certainty
- CompletenessScore (0-100) - Field presence score
- CreatedAt, UpdatedAt, CreatedBy

**BookDetails** (extends LibraryItem)
- Author, Publisher, ISBN, Edition, Year, PageCount
- Completeness: title+author+year+publisher+ISBN = 100%

**DocumentDetails** (extends LibraryItem)
- DocumentDate, DocumentNumber, Source, PageCount
- Completeness: title+date+source+docNumber = 100%

**PlanDetails** (extends LibraryItem)
- DrawingNumber, DrawingTitle, ProjectName, Revision, Scale, SheetNumber, Discipline, Date
- Completeness: drawingNumber+revision+title+date = 100%

**LibraryFile**
- Id, LibraryItemId, OriginalPath, FileType, SizeBytes
- ContentHash (SHA256) - UNIQUE INDEX for duplicate detection

**FileDerivative**
- Id, LibraryFileId, DerivativeType (Thumbnail/PageImage/ExtractedText/TitleBlockCrop)
- Path, GeneratedAt, GeneratorVersion, InputHash
- UNIQUE: (LibraryFileId, DerivativeType, GeneratorVersion, InputHash)

**ExtractedPageText** (NEW - page-level storage)
- Id, LibraryFileId, PageNumber
- TextContent, TextPath (for large text, store as file)
- Confidence (0-100), Method (Native/Tesseract/Gemini)
- CreatedAt

**Category** (hierarchical)
- Id, Name, ParentId
- UNIQUE: (ParentId, Name) - case-insensitive

**Tag** (flat)
- Id, Name
- UNIQUE: Name (case-insensitive)

**LibraryItemCategory** / **LibraryItemTag** (many-to-many)

**ProcessingState** (NEW - current state per item)
- Id, LibraryItemId (UNIQUE)
- CurrentStep (ExtractText/OCRLocal/OCRCloud/Chunk/Embed/Categorize/Complete/Failed)
- LastRunId (GUID), LastError, NextRetryAt, RetryCount, LockedUntil

**ProcessingLog** (history)
- Id, LibraryItemId, RunId (GUID), StartedAt, CompletedAt
- ProcessorName, ProcessorVersion, Step, Status, ErrorMessage
- InputHash, CostEstimate, RetryCount

**ReviewQueueItem**
- Id, LibraryItemId, AIExtractedData (JSON), FieldsNeedingReview (JSON)
- ReviewedBy, ReviewedAt, Notes

### PostgreSQL + pgvector

**DocumentChunk**
- Id, LibraryItemId, ChunkIndex, Content (~500 tokens)
- PageNumbers (array), Embedding (vector 1536), Metadata (JSON)
- EmbeddingModel, EmbeddingVersion, ChunkingParams (JSON)
- TextHash, CreatedAt
- UNIQUE: (LibraryItemId, EmbeddingVersion, ChunkIndex, TextHash)

### Uniqueness Constraints Summary

| Table | Constraint | Purpose |
|-------|-----------|---------|
| LibraryFile | ContentHash | Detect duplicate uploads |
| FileDerivative | (FileId, Type, Version, InputHash) | Safe reprocessing |
| DocumentChunk | (ItemId, EmbedVersion, ChunkIdx, TextHash) | Safe re-embedding |
| Category | (ParentId, Name) | No duplicate categories |
| Tag | Name | No duplicate tags |
| ProcessingState | LibraryItemId | One state per item |

## Category Taxonomy

```
Timber
├── Bridges & Trestles
├── Retaining Structures
└── Repair & Rehabilitation

Steel
├── Bridges & Spans
├── Culverts
├── Retaining Structures
└── Repair & Rehabilitation

Concrete
├── Bridges & Spans
├── Culverts
├── Retaining Structures
└── Repair & Rehabilitation

Structures
├── Load Analysis
├── Foundation Design
├── Geotechnical
├── Drainage & Hydraulics
└── General Design Principles

Standards & References
├── AREMA
├── Transport Canada
├── Provincial Codes
├── Company Standards
├── Inspection & Maintenance
└── Historical Records
```

AI auto-generates additional tags for cross-cutting details (years, railway companies, bridge names).

## AI Processing Pipeline

### OCR Strategy (Layered)

```
1. Check if PDF has embedded text
   ├── Yes → Extract native text (free, fast)
   └── No → Continue to step 2

2. Run local Tesseract OCR
   ├── Confidence > 70% → Use result
   └── Confidence < 70% → Continue to step 3

3. Escalate to Cloud AI (Gemini Vision)
   ├── Success → Use result
   └── Failure → Mark for manual review
```

Cost savings: ~60-80% for mixed-quality collections.

### Text Cleanup Pipeline (NEW)

Before chunking, apply deterministic cleanup:
1. Remove repeated headers/footers (lines appearing on 3+ pages)
2. Remove standalone page numbers
3. Normalize whitespace (collapse multiple spaces, preserve meaningful line breaks)
4. Remove boilerplate stamps (date stamps, "COPY", etc.)
5. Preserve table structure (line breaks matter for tables)

### Books (single cover photo)
1. Admin uploads single high-res photo (cover + spine + back visible)
2. **Background Job**: Vision AI auto-crops regions, extracts metadata
3. AI assigns categories/tags from taxonomy
4. Calculate CompletenessScore based on field presence
5. If confidence < 80% OR completeness < 60% → review queue
6. Admin reviews/corrects → publish

### PDFs (documents)
1. Upload PDF (single or bulk import)
2. **Background Job**:
   - Layered text extraction (see OCR Strategy)
   - Store text per page in ExtractedPageText
   - Run text cleanup pipeline
3. Chunk cleaned text into ~500-token segments with page numbers
4. Generate embeddings, store in pgvector with versioning
5. Claude analyzes first pages for metadata + categorization
6. Calculate CompletenessScore
7. If confidence < 80% OR completeness < 60% → review queue
8. Admin reviews/corrects → publish

### Plans (title-block focused)
1. Upload plan PDF
2. **Background Job**:
   - Rasterize page 1 at moderate DPI (150-200)
   - Attempt title block crops (bottom-right, bottom, right side)
   - OCR only the cropped regions (cheaper, faster)
   - Store crop images as FileDerivative
3. Extract: DrawingTitle, DrawingNumber, ProjectName, Revision, Date
4. Generate thumbnail preview
5. If extraction fails → full manual entry with crop images as hints

### AI Provider Strategy
- **Gemini**: Primary for OCR/vision (better with handwriting and old scans)
- **Claude**: Primary for metadata extraction, categorization, Q&A (better reasoning)
- **Fallback**: If one fails, retry with the other
- **Tesseract**: Local baseline OCR to reduce cloud costs

## Search & Q&A

### Hybrid Ranking Formula (NEW)

```
FinalScore = w1 * NormalizedKeyword
           + w2 * NormalizedSemantic
           + w3 * RecencyBoost
           + w4 * TypeBoost
           + w5 * FieldMatchBoost

Default weights (configurable in appsettings):
  w1 = 0.4  (keyword/BM25)
  w2 = 0.5  (semantic/cosine)
  w3 = 0.05 (recency - newer docs slightly preferred)
  w4 = 0.05 (type boost - e.g., prefer Standards for regulatory queries)
  w5 = 0.2  (field boost - title/docNumber matches weighted higher)

Field Boosting:
  Title match: 3x
  DocumentNumber/DrawingNumber match: 3x
  Tag match: 2x
  Author match: 1.5x
  Content match: 1x (baseline)
```

### Search Types
1. **Keyword** (SQL Server full-text): Exact matches on title, author, tags
2. **Semantic** (pgvector): Conceptually similar content via embeddings
3. **Hybrid** (default): Both combined using formula above

### Q&A (RAG with Retrieval Gates) (UPDATED)

**Two-Mode Answering:**

1. **Grounded Mode (default)**:
   - Answer primarily from retrieved chunks
   - General knowledge only for brief context
   - Every substantive claim must have citation where possible
   - If top_similarity < 0.65: respond with "Insufficient support in library. Try: [suggested refined searches]"

2. **General Mode (explicit toggle)**:
   - Broader engineering explanations allowed
   - Still shows retrieved references separately
   - Clear indicator: "This answer includes general engineering knowledge"

**Q&A Pipeline:**
1. User question → embedding
2. Retrieve top 5-10 relevant chunks from pgvector
3. **Retrieval gate**: If max(similarity) < 0.65, return "insufficient support" with search suggestions
4. Send chunks + question to Claude with mode-appropriate prompt
5. Claude answers with citations
6. **Context-aware disclaimer**:
   - Grounded mode + strong retrieval: "AI-assisted summary with citations from library."
   - General mode or weak retrieval: "AI-generated content. Consult original standards for critical decisions."
7. Show which documents were used (audit transparency)
8. Audit log: query, retrieved chunks, model response, mode, user

## Security & File Serving (UPDATED)

### Secure File Delivery
- Store only internal file IDs in URLs: `/files/{fileId}` (never filesystem paths)
- Validate access based on LibraryItemId and user role
- Use Content-Disposition headers appropriately
- Implement path traversal prevention in FileStorageService

### Authentication & Authorization
- ASP.NET Core Identity with Admin/Viewer roles
- No anonymous access to any file endpoints
- Session timeout aligned with FRMv2 (30 min idle)

### API Key Security
- Store in environment variables or Windows DPAPI
- Never in appsettings.json in source control
- Use user secrets for local development

### Audit Logging
- Logins (success/failure)
- File downloads (who, what, when)
- Q&A prompts and responses:
  - Full text retained for 30 days (debugging)
  - After 30 days: metadata only (user, timestamp, item IDs, similarity scores, cost)
- Processing job results

## Backup & Recovery (NEW)

### Components to Backup
1. **SQL Server**: metadata, Hangfire state, Identity
2. **PostgreSQL**: vectors, chunks
3. **Filesystem**: originals, derivatives, thumbnails

### Backup Strategy
```
Daily:
- SQL Server: Full backup
- PostgreSQL: pg_dump
- Filesystem: Incremental backup of app_data/

Weekly:
- Full filesystem backup
- Verify backup integrity

Retention:
- Daily: 7 days
- Weekly: 4 weeks
```

### Restore Procedure (document this!)
1. Stop IIS application pool
2. Restore SQL Server from backup
3. Restore PostgreSQL from pg_dump
4. Restore filesystem to app_data/
5. Verify file paths match database records
6. Start application pool
7. Run health check: `/health` (validates all dependencies)

### Health Endpoint
`/health` validates:
- SQL Server connectivity
- PostgreSQL connectivity
- Filesystem read/write to app_data/
- Tesseract/OCR dependencies present
- Hangfire server running

## Monitoring (NEW)

### Admin Dashboard Metrics
- Items by status (Pending/Processing/Review/Published/Failed)
- Review queue count
- Processing job success/failure rate (last 24h)
- Average processing time per item type
- OCR escalation rate (local → cloud)
- Estimated API costs (daily/weekly)

### Alerts (optional, Phase 6)
- Job failure rate > 10%
- Review queue > 50 items
- Disk space < 10GB

## Pages

### Public (Viewer + Admin)
- `/` - Home: search box + category grid
- `/search` - Results with filters (material, type, year range)
- `/ask` - Q&A chat interface with mode toggle and AI disclaimer
- `/browse` - Category tree navigation
- `/item/{id}` - Item detail + metadata + download link
- `/files/{fileId}` - Secure file download (authorized)
- `/files/{fileId}/view` - Embedded PDF viewer

### Admin Only
- `/admin` - Dashboard (stats, queue count, processing metrics, costs)
- `/admin/upload` - Single item upload
- `/admin/bulk` - Bulk import trigger/status/progress
- `/admin/review` - Review queue (sorted by completeness, then confidence)
- `/admin/items` - Manage all items (CRUD)
- `/admin/categories` - Manage taxonomy
- `/admin/users` - Manage users/roles
- `/admin/jobs` - Hangfire dashboard (processing status)

## Error Handling

| Scenario | Handling |
|----------|----------|
| AI can't read cover (blurry) | Mark "Failed", allow manual entry |
| Local OCR low confidence | Escalate to cloud OCR |
| Cloud OCR fails | Mark for manual review with partial data |
| Confidence < 50% all fields | Full manual entry with AI hints |
| API rate limit/timeout | Queue retry with exponential backoff |
| Duplicate file detected | Hash check, warn admin, link to existing |
| Job already running for item | Skip (LockedUntil check) |
| Reprocessing same input | Skip if InputHash matches (idempotent) |

## Implementation Phases

### Phase 1: Foundation
- [ ] Project scaffolding (ASP.NET Core 9.0 + Razor Pages)
- [ ] Solution structure matching FRMv2 patterns
- [ ] SQL Server + EF Core setup with migrations
- [ ] All uniqueness constraints defined
- [ ] Docker compose for PostgreSQL + pgvector
- [ ] Hangfire setup (SQL Server storage, single queue)
- [ ] ASP.NET Core Identity (Admin/Viewer roles)
- [ ] Tailwind CSS setup
- [ ] Basic layout and navigation
- [ ] Secure file serving endpoint (`/files/{fileId}`)
- [ ] Health endpoint with full dependency validation (`/health`)

### Phase 2: Core Library (Manual CRUD)
- [ ] File upload service with hash-based duplicate detection
- [ ] FileDerivative tracking
- [ ] ExtractedPageText table
- [ ] Manual metadata entry forms (books, documents)
- [ ] CompletenessScore calculation
- [ ] Category/tag management (admin)
- [ ] Browse by category
- [ ] Basic keyword search (SQL full-text with field boosting)
- [ ] Item detail pages
- [ ] Embedded PDF viewer
- [ ] File download (authorized)

### Phase 3: AI Integration
- [ ] AI service abstraction (IAIProvider interface)
- [ ] Claude provider implementation
- [ ] Gemini provider implementation
- [ ] PDF rasterization library research and selection (for OCR/thumbnails)
- [ ] Tesseract local OCR integration
- [ ] Layered OCR pipeline with page-level storage
- [ ] Text cleanup pipeline
- [ ] ProcessingState table + idempotent job logic
- [ ] Book cover processing (vision + auto-crop)
- [ ] PDF text extraction pipeline
- [ ] Plan title-block cropping
- [ ] Metadata extraction + auto-categorization
- [ ] Review queue UI (sorted by completeness)
- [ ] ProcessingLog tracking

### Phase 4: Search & Q&A
- [ ] pgvector connection from .NET (Npgsql)
- [ ] Embedding generation service (with versioning)
- [ ] Document chunking service (with cleanup)
- [ ] Semantic search implementation
- [ ] Hybrid search with explicit ranking formula
- [ ] Field boosting configuration
- [ ] RAG-based Q&A with retrieval gates
- [ ] Two-mode answering (grounded/general)
- [ ] AI disclaimer/warning display
- [ ] Q&A audit logging

### Phase 5: Bulk Import & Polish
- [ ] Bulk import background job
- [ ] Progress tracking dashboard
- [ ] Error reporting for failed items
- [ ] Admin dashboard metrics (costs, rates)
- [ ] Reprocessing UI (re-run OCR, re-embed, re-categorize per item)
- [ ] Mobile responsive UI
- [ ] Performance optimization

### Phase 6: Production Deploy
- [ ] Production PostgreSQL setup
- [ ] IIS deployment alongside FRMv2
- [ ] Production configuration
- [ ] Backup procedures documented and tested
- [ ] Restore procedure documented and tested
- [ ] Monitoring/alerting setup

## Local Development Setup

```
Prerequisites:
- .NET 9.0 SDK
- Docker Desktop
- SQL Server LocalDB
- Claude API key (ANTHROPIC_API_KEY)
- Gemini API key (GOOGLE_API_KEY)
- Tesseract OCR installed locally

# Start PostgreSQL + pgvector
docker compose up -d

# Run migrations
dotnet ef database update

# Start the app
dotnet run

# Hangfire dashboard available at /admin/jobs
```

## Verification Checklist

### Phase 1-2 (Manual CRUD)
- [ ] Duplicate upload → warns about existing item (hash check)
- [ ] File download → requires authentication
- [ ] Viewer role → cannot access admin pages
- [ ] Admin role → can upload, review, edit items

### Phase 3 (AI)
- [ ] Upload a book cover photo → AI extracts correct metadata
- [ ] Upload a clear PDF → native text extracted per page
- [ ] Upload an image-only PDF → Tesseract processes it
- [ ] Upload a handwritten PDF → escalates to Gemini
- [ ] Low-confidence item → appears in review queue
- [ ] Low-completeness item → appears in review queue
- [ ] ProcessingLog shows extraction history
- [ ] Reprocessing same file → skips (idempotent)
- [ ] Plan upload → title block cropped and OCR'd

### Phase 4 (Search & Q&A)
- [ ] Search "load calculations" → finds docs without exact phrase
- [ ] Title match → ranked higher than content match
- [ ] Q&A with good retrieval → grounded answer with citations
- [ ] Q&A with weak retrieval → "insufficient support" message
- [ ] Q&A general mode → shows general knowledge indicator
- [ ] Q&A shows AI disclaimer warning

### Phase 5-6 (Production)
- [ ] Backup/restore procedure tested
- [ ] Admin dashboard shows processing metrics
- [ ] IIS deployment works alongside FRMv2

## Files to Create

```
BriansLegacyV2/
├── src/
│   └── BriansLegacy/
│       ├── BriansLegacy.csproj
│       ├── Program.cs
│       ├── appsettings.json
│       ├── appsettings.Development.json
│       ├── Data/
│       │   ├── ApplicationDbContext.cs
│       │   ├── VectorDbContext.cs
│       │   └── Migrations/
│       ├── Models/
│       │   ├── LibraryItem.cs
│       │   ├── BookDetails.cs
│       │   ├── DocumentDetails.cs
│       │   ├── PlanDetails.cs
│       │   ├── LibraryFile.cs
│       │   ├── FileDerivative.cs
│       │   ├── ExtractedPageText.cs
│       │   ├── Category.cs
│       │   ├── Tag.cs
│       │   ├── ProcessingState.cs
│       │   ├── ProcessingLog.cs
│       │   └── ReviewQueueItem.cs
│       ├── Services/
│       │   ├── AI/
│       │   │   ├── IAIProvider.cs
│       │   │   ├── ClaudeProvider.cs
│       │   │   ├── GeminiProvider.cs
│       │   │   └── TesseractOcrService.cs
│       │   ├── DocumentProcessorService.cs
│       │   ├── TextCleanupService.cs
│       │   ├── SearchService.cs
│       │   ├── EmbeddingService.cs
│       │   ├── FileStorageService.cs
│       │   ├── CompletenessCalculator.cs
│       │   └── QAService.cs
│       ├── Jobs/
│       │   ├── ProcessBookCoverJob.cs
│       │   ├── ProcessPdfJob.cs
│       │   ├── ProcessPlanJob.cs
│       │   ├── GenerateEmbeddingsJob.cs
│       │   └── BulkImportJob.cs
│       ├── Pages/
│       │   ├── Index.cshtml
│       │   ├── Search.cshtml
│       │   ├── Ask.cshtml
│       │   ├── Browse.cshtml
│       │   ├── Item.cshtml
│       │   └── Admin/
│       │       ├── Index.cshtml (dashboard with metrics)
│       │       ├── Upload.cshtml
│       │       ├── Bulk.cshtml
│       │       ├── Review.cshtml
│       │       ├── Items.cshtml
│       │       ├── Categories.cshtml
│       │       └── Users.cshtml
│       └── wwwroot/
│           ├── css/
│           └── js/
├── docker-compose.yml
├── tailwind.config.js
├── package.json
├── docs/
│   └── backup-restore.md
└── README.md
```

## Changelog

- **v4 (2026-01-18)**: Incorporated ChatGPT round 3 feedback (user-reviewed)
  - Added Q&A log retention policy: 30 days full, then metadata only
  - Added context-aware disclaimers (softer for grounded, stronger for general)
  - Added full dependency validation in /health endpoint
  - Added PDF rasterization research to Phase 3
  - Added reprocessing UI to Phase 5
  - Single Hangfire queue (keep simple for v1)
  - No publish gate: all items visible to viewers regardless of status
  - **Deferred**: NormalizedIdentifiers (add if regular search insufficient)
  - **Declined again**: PhysicalRef field

- **v3 (2026-01-18)**: Incorporated ChatGPT round 2 feedback (user-reviewed)
  - Added ProcessingState table for current state + idempotent job logic
  - Added uniqueness constraints for safe reprocessing
  - Added ExtractedPageText for page-level text storage
  - Added text cleanup pipeline before chunking
  - Added explicit hybrid ranking formula with field boosting
  - Added Q&A retrieval gates and two-mode answering
  - Added CompletenessScore (field presence) separate from ConfidenceScore
  - Added title-block cropping for Plans
  - Added backup/restore procedures
  - Added full monitoring metrics for admin dashboard
  - **Declined**: Provenance fields (not needed - all from Brian's collection)
  - **Declined**: Formal capture protocol doc (verbal instructions sufficient)

- **v2 (2026-01-18)**: Incorporated ChatGPT round 1 feedback
  - Added Hangfire background jobs to Phase 1
  - Added layered OCR strategy (Tesseract → Cloud escalation)
  - Added FileDerivative and ProcessingLog tables
  - Added embedding versioning fields
  - Added Q&A audit logging and AI disclaimer
  - Clarified Plans as lower priority with simple title extraction
  - Confirmed: 1 photo per book, mixed PDFs, cloud AI OK, English only, small team
