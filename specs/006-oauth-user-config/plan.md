# Implementation Plan: OAuth User Configuration and Document Source Management

**Branch**: `006-oauth-user-config` | **Date**: 2026-05-02 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/006-oauth-user-config/spec.md`

## Summary

Implement OAuth-based user authentication with Google and GitHub, allowing users to create accounts and link multiple document sources (Google Drive folders and GitHub repositories) to retrieve flashcard content. The system will use a thin backend database for user configuration while keeping all flashcard content and study progress in external storage.

**Technical Approach**:
- ASP.NET Core backend with OAuth 2.0 authentication handlers for Google and GitHub
- SQLite database for user accounts, auth connections, and document source configuration
- AES-256 encryption for OAuth token storage at rest
- Blazor WebAssembly frontend with authentication state management
- Cookie-based sessions with HTTP-only, Secure, SameSite=Strict settings
- Integration with existing `Adapter.Source.GoogleDrive` and future GitHub adapter

## Technical Context

**Language/Version**: C# 12 / .NET 8.0  
**Primary Dependencies**: 
- ASP.NET Core Authentication (Microsoft.AspNetCore.Authentication.Google, Microsoft.AspNetCore.Authentication.GitHub)
- Entity Framework Core with SQLite
- Microsoft.AspNetCore.DataProtection for token encryption
- Blazor WebAssembly Authentication
**Storage**: SQLite database for user/auth configuration; external storage (Google Drive/GitHub) for content
**Testing**: xUnit with WebApplicationFactory for integration tests, bUnit for Blazor component tests
**Target Platform**: Web (Blazor WebAssembly), cross-platform via shared UI components
**Project Type**: Web application with shared UI components for future MAUI Blazor
**Performance Goals**: OAuth flows complete within 30 seconds; login page load < 2 seconds
**Constraints**: Tokens encrypted at rest; session cookies HTTP-only/Secure/SameSite=Strict; rate limiting on auth endpoints
**Scale/Scope**: Single-user focused; support for multiple document sources per user; no team/organization features

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

The constitution file is a template and has not been ratified with specific principles. Based on the project structure and existing code:

✅ **Clean Architecture**: Following existing pattern with Domain, Application, Adapter, and UI layers  
✅ **Test-First**: Existing test projects for all components  
✅ **Separation of Concerns**: Auth logic in Application/Domain, UI in Shared/Web/Maui  
✅ **Shared Components**: UI components in Ui.Shared for Web and MAUI reuse  

**No violations identified** - proceeding with established project patterns.

## Project Structure

### Documentation (this feature)

```text
specs/006-oauth-user-config/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
└── tasks.md             # Phase 2 output (created by /speckit.tasks)
```

### Source Code (repository root)

Following the existing clean architecture pattern:

```text
src/
├── Domain/                          # Core entities and value objects
│   └── Entity/
│       ├── User.cs                  # User aggregate root
│       ├── AuthConnection.cs        # OAuth connection entity
│       └── DocumentSourceConfig.cs  # Document source configuration
├── Application/                     # Use cases and ports
│   ├── Ports/
│   │   ├── IAuthService.cs          # Authentication operations
│   │   ├── IUserRepository.cs       # User persistence
│   │   └── IDocumentSourceRepository.cs
│   └── Services/
│       └── UserService.cs           # User management logic
├── Adapter.Auth.OAuth/              # NEW: OAuth adapter
│   └── src/
│       ├── GoogleAuthHandler.cs
│       ├── GitHubAuthHandler.cs
│       └── TokenEncryptionService.cs
├── Adapter.Persistence.Sqlite/      # NEW: SQLite persistence
│   └── src/
│       ├── Entities/                # EF Core entities
│       ├── Repositories/
│       └── MnemiDbContext.cs
├── Ui.Shared/                       # Shared Blazor components
│   ├── Components/
│   │   ├── Auth/
│   │   │   ├── LoginButton.razor    # OAuth provider buttons
│   │   │   ├── UserMenu.razor       # User dropdown menu
│   │   │   └── AuthGuard.razor      # Protected route wrapper
│   │   └── DocumentSources/
│   │       ├── DocumentSourceList.razor
│   │       ├── AddDocumentSourceDialog.razor
│   │       └── GoogleDriveBrowser.razor
│   └── Services/
│       └── IAuthStateProvider.cs
├── Ui.Web/                          # Web host
│   ├── Program.cs                   # Auth middleware registration
│   └── Services/
│       └── WebAuthStateProvider.cs
└── Ui.Maui/                         # MAUI host (future)
    └── Services/
        └── MauiAuthStateProvider.cs

tests/
├── Domain.Tests/
├── Application.Tests/
├── Adapter.Auth.OAuth.Tests/        # NEW
├── Adapter.Persistence.Sqlite.Tests/# NEW
├── Ui.Shared.Tests/
└── Ui.E2E.Playwright/               # Auth flow E2E tests
```

**Structure Decision**: Following the established clean architecture with Domain, Application, Adapter, and UI layers. New adapters added for OAuth authentication (`Adapter.Auth.OAuth`) and SQLite persistence (`Adapter.Persistence.Sqlite`). UI components added to `Ui.Shared` for reuse across Web and MAUI platforms.

## Complexity Tracking

No violations identified. The architecture follows existing project patterns with clear separation of concerns.
