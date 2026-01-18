# Backlog

> Updated: 2026-01-18T21:42:47.317Z
> Active: TASK-001-A-1

---

## EPIC-001: Phase 1 - Foundation
**Priority:** P1 | **Progress:** 0/11

### FEAT-001-A: Project Scaffolding (0/3)

| ID | Task | Status | Est |
|----|------|--------|-----|
| TASK-001-A-1 | Create ASP.NET Core 9.0 + Razor Pages project with solution structure matching FRMv2 patterns | 🟨 |  |
| TASK-001-A-2 | Set up Tailwind CSS with build scripts (css:build, css:watch) | 🟦 |  |
| TASK-001-A-3 | Create basic layout (_Layout.cshtml) and navigation | 🟦 |  |

### FEAT-001-B: Database Setup (0/4)

| ID | Task | Status | Est |
|----|------|--------|-----|
| TASK-001-B-1 | Set up SQL Server + EF Core 9.0 with ApplicationDbContext | ⬜ |  |
| TASK-001-B-2 | Create initial EF Core migration with all entities (LibraryItem, BookDetails, DocumentDetails, PlanDetails, LibraryFile, FileDerivative, ExtractedPageText, Category, Tag, ProcessingState, ProcessingLog, ReviewQueueItem) | ⬜ |  |
| TASK-001-B-3 | Add all uniqueness constraints (ContentHash, FileDerivative composite, Category name, Tag name, ProcessingState) | ⬜ |  |
| TASK-001-B-4 | Create docker-compose.yml for PostgreSQL + pgvector | ⬜ |  |

### FEAT-001-C: Infrastructure (0/4)

| ID | Task | Status | Est |
|----|------|--------|-----|
| TASK-001-C-1 | Set up Hangfire with SQL Server storage (single queue) | ⬜ |  |
| TASK-001-C-2 | Set up ASP.NET Core Identity with Admin/Viewer roles | ⬜ |  |
| TASK-001-C-3 | Create secure file serving endpoint (/files/{fileId}) with authorization | ⬜ |  |
| TASK-001-C-4 | Create health endpoint (/health) with full dependency validation (SQL, Postgres, filesystem, Hangfire) | ⬜ |  |

## EPIC-002: Phase 2 - Core Library
**Priority:** P2 | **Progress:** 0/11

### FEAT-002-A: File Management (0/3)

| ID | Task | Status | Est |
|----|------|--------|-----|
| TASK-002-A-1 | Create FileStorageService with hash-based duplicate detection | ⬜ |  |
| TASK-002-A-2 | Create FileDerivative tracking service | ⬜ |  |
| TASK-002-A-3 | Create ExtractedPageText storage service | ⬜ |  |

### FEAT-002-B: Manual CRUD (0/5)

| ID | Task | Status | Est |
|----|------|--------|-----|
| TASK-002-B-1 | Create manual metadata entry forms for Books | ⬜ |  |
| TASK-002-B-2 | Create manual metadata entry forms for Documents | ⬜ |  |
| TASK-002-B-3 | Create CompletenessScore calculation service | ⬜ |  |
| TASK-002-B-4 | Create Category/Tag management admin pages | ⬜ |  |
| TASK-002-B-5 | Seed initial category taxonomy from design | ⬜ |  |

### FEAT-002-C: Browse & Search (0/3)

| ID | Task | Status | Est |
|----|------|--------|-----|
| TASK-002-C-1 | Create browse by category page | ⬜ |  |
| TASK-002-C-2 | Create basic keyword search with SQL full-text and field boosting | ⬜ |  |
| TASK-002-C-3 | Create item detail pages with PDF viewer | ⬜ |  |

## EPIC-003: Phase 3 - AI Integration
**Priority:** P3 | **Progress:** 0/14

### FEAT-003-A: AI Services (0/3)

| ID | Task | Status | Est |
|----|------|--------|-----|
| TASK-003-A-1 | Create IAIProvider interface for swappable AI providers | ⬜ |  |
| TASK-003-A-2 | Implement Claude provider (ClaudeAIProvider) | ⬜ |  |
| TASK-003-A-3 | Implement Gemini provider (GeminiAIProvider) | ⬜ |  |

### FEAT-003-B: OCR Pipeline (0/4)

