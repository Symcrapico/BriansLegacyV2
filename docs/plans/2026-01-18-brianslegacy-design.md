# BriansLegacyV2 - Private Railway Engineering Library

## Overview

A private online library for railway structural engineering materials inherited from a retired senior engineer. Enables a team of engineers to search, browse, and ask questions about books, documents, and plans using AI-powered cataloging and semantic search.

## Key Requirements

- **Collection**: ~100-500 books (cover photos) + ~500-2000 documents/plans (PDFs)
- **Quality**: Mixed - some handwritten, old scans (60-70% process cleanly)
- **Access**: Role-based (Admin: add/edit/review, Viewer: browse/search)
- **AI**: Cloud APIs (Claude + Gemini) for vision, OCR, categorization, search, Q&A
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
└── Document Processor (OCR + chunking)
        │
        ├──► SQL Server (metadata, users, review queue)
        ├──► PostgreSQL + pgvector (embeddings, chunks)
        └──► Local Filesystem (original files, thumbnails)
```

## Data Model

### SQL Server (EF Core)

**LibraryItem**
- Id, Type (Book/Document/Plan), Title, Description
- Status (Pending/Processing/Review/Published/Failed)
- ConfidenceScore (0-100)
- CreatedAt, UpdatedAt, CreatedBy

**BookDetails** (extends LibraryItem)
- Author, Publisher, ISBN, Edition, Year, PageCount

**DocumentDetails** (extends LibraryItem)
- DocumentDate, DocumentNumber, Source, PageCount

**File**
- LibraryItemId, OriginalPath, ThumbnailPath, FileType, SizeBytes

**Category** (hierarchical)
- Id, Name, ParentId

**Tag** (flat)
- Id, Name

**ReviewQueue**
- LibraryItemId, AIExtractedData (JSON), FieldsNeedingReview, ReviewedBy, ReviewedAt

### PostgreSQL + pgvector

**DocumentChunk**
- Id, LibraryItemId, ChunkIndex, Content (~500 tokens)
- PageNumbers, Embedding (vector), Metadata (JSON)

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

### Books (cover photo)
1. Upload photo (cover + spine + back in single image)
2. Vision AI (Claude/Gemini) extracts: Title, Author, Publisher, ISBN, Edition, Year
3. AI assigns categories/tags from taxonomy
4. If confidence < 80% → review queue
5. Admin reviews/corrects → publish

### PDFs (documents & plans)
1. Upload PDF (single or bulk import)
2. Text extraction (native PDF text, fallback to Gemini OCR for images/handwriting)
3. Chunk into ~500-token segments with page numbers
4. Generate embeddings, store in pgvector
5. Claude analyzes first pages for metadata + categorization
6. If confidence < 80% → review queue
7. Admin reviews/corrects → publish

### AI Provider Strategy
- **Gemini**: Primary for OCR/vision (better with handwriting)
- **Claude**: Primary for metadata extraction, categorization, Q&A (better reasoning)
- **Fallback**: If one fails, retry with the other

## Search & Q&A

### Search Types
1. **Keyword** (SQL Server full-text): Exact matches on title, author, tags
2. **Semantic** (pgvector): Conceptually similar content via embeddings
3. **Hybrid** (default): Both combined and re-ranked

### Q&A (RAG)
1. User question → embedding
2. Retrieve top 5-10 relevant chunks from pgvector
3. Send chunks + question to Claude
4. Claude answers using only retrieved content
5. Response includes citations with document name + page numbers

## Pages

### Public (Viewer + Admin)
- `/` - Home: search box + category grid
- `/search` - Results with filters
- `/ask` - Q&A chat interface
- `/browse` - Category tree navigation
- `/item/{id}` - Item detail + metadata
- `/item/{id}/pdf` - PDF viewer

### Admin Only
- `/admin` - Dashboard (stats, queue count)
- `/admin/upload` - Single item upload
- `/admin/bulk` - Bulk import trigger/status
- `/admin/review` - Review queue (AI guesses vs editable fields)
- `/admin/items` - Manage all items (CRUD)
- `/admin/categories` - Manage taxonomy
- `/admin/users` - Manage users/roles

## Error Handling

| Scenario | Handling |
|----------|----------|
| AI can't read cover (blurry) | Mark "Failed", allow manual entry |
| OCR fails on PDF | Gemini → Claude fallback, then manual review |
| Confidence < 50% all fields | Full manual entry with AI hints |
| API rate limit/timeout | Queue retry with exponential backoff |
| Duplicate file detected | Warn admin, show existing item |

## Implementation Phases

### Phase 1: Foundation
- [ ] Project scaffolding (ASP.NET Core 9.0 + Razor Pages)
- [ ] Solution structure matching FRMv2 patterns
- [ ] SQL Server + EF Core setup with migrations
- [ ] Docker compose for PostgreSQL + pgvector
- [ ] ASP.NET Core Identity (Admin/Viewer roles)
- [ ] Tailwind CSS setup

### Phase 2: Core Library
- [ ] File upload service (local filesystem storage)
- [ ] Manual metadata entry forms
- [ ] Category/tag management
- [ ] Browse by category
- [ ] Basic keyword search
- [ ] Item detail pages
- [ ] Embedded PDF viewer

### Phase 3: AI Integration
- [ ] AI service abstraction (IAIProvider interface)
- [ ] Claude provider implementation
- [ ] Gemini provider implementation
- [ ] Book cover processing pipeline
- [ ] PDF text extraction + OCR
- [ ] Metadata extraction + auto-categorization
- [ ] Review queue UI

### Phase 4: Search & Q&A
- [ ] pgvector connection from .NET
- [ ] Embedding generation service
- [ ] Document chunking service
- [ ] Semantic search implementation
- [ ] Hybrid search (keyword + semantic)
- [ ] RAG-based Q&A with citations

### Phase 5: Bulk Import
- [ ] Bulk import console command / background job
- [ ] Watched folder detection
- [ ] Progress tracking dashboard
- [ ] Error reporting for failed items

### Phase 6: Polish & Deploy
- [ ] Mobile responsive UI
- [ ] Performance optimization
- [ ] Audit logging
- [ ] IIS deployment alongside FRMv2
- [ ] Production configuration

## Local Development Setup

```
Prerequisites:
- .NET 9.0 SDK
- Docker Desktop
- SQL Server LocalDB
- Claude API key (ANTHROPIC_API_KEY)
- Gemini API key (GOOGLE_API_KEY)

