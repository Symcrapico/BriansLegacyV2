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

## Data Model

### SQL Server (EF Core)

**LibraryItem** (base)
- Id (GUID), Type (Book/Document/Plan), Title, Description
- Status (Pending/Processing/Review/Published/Failed)
- ConfidenceScore (0-100)
- CreatedAt, UpdatedAt, CreatedBy

**BookDetails** (extends LibraryItem)
- Author, Publisher, ISBN, Edition, Year, PageCount

**DocumentDetails** (extends LibraryItem)
- DocumentDate, DocumentNumber, Source, PageCount

**PlanDetails** (extends LibraryItem) - Lower priority, Phase 2+
- DrawingNumber, Revision, Scale, SheetNumber

**LibraryFile**
- Id, LibraryItemId, OriginalPath, FileType, SizeBytes, ContentHash

**FileDerivative** (NEW - per ChatGPT feedback)
- Id, LibraryFileId, DerivativeType (Thumbnail/PageImage/ExtractedText/TitleBlockCrop)
- Path, GeneratedAt, GeneratorVersion

**Category** (hierarchical)
- Id, Name, ParentId

**Tag** (flat)
- Id, Name

**LibraryItemCategory** / **LibraryItemTag** (many-to-many)

**ReviewQueueItem**
- Id, LibraryItemId, AIExtractedData (JSON), FieldsNeedingReview (JSON)
- ReviewedBy, ReviewedAt, Notes

**ProcessingLog** (NEW - per ChatGPT feedback)
- Id, LibraryItemId, RunId (GUID), StartedAt, CompletedAt
- ProcessorName, ProcessorVersion, Status, ErrorMessage
- InputHash, CostEstimate, RetryCount

### PostgreSQL + pgvector

**DocumentChunk**
- Id, LibraryItemId, ChunkIndex, Content (~500 tokens)
- PageNumbers, Embedding (vector 1536), Metadata (JSON)
- EmbeddingModel, EmbeddingVersion, ChunkingParams (JSON)
- TextHash, CreatedAt

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

### OCR Strategy (Layered - per ChatGPT feedback)

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

This reduces cloud API costs by ~60-80% for mixed-quality collections.

### Books (single cover photo)
1. Admin uploads single high-res photo (cover + spine + back visible)
2. **Background Job**: Vision AI auto-crops regions, extracts metadata
3. AI assigns categories/tags from taxonomy
4. If confidence < 80% → review queue
5. Admin reviews/corrects → publish

### PDFs (documents)
1. Upload PDF (single or bulk import)
2. **Background Job**: Layered text extraction (see OCR Strategy above)
3. Chunk into ~500-token segments with page numbers preserved
4. Generate embeddings, store in pgvector with versioning
5. Claude analyzes first pages for metadata + categorization
6. If confidence < 80% → review queue
7. Admin reviews/corrects → publish

### Plans (lower priority - after books/PDFs)
1. Upload plan PDF
2. **Background Job**: Extract title from first page (simple approach)
3. Generate thumbnail preview
4. Manual categorization initially, AI assist later

### AI Provider Strategy
- **Gemini**: Primary for OCR/vision (better with handwriting and old scans)
- **Claude**: Primary for metadata extraction, categorization, Q&A (better reasoning)
- **Fallback**: If one fails, retry with the other
- **Tesseract**: Local baseline OCR to reduce cloud costs

## Search & Q&A

### Search Types
1. **Keyword** (SQL Server full-text): Exact matches on title, author, tags
2. **Semantic** (pgvector): Conceptually similar content via embeddings
3. **Hybrid** (default): Both combined and re-ranked

### Q&A (RAG + General Knowledge)
1. User question → embedding
2. Retrieve top 5-10 relevant chunks from pgvector
3. Send chunks + question to Claude
4. Claude answers using library content + general engineering knowledge
5. **Clear warning**: "AI-generated content. Verify critical information."
6. Response includes citations with document name + page numbers
7. Audit log: which chunks were provided, model response

## Pages

### Public (Viewer + Admin)
- `/` - Home: search box + category grid
- `/search` - Results with filters (material, type, year range)
- `/ask` - Q&A chat interface with AI disclaimer
- `/browse` - Category tree navigation
- `/item/{id}` - Item detail + metadata + download link
- `/item/{id}/view` - Embedded PDF viewer

### Admin Only
- `/admin` - Dashboard (stats, queue count, processing jobs)
- `/admin/upload` - Single item upload
- `/admin/bulk` - Bulk import trigger/status/progress
- `/admin/review` - Review queue (AI guesses vs editable fields)
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
| Duplicate file detected | Hash-based check, warn admin, show existing item |

## Security & Audit

- ASP.NET Core Identity with Admin/Viewer roles
- No anonymous access to file endpoints
- Files served through authorized controller (not static folder)
- API keys in environment variables (not appsettings.json in source)
- Audit logging for: logins, downloads, Q&A prompts/responses

