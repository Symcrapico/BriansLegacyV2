# Backlog

> Updated: 2026-01-18T15:30:00.000Z
> Active: None

---

## EPIC-001: Phase 1 - Foundation
**Priority:** P1 | **Progress:** 0/11

### FEAT-001-A: Project Scaffolding (0/3)

| ID | Task | Status | Est |
|----|------|--------|-----|
| TASK-001-A-1 | Create ASP.NET Core 9.0 + Razor Pages project with solution structure matching FRMv2 patterns | 🟦 Ready | 30m |
| TASK-001-A-2 | Set up Tailwind CSS with build scripts (css:build, css:watch) | 🟦 Ready | 20m |
| TASK-001-A-3 | Create basic layout (_Layout.cshtml) and navigation | 🟦 Ready | 30m |

### FEAT-001-B: Database Setup (0/4)

| ID | Task | Status | Est |
|----|------|--------|-----|
| TASK-001-B-1 | Set up SQL Server + EF Core 9.0 with ApplicationDbContext | ⬜ Backlog | 30m |
| TASK-001-B-2 | Create initial EF Core migration with all entities (LibraryItem, BookDetails, DocumentDetails, PlanDetails, LibraryFile, FileDerivative, ExtractedPageText, Category, Tag, ProcessingState, ProcessingLog, ReviewQueueItem) | ⬜ Backlog | 45m |
| TASK-001-B-3 | Add all uniqueness constraints (ContentHash, FileDerivative composite, Category name, Tag name, ProcessingState) | ⬜ Backlog | 20m |
| TASK-001-B-4 | Create docker-compose.yml for PostgreSQL + pgvector | ⬜ Backlog | 20m |

### FEAT-001-C: Infrastructure (0/4)

| ID | Task | Status | Est |
|----|------|--------|-----|
| TASK-001-C-1 | Set up Hangfire with SQL Server storage (single queue) | ⬜ Backlog | 30m |
| TASK-001-C-2 | Set up ASP.NET Core Identity with Admin/Viewer roles | ⬜ Backlog | 30m |
| TASK-001-C-3 | Create secure file serving endpoint (/files/{fileId}) with authorization | ⬜ Backlog | 30m |
| TASK-001-C-4 | Create health endpoint (/health) with full dependency validation (SQL, Postgres, filesystem, Hangfire) | ⬜ Backlog | 30m |

---

## EPIC-002: Phase 2 - Core Library
**Priority:** P2 | **Progress:** 0/11

### FEAT-002-A: File Management (0/3)

| ID | Task | Status | Est |
|----|------|--------|-----|
| TASK-002-A-1 | Create FileStorageService with hash-based duplicate detection | ⬜ Backlog | 45m |
| TASK-002-A-2 | Create FileDerivative tracking service | ⬜ Backlog | 30m |
| TASK-002-A-3 | Create ExtractedPageText storage service | ⬜ Backlog | 30m |

### FEAT-002-B: Manual CRUD (0/5)

| ID | Task | Status | Est |
|----|------|--------|-----|
| TASK-002-B-1 | Create manual metadata entry forms for Books | ⬜ Backlog | 45m |
| TASK-002-B-2 | Create manual metadata entry forms for Documents | ⬜ Backlog | 45m |
| TASK-002-B-3 | Create CompletenessScore calculation service | ⬜ Backlog | 20m |
| TASK-002-B-4 | Create Category/Tag management admin pages | ⬜ Backlog | 45m |
| TASK-002-B-5 | Seed initial category taxonomy from design | ⬜ Backlog | 20m |

### FEAT-002-C: Browse & Search (0/3)

| ID | Task | Status | Est |
|----|------|--------|-----|
| TASK-002-C-1 | Create browse by category page | ⬜ Backlog | 30m |
| TASK-002-C-2 | Create basic keyword search with SQL full-text and field boosting | ⬜ Backlog | 45m |
| TASK-002-C-3 | Create item detail pages with PDF viewer | ⬜ Backlog | 45m |

---

## EPIC-003: Phase 3 - AI Integration
**Priority:** P3 | **Progress:** 0/0

### FEAT-003-A: AI Services (0/0)

| ID | Task | Status | Est |
|----|------|--------|-----|

### FEAT-003-B: OCR Pipeline (0/0)

| ID | Task | Status | Est |
|----|------|--------|-----|

### FEAT-003-C: Processing Jobs (0/0)

| ID | Task | Status | Est |
|----|------|--------|-----|

---

## EPIC-004: Phase 4 - Search & Q&A
**Priority:** P4 | **Progress:** 0/0

### FEAT-004-A: Vector Search (0/0)

| ID | Task | Status | Est |
|----|------|--------|-----|

### FEAT-004-B: RAG Q&A (0/0)

| ID | Task | Status | Est |
|----|------|--------|-----|

---

## Completed

| ID | Task | Completed | Commit |
|----|------|-----------|--------|