# Start PostgreSQL + pgvector
docker compose up -d

# Run migrations
dotnet ef database update

# Start the app
dotnet run
```

## Verification Checklist

- [ ] Upload a book cover photo → AI extracts correct metadata
- [ ] Upload a clear PDF → text extracted, searchable
- [ ] Upload a handwritten PDF → OCR extracts readable text
- [ ] Low-confidence item → appears in review queue
- [ ] Search "load calculations" → finds docs without exact phrase in title
- [ ] Ask "What's the max span for timber trestle?" → returns answer with citation
- [ ] Viewer role → cannot access admin pages
- [ ] Admin role → can upload, review, edit items

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
│       │   └── Migrations/
│       ├── Models/
│       │   ├── LibraryItem.cs
│       │   ├── BookDetails.cs
│       │   ├── DocumentDetails.cs
│       │   ├── Category.cs
│       │   ├── Tag.cs
│       │   └── ReviewQueueItem.cs
│       ├── Services/
│       │   ├── AI/
│       │   │   ├── IAIProvider.cs
│       │   │   ├── ClaudeProvider.cs
│       │   │   └── GeminiProvider.cs
│       │   ├── DocumentProcessor.cs
│       │   ├── SearchService.cs
│       │   ├── EmbeddingService.cs
│       │   └── FileStorageService.cs
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
│       │       └── Items.cshtml
│       └── wwwroot/
│           ├── css/
│           └── js/
├── docker-compose.yml
├── tailwind.config.js
├── package.json
└── README.md
```
