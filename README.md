# BriansLegacyV2

A private online library for railway structural engineering materials. Enables engineers to search, browse, and ask questions about books, documents, and plans using AI-powered cataloging and semantic search.

## Project Status

**Current Phase:** Phase 2 - Core Library (in progress)

### Completed
- Phase 1: Foundation (Project scaffolding, database setup, infrastructure)

### In Progress
- Phase 2: Core Library (File management, manual CRUD, browse/search)

## Tech Stack

- **Backend:** ASP.NET Core 9.0 + Razor Pages
- **Databases:**
  - SQL Server + EF Core 9.0 (metadata, users, jobs)
  - PostgreSQL + pgvector (vector search, embeddings)
- **Styling:** Tailwind CSS
- **Background Jobs:** Hangfire
- **AI:** Claude API + Gemini API

## Implemented Features

### Authentication & Authorization
- Google OAuth authentication with email whitelist
- Role-based access: Admin and Viewer roles
- 30-minute session timeout with sliding expiration

### File Management
- Secure file serving at `/files/{fileId}` (download) and `/files/{fileId}/view` (inline)
- Path traversal prevention in FileStorageService
- SHA256 hash-based duplicate detection
- Organized storage: `originals/yyyy/MM/guid.ext`

### Background Jobs
- Hangfire with SQL Server storage
- Admin dashboard at `/admin/jobs` (Admin role only)

### Health Monitoring
- `/health` endpoint with JSON response
- Validates: SQL Server, PostgreSQL, Hangfire, Filesystem

### Data Model
- LibraryItem base entity with TPH inheritance (Book, Document, Plan)
- LibraryFile with ContentHash for duplicate detection
- FileDerivative for processed outputs
- ExtractedPageText for page-level OCR storage
- Category (hierarchical) and Tag (flat) taxonomies
- ProcessingState and ProcessingLog for job tracking
- ReviewQueueItem for admin review workflow

## Planned Features

- AI-powered cataloging: Upload book cover photos or PDFs, AI extracts metadata
- Semantic search: Find documents by meaning, not just keywords
- Q&A with RAG: Ask questions and get answers with citations from the library
- Review queue: Admin reviews AI extractions before publishing

## Documentation

- **Design Document:** `docs/plans/2026-01-18-brianslegacy-design.md`
- **Task Backlog:** `docs/BACKLOG.md`
- **Claude Instructions:** `CLAUDE.md`

## Development Setup

### Prerequisites

- .NET 9.0 SDK
- Docker Desktop
- SQL Server (LocalDB or container)
- Node.js (for Tailwind CSS)

### Quick Start

```bash
# Start PostgreSQL + pgvector
docker compose up -d

# Run migrations
dotnet ef database update --project src/BriansLegacy

# Start the app
dotnet run --project src/BriansLegacy

# Tailwind CSS watch (separate terminal)
cd src/BriansLegacy && npm run css:watch
```

### Environment Variables / User Secrets

```bash
# Set up user secrets for development
dotnet user-secrets set "Authentication:Google:ClientId" "your-client-id" --project src/BriansLegacy
dotnet user-secrets set "Authentication:Google:ClientSecret" "your-secret" --project src/BriansLegacy
```

Configuration in `appsettings.json`:
- `AllowedEmails` - Array of authorized email addresses
- `AdminEmails` - Array of admin email addresses
- `FileStorage:BasePath` - File storage location (default: `app_data`)

## API Endpoints

| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/` | GET | None | Home page |
| `/login` | GET | None | Initiates Google OAuth |
| `/logout` | GET | Any | Signs out user |
| `/files/{fileId}` | GET | Viewer+ | Download file |
| `/files/{fileId}/view` | GET | Viewer+ | View file inline (PDF viewer) |
| `/health` | GET | None | Health check (JSON) |
| `/admin/jobs` | GET | Admin | Hangfire dashboard |

## Project Structure

```
BriansLegacyV2/
├── .claude/              <- Claude Code configuration
│   ├── agents/           <- Subagent instruction files
│   ├── commands/         <- Custom slash commands
│   └── docs/             <- Policies and workflows
├── docs/
│   ├── plans/            <- Design documents
│   ├── tasks/            <- Task detail files
│   └── BACKLOG.md        <- Task backlog
├── src/
│   └── BriansLegacy/     <- Main application
│       ├── Data/         <- DbContext and migrations
│       ├── Infrastructure/ <- Filters, health checks
│       ├── Models/       <- Domain entities
│       ├── Pages/        <- Razor Pages
│       ├── Services/     <- Business logic
│       └── wwwroot/      <- Static assets (CSS, JS)
├── docker-compose.yml    <- PostgreSQL + pgvector
├── CLAUDE.md             <- AI assistant instructions
└── README.md             <- This file
```

## License

Private - Internal Use Only