| ID | Task | Status | Est |
|----|------|--------|-----|
| TASK-003-B-1 | Research and select PDF rasterization library for OCR/thumbnails | ⬜ |  |
| TASK-003-B-2 | Integrate Tesseract for local OCR processing | ⬜ |  |
| TASK-003-B-3 | Implement layered OCR pipeline (native → Tesseract → Cloud) with page-level storage | ⬜ |  |
| TASK-003-B-4 | Create text cleanup pipeline (headers, footers, whitespace, boilerplate) | ⬜ |  |

### FEAT-003-C: Processing Jobs (0/7)

| ID | Task | Status | Est |
|----|------|--------|-----|
| TASK-003-C-1 | Create ProcessingState table + idempotent job logic | ⬜ |  |
| TASK-003-C-2 | Create book cover processing job (vision + auto-crop) | ⬜ |  |
| TASK-003-C-3 | Create PDF text extraction pipeline job | ⬜ |  |
| TASK-003-C-4 | Create plan title-block cropping job | ⬜ |  |
| TASK-003-C-5 | Create metadata extraction + auto-categorization job | ⬜ |  |
| TASK-003-C-6 | Create review queue UI (sorted by completeness) | ⬜ |  |
| TASK-003-C-7 | Implement ProcessingLog tracking | ⬜ |  |

## EPIC-004: Phase 4 - Search & Q&A
**Priority:** P4 | **Progress:** 0/10

### FEAT-004-A: Vector Search (0/6)

| ID | Task | Status | Est |
|----|------|--------|-----|
| TASK-004-A-1 | Set up pgvector connection from .NET (Npgsql) | ⬜ |  |
| TASK-004-A-2 | Create embedding generation service with model/version tracking | ⬜ |  |
| TASK-004-A-3 | Create document chunking service (~500 tokens, with cleanup) | ⬜ |  |
| TASK-004-A-4 | Implement semantic search with pgvector | ⬜ |  |
| TASK-004-A-5 | Implement hybrid search with explicit ranking formula | ⬜ |  |
| TASK-004-A-6 | Configure field boosting (title 3x, docNumber 3x, tag 2x, author 1.5x) | ⬜ |  |

### FEAT-004-B: RAG Q&A (0/4)

| ID | Task | Status | Est |
|----|------|--------|-----|
| TASK-004-B-1 | Implement RAG-based Q&A with retrieval gates (0.65 threshold) | ⬜ |  |
| TASK-004-B-2 | Implement two-mode answering (grounded/general toggle) | ⬜ |  |
| TASK-004-B-3 | Create AI disclaimer/warning display component | ⬜ |  |
| TASK-004-B-4 | Implement Q&A audit logging (queries, chunks, responses, scores) | ⬜ |  |

## EPIC-005: Phase 5 - Bulk Import & Polish
**Priority:** P5 | **Progress:** 0/7

### FEAT-005-A: Bulk Import (0/3)

| ID | Task | Status | Est |
|----|------|--------|-----|
| TASK-005-A-1 | Create bulk import Hangfire job | ⬜ |  |
| TASK-005-A-2 | Create progress tracking dashboard | ⬜ |  |
| TASK-005-A-3 | Create error reporting for failed items | ⬜ |  |

### FEAT-005-B: Admin & Polish (0/4)

| ID | Task | Status | Est |
|----|------|--------|-----|
| TASK-005-B-1 | Create admin dashboard metrics (costs, rates, processing stats) | ⬜ |  |
| TASK-005-B-2 | Create reprocessing UI (re-run OCR, re-embed, re-categorize per item) | ⬜ |  |
| TASK-005-B-3 | Make UI mobile responsive | ⬜ |  |
| TASK-005-B-4 | Performance optimization (search, chunking, embedding, page load) | ⬜ |  |

## EPIC-006: Phase 6 - Production Deploy
**Priority:** P6 | **Progress:** 0/6

### FEAT-006-A: Production Setup (0/3)

| ID | Task | Status | Est |
|----|------|--------|-----|
| TASK-006-A-1 | Set up production PostgreSQL + pgvector | ⬜ |  |
| TASK-006-A-2 | Deploy to IIS alongside FRMv2 (securail.ca) | ⬜ |  |
| TASK-006-A-3 | Configure production settings (connection strings, API keys) | ⬜ |  |

### FEAT-006-B: Operations (0/3)

| ID | Task | Status | Est |
|----|------|--------|-----|
| TASK-006-B-1 | Document and test backup procedures (SQL Server, PostgreSQL, filesystem) | ⬜ |  |
| TASK-006-B-2 | Document and test restore procedures | ⬜ |  |
| TASK-006-B-3 | Set up monitoring and alerting | ⬜ |  |

---

## Completed

*No completed tasks yet.*
