# BriansLegacyV2

A private online library for railway structural engineering materials. Enables engineers to search, browse, and ask questions about books, documents, and plans using AI-powered cataloging and semantic search.

## Project Status

**Phase:** Planning Complete, Implementation Starting

## Tech Stack

- **Backend:** ASP.NET Core 9.0 + Razor Pages
- **Databases:**
  - SQL Server + EF Core 9.0 (metadata, users, jobs)
  - PostgreSQL + pgvector (vector search, embeddings)
- **Styling:** Tailwind CSS
- **Background Jobs:** Hangfire
- **AI:** Claude API + Gemini API

## Features (Planned)

- **AI-powered cataloging:** Upload book cover photos or PDFs, AI extracts metadata
- **Semantic search:** Find documents by meaning, not just keywords
- **Q&A:** Ask questions and get answers with citations from the library
- **Review queue:** Admin reviews AI extractions before publishing

## Documentation

- **Design Document:** `docs/plans/2026-01-18-brianslegacy-design.md`
- **Task Backlog:** `docs/BACKLOG.md`
- **Claude Instructions:** `CLAUDE.md`

## Development Setup

### Prerequisites

- .NET 9.0 SDK
- Docker Desktop
- SQL Server LocalDB
- Node.js (for Tailwind CSS)

### Quick Start

```bash
# Start PostgreSQL + pgvector
docker compose up -d

# Run migrations
dotnet ef database update

# Start the app
dotnet run --project src/BriansLegacy

# Tailwind CSS (in separate terminal)
npm run css:watch
```

### Environment Variables

```
ANTHROPIC_API_KEY=your_claude_api_key
GOOGLE_API_KEY=your_gemini_api_key
```

## Project Structure

```
BriansLegacyV2/
├── .claude/              ← Claude Code configuration
├── docs/
│   ├── plans/            ← Design documents
│   └── BACKLOG.md        ← Task backlog
├── src/
│   └── BriansLegacy/     ← Main application
├── docker-compose.yml    ← PostgreSQL + pgvector
├── CLAUDE.md             ← AI assistant instructions
└── README.md             ← This file
```

## License

Private - Internal Use Only