## Implementation Phases

### Phase 1: Foundation
- [ ] Project scaffolding (ASP.NET Core 9.0 + Razor Pages)
- [ ] Solution structure matching FRMv2 patterns
- [ ] SQL Server + EF Core setup with migrations
- [ ] Docker compose for PostgreSQL + pgvector
- [ ] **Hangfire setup** (SQL Server storage) - moved from Phase 5
- [ ] ASP.NET Core Identity (Admin/Viewer roles)
- [ ] Tailwind CSS setup
- [ ] Basic layout and navigation

### Phase 2: Core Library (Manual CRUD)
- [ ] File upload service (local filesystem storage)
- [ ] FileDerivative tracking
- [ ] Manual metadata entry forms (books, documents)
- [ ] Category/tag management (admin)
- [ ] Browse by category
- [ ] Basic keyword search (SQL full-text)
- [ ] Item detail pages
- [ ] Embedded PDF viewer
- [ ] File download (authorized)

### Phase 3: AI Integration
- [ ] AI service abstraction (IAIProvider interface)
- [ ] Claude provider implementation
- [ ] Gemini provider implementation
- [ ] Tesseract local OCR integration
- [ ] Layered OCR pipeline
- [ ] Book cover processing (vision + auto-crop)
- [ ] PDF text extraction pipeline
- [ ] Metadata extraction + auto-categorization
- [ ] Review queue UI
- [ ] ProcessingLog tracking

### Phase 4: Search & Q&A
- [ ] pgvector connection from .NET (Npgsql)
- [ ] Embedding generation service (with versioning)
- [ ] Document chunking service
- [ ] Semantic search implementation
- [ ] Hybrid search (keyword + semantic merge)
- [ ] RAG-based Q&A with citations
- [ ] AI disclaimer/warning display
- [ ] Q&A audit logging

### Phase 5: Bulk Import & Polish
- [ ] Bulk import background job
- [ ] Progress tracking dashboard
- [ ] Error reporting for failed items
- [ ] Mobile responsive UI
- [ ] Performance optimization
- [ ] Audit logging completion

### Phase 6: Production Deploy
- [ ] Production PostgreSQL setup (same server or separate)
- [ ] IIS deployment alongside FRMv2
- [ ] Production configuration
- [ ] Backup strategy for both databases
- [ ] Monitoring setup

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

- [ ] Upload a book cover photo → AI extracts correct metadata
- [ ] Upload a clear PDF → native text extracted, searchable
- [ ] Upload an image-only PDF → Tesseract processes it
- [ ] Upload a handwritten PDF → escalates to Gemini, extracts readable text
- [ ] Low-confidence item → appears in review queue
- [ ] ProcessingLog shows extraction history
- [ ] Search "load calculations" → finds docs without exact phrase in title
- [ ] Ask "What's the max span for timber trestle?" → returns answer with citation
- [ ] Q&A shows AI disclaimer warning
- [ ] Viewer role → cannot access admin pages
- [ ] Admin role → can upload, review, edit items
- [ ] File download → requires authentication
- [ ] Duplicate upload → warns about existing item

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
│       │   ├── Category.cs
│       │   ├── Tag.cs
│       │   ├── ReviewQueueItem.cs
│       │   └── ProcessingLog.cs
│       ├── Services/
│       │   ├── AI/
│       │   │   ├── IAIProvider.cs
│       │   │   ├── ClaudeProvider.cs
│       │   │   ├── GeminiProvider.cs
│       │   │   └── TesseractOcrService.cs
│       │   ├── DocumentProcessorService.cs
│       │   ├── SearchService.cs
│       │   ├── EmbeddingService.cs
│       │   ├── FileStorageService.cs
│       │   └── QAService.cs
│       ├── Jobs/
│       │   ├── ProcessBookCoverJob.cs
│       │   ├── ProcessPdfJob.cs
│       │   ├── GenerateEmbeddingsJob.cs
│       │   └── BulkImportJob.cs
│       ├── Pages/
│       │   ├── Index.cshtml
│       │   ├── Search.cshtml
│       │   ├── Ask.cshtml
│       │   ├── Browse.cshtml
│       │   ├── Item.cshtml
│       │   └── Admin/
│       │       ├── Index.cshtml
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
└── README.md
```

## Changelog

- **v2 (2026-01-18)**: Incorporated ChatGPT review feedback
  - Added Hangfire background jobs to Phase 1
  - Added layered OCR strategy (Tesseract → Cloud escalation)
  - Added FileDerivative and ProcessingLog tables
  - Added embedding versioning fields
  - Added Q&A audit logging and AI disclaimer
  - Clarified Plans as lower priority with simple title extraction
  - Confirmed: 1 photo per book, mixed PDFs, cloud AI OK, English only, small team
