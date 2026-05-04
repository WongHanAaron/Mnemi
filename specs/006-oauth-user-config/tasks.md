# Tasks: OAuth User Configuration and Document Source Management

**Input**: Design documents from `/specs/006-oauth-user-config/`  
**Prerequisites**: plan.md, spec.md, data-model.md, contracts/, research.md

**Tests**: Test tasks are included as this feature requires robust authentication and data persistence testing.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and adapter project creation

- [ ] T001 Create `src/Adapter.Persistence.Sqlite/src/Adapter.Persistence.Sqlite.csproj` project file with EF Core SQLite dependencies
- [ ] T002 Create `src/Adapter.Auth.OAuth/src/Adapter.Auth.OAuth.csproj` project file with ASP.NET Core Authentication dependencies
- [ ] T003 Add project references: Adapter.Persistence.Sqlite → Domain, Adapter.Auth.OAuth → Application
- [ ] T004 Update solution file to include new adapter projects
- [ ] T005 [P] Create `tests/Adapter.Persistence.Sqlite.Tests/Adapter.Persistence.Sqlite.Tests.csproj` test project
- [ ] T006 [P] Create `tests/Adapter.Auth.OAuth.Tests/Adapter.Auth.OAuth.Tests.csproj` test project

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core domain entities, persistence layer, and token encryption that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

### Domain Entities

- [ ] T007 [P] Create `OAuthProvider` enum in `src/Domain/Entity/OAuthProvider.cs`
- [ ] T008 [P] Create `DocumentSourceProvider` enum in `src/Domain/Entity/DocumentSourceProvider.cs`
- [ ] T009 Create `User` entity in `src/Domain/Entity/User.cs` with factory method and domain logic
- [ ] T010 Create `AuthConnection` entity in `src/Domain/Entity/AuthConnection.cs` with token management methods
- [ ] T011 Create `DocumentSourceConfig` entity in `src/Domain/Entity/DocumentSourceConfig.cs` with provider-specific factory methods
- [ ] T012 [P] Create `GoogleDriveProviderConfig` value object in `src/Domain/ValueObjects/GoogleDriveProviderConfig.cs`
- [ ] T013 [P] Create `GitHubProviderConfig` value object in `src/Domain/ValueObjects/GitHubProviderConfig.cs`

### Repository Interfaces (Application Layer)

- [ ] T014 Create `IUserRepository` interface in `src/Application/Ports/IUserRepository.cs`
- [ ] T015 Create `IAuthConnectionRepository` interface in `src/Application/Ports/IAuthConnectionRepository.cs`
- [ ] T016 Create `IDocumentSourceRepository` interface in `src/Application/Ports/IDocumentSourceRepository.cs`

### Persistence Implementation

- [ ] T017 Create EF Core entity configurations in `src/Adapter.Persistence.Sqlite/Entities/`
- [ ] T018 Create `MnemiDbContext` in `src/Adapter.Persistence.Sqlite/MnemiDbContext.cs` with DbSets and relationships
- [ ] T019 Create `UserRepository` in `src/Adapter.Persistence.Sqlite/Repositories/UserRepository.cs`
- [ ] T020 Create `AuthConnectionRepository` in `src/Adapter.Persistence.Sqlite/Repositories/AuthConnectionRepository.cs`
- [ ] T021 Create `DocumentSourceRepository` in `src/Adapter.Persistence.Sqlite/Repositories/DocumentSourceRepository.cs`
- [ ] T022 Create initial EF Core migration for User, AuthConnection, DocumentSource tables

### Token Encryption Service

- [ ] T023 Create `ITokenEncryptionService` interface in `src/Application/Ports/ITokenEncryptionService.cs`
- [ ] T024 Create `TokenEncryptionService` in `src/Adapter.Auth.OAuth/TokenEncryptionService.cs` using Data Protection APIs

### Unit Tests for Foundation

- [ ] T025 [P] Create `UserTests` in `tests/Domain.Tests/Entity/UserTests.cs` testing factory and domain methods
- [ ] T026 [P] Create `AuthConnectionTests` in `tests/Domain.Tests/Entity/AuthConnectionTests.cs`
- [ ] T027 [P] Create `DocumentSourceConfigTests` in `tests/Domain.Tests/Entity/DocumentSourceConfigTests.cs`
- [ ] T028 [P] Create `TokenEncryptionServiceTests` in `tests/Adapter.Auth.OAuth.Tests/TokenEncryptionServiceTests.cs`

**Checkpoint**: Foundation ready - domain entities, repositories, and token encryption are implemented and tested

---

## Phase 3: User Story 1 - Account Creation via OAuth (Priority: P1) 🎯 MVP

**Goal**: Users can sign up and sign in using Google or GitHub OAuth, with automatic account creation

**Independent Test**: A new user can click "Sign in with Google" or "Sign in with GitHub", complete the OAuth flow, and arrive at the Mnemi dashboard with a new account created automatically. Returning users are authenticated to their existing account.

### Tests for User Story 1

- [ ] T029 [P] [US1] Create integration test for Google OAuth callback in `tests/Adapter.Auth.OAuth.Tests/GoogleAuthHandlerTests.cs`
- [ ] T030 [P] [US1] Create integration test for GitHub OAuth callback in `tests/Adapter.Auth.OAuth.Tests/GitHubAuthHandlerTests.cs`
- [ ] T031 [P] [US1] Create test for duplicate account prevention in `tests/Application.Tests/Services/UserServiceTests.cs`

### Implementation for User Story 1

- [ ] T032 [P] [US1] Create `IAuthService` interface in `src/Application/Ports/IAuthService.cs`
- [ ] T033 [P] [US1] Create `UserService` in `src/Application/Services/UserService.cs` with account creation logic
- [ ] T034 [US1] Create `GoogleAuthHandler` in `src/Adapter.Auth.OAuth/GoogleAuthHandler.cs` with OAuth flow handling
- [ ] T035 [US1] Create `GitHubAuthHandler` in `src/Adapter.Auth.OAuth/GitHubAuthHandler.cs` with OAuth flow handling
- [ ] T036 [US1] Create `AuthController` in `src/Ui.Web/Controllers/AuthController.cs` with login and callback endpoints
- [ ] T037 [US1] Configure authentication middleware in `src/Ui.Web/Program.cs` (Google and GitHub handlers)
- [ ] T038 [US1] Create `LoginButton.razor` component in `src/Ui.Shared/Components/Auth/LoginButton.razor`
- [ ] T039 [US1] Create `LoginPage.razor` in `src/Ui.Web/Pages/LoginPage.razor` with OAuth provider buttons
- [ ] T040 [US1] Add error handling for OAuth failures with user-friendly messages

**Checkpoint**: User Story 1 complete - users can create accounts and sign in via OAuth

---

## Phase 4: User Story 2 - Linking Google Drive Document Sources (Priority: P1) 🎯 MVP

**Goal**: Authenticated users can browse their Google Drive and link folders as document sources

**Independent Test**: An authenticated user can navigate to "Add Document Source", browse their Google Drive folders, select one, and see it appear in their document sources list. The system validates access permissions.

### Tests for User Story 2

- [ ] T041 [P] [US2] Create integration test for Google Drive folder browsing in `tests/Adapter.Auth.OAuth.Tests/GoogleDriveBrowseTests.cs`
- [ ] T042 [P] [US2] Create test for document source creation in `tests/Application.Tests/Services/DocumentSourceServiceTests.cs`
- [ ] T043 [P] [US2] Create test for duplicate source prevention in `tests/Adapter.Persistence.Sqlite.Tests/DocumentSourceRepositoryTests.cs`

### Implementation for User Story 2

- [ ] T044 [P] [US2] Create `IDocumentSourceService` interface in `src/Application/Ports/IDocumentSourceService.cs`
- [ ] T045 [P] [US2] Create `DocumentSourceService` in `src/Application/Services/DocumentSourceService.cs`
- [ ] T046 [US2] Create `GoogleDriveBrowserService` in `src/Adapter.Auth.OAuth/GoogleDriveBrowserService.cs` for folder listing
- [ ] T047 [US2] Create `DocumentSourceController` in `src/Ui.Web/Controllers/DocumentSourceController.cs` with CRUD endpoints
- [ ] T048 [US2] Create `BrowseController` in `src/Ui.Web/Controllers/BrowseController.cs` with Google Drive folder listing endpoint
- [ ] T049 [US2] Create `GoogleDriveBrowser.razor` component in `src/Ui.Shared/Components/DocumentSources/GoogleDriveBrowser.razor`
- [ ] T050 [US2] Create `AddDocumentSourceDialog.razor` in `src/Ui.Shared/Components/DocumentSources/AddDocumentSourceDialog.razor`
- [ ] T051 [US2] Add permission validation when linking Google Drive folders

**Checkpoint**: User Story 2 complete - users can link Google Drive folders as document sources

---

## Phase 5: User Story 3 - Linking GitHub Repository Document Sources (Priority: P1) 🎯 MVP

**Goal**: Authenticated users can browse their GitHub repositories and link them (with optional subpaths) as document sources

**Independent Test**: An authenticated user can navigate to "Add Document Source", browse their GitHub repositories, select one with an optional path, and see it appear in their document sources list.

### Tests for User Story 3

- [ ] T052 [P] [US3] Create integration test for GitHub repository browsing in `tests/Adapter.Auth.OAuth.Tests/GitHubBrowseTests.cs`
- [ ] T053 [P] [US3] Create test for GitHub document source creation in `tests/Application.Tests/Services/DocumentSourceServiceTests.cs`

### Implementation for User Story 3

- [ ] T054 [P] [US3] Create `GitHubBrowserService` in `src/Adapter.Auth.OAuth/GitHubBrowserService.cs` for repository listing
- [ ] T055 [US3] Add GitHub repository listing endpoint to `BrowseController`
- [ ] T056 [US3] Add GitHub document source creation endpoint to `DocumentSourceController`
- [ ] T057 [US3] Create `GitHubRepoBrowser.razor` component in `src/Ui.Shared/Components/DocumentSources/GitHubRepoBrowser.razor`
- [ ] T058 [US3] Update `AddDocumentSourceDialog.razor` to support GitHub provider selection
- [ ] T059 [US3] Add permission validation when linking GitHub repositories

**Checkpoint**: User Story 3 complete - users can link GitHub repositories as document sources

---

## Phase 6: User Story 4 - Managing Multiple Document Sources (Priority: P2)

**Goal**: Users can view, edit display names, and remove their linked document sources

**Independent Test**: A user can view all their linked sources in a settings page, rename a source, or remove it. Removed sources no longer appear in deck enumeration.

### Tests for User Story 4

- [ ] T060 [P] [US4] Create test for document source update in `tests/Application.Tests/Services/DocumentSourceServiceTests.cs`
- [ ] T061 [P] [US4] Create test for document source deletion in `tests/Application.Tests/Services/DocumentSourceServiceTests.cs`

### Implementation for User Story 4

- [ ] T062 [P] [US4] Add update display name method to `DocumentSourceService`
- [ ] T063 [P] [US4] Add delete method to `DocumentSourceService`
- [ ] T064 [US4] Add PATCH endpoint for updating document source to `DocumentSourceController`
- [ ] T065 [US4] Add DELETE endpoint for removing document source to `DocumentSourceController`
- [ ] T066 [US4] Create `DocumentSourceList.razor` component in `src/Ui.Shared/Components/DocumentSources/DocumentSourceList.razor`
- [ ] T067 [US4] Create `DocumentSourcesPage.razor` in `src/Ui.Web/Pages/DocumentSourcesPage.razor`
- [ ] T068 [US4] Create `EditDocumentSourceDialog.razor` in `src/Ui.Shared/Components/DocumentSources/EditDocumentSourceDialog.razor`

**Checkpoint**: User Story 4 complete - users can manage their document sources

---

## Phase 7: User Story 5 - Login and Session Management (Priority: P2)

**Goal**: Users have persistent sessions with secure cookies and can sign out

**Independent Test**: A user can sign in, close the browser, reopen it, and still be authenticated. They can explicitly log out, which clears their session.

### Tests for User Story 5

- [ ] T069 [P] [US5] Create test for session persistence in `tests/Ui.E2E.Playwright/AuthFlowTests.cs`
- [ ] T070 [P] [US5] Create test for sign out functionality in `tests/Ui.E2E.Playwright/AuthFlowTests.cs`

### Implementation for User Story 5

- [ ] T071 [P] [US5] Configure cookie authentication in `src/Ui.Web/Program.cs` with HTTP-only, Secure, SameSite=Strict settings
- [ ] T072 [P] [US5] Create `IAuthStateProvider` interface in `src/Ui.Shared/Services/IAuthStateProvider.cs`
- [ ] T073 [P] [US5] Create `WebAuthStateProvider` in `src/Ui.Web/Services/WebAuthStateProvider.cs`
- [ ] T074 [US5] Add logout endpoint to `AuthController`
- [ ] T075 [US5] Create `UserMenu.razor` component in `src/Ui.Shared/Components/Auth/UserMenu.razor` with sign out button
- [ ] T076 [US5] Create `AuthGuard.razor` component in `src/Ui.Shared/Components/Auth/AuthGuard.razor` for protected routes
- [ ] T077 [US5] Add session expiration handling with redirect to login

**Checkpoint**: User Story 5 complete - session management and sign out working

---

## Phase 8: User Story 6 - Linking Additional OAuth Providers (Priority: P3)

**Goal**: Users can link additional OAuth providers to their existing account

**Independent Test**: A user signed up with Google can link their GitHub account and then add GitHub repositories as document sources.

### Tests for User Story 6

- [ ] T078 [P] [US6] Create test for linking additional provider in `tests/Application.Tests/Services/UserServiceTests.cs`
- [ ] T079 [P] [US6] Create test for preventing cross-user provider linking in `tests/Application.Tests/Services/UserServiceTests.cs`

### Implementation for User Story 6

- [ ] T080 [P] [US6] Add link provider method to `UserService`
- [ ] T081 [P] [US6] Add unlink provider method to `UserService` with validation (prevent removing last connection)
- [ ] T082 [US6] Add link provider endpoint to `AuthController`
- [ ] T083 [US6] Add unlink provider endpoint to `AuthController`
- [ ] T084 [US6] Create `AccountSettingsPage.razor` in `src/Ui.Web/Pages/AccountSettingsPage.razor`
- [ ] T085 [US6] Create `LinkedProvidersList.razor` in `src/Ui.Shared/Components/Auth/LinkedProvidersList.razor`
- [ ] T086 [US6] Add error handling for linking provider already associated with another user

**Checkpoint**: User Story 6 complete - users can link multiple OAuth providers

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Security hardening, rate limiting, error handling, and UI polish

### Security & Rate Limiting

- [ ] T087 [P] Implement rate limiting middleware in `src/Ui.Web/Program.cs` for auth endpoints (5 req/min for login, 10 req/min for callbacks)
- [ ] T088 [P] Add PKCE support to OAuth flows in `GoogleAuthHandler` and `GitHubAuthHandler`
- [ ] T089 Add token refresh logic in `AuthConnection` when access tokens expire
- [ ] T090 Add security headers middleware (HSTS, X-Frame-Options, CSP) in `src/Ui.Web/Program.cs`

### Error Handling & Logging

- [ ] T091 [P] Create global exception handler middleware in `src/Ui.Web/Middleware/ExceptionHandlerMiddleware.cs`
- [ ] T092 [P] Add structured logging for authentication events in `AuthController`
- [ ] T093 Create user-friendly error pages for OAuth failures in `src/Ui.Web/Pages/AuthError.razor`

### UI Polish

- [ ] T094 [P] Add loading states to `LoginButton.razor` and `AddDocumentSourceDialog.razor`
- [ ] T095 [P] Add empty state UI to `DocumentSourceList.razor` when no sources linked
- [ ] T096 Add accessibility attributes (ARIA labels) to all auth and document source components
- [ ] T097 Add responsive design for mobile to auth and document source pages

### E2E Tests

- [ ] T098 [P] Create complete OAuth flow E2E test in `tests/Ui.E2E.Playwright/CompleteAuthFlowTests.cs`
- [ ] T099 [P] Create document source management E2E test in `tests/Ui.E2E.Playwright/DocumentSourceManagementTests.cs`

### Documentation

- [ ] T100 Update `docs/dev/authentication.md` with OAuth implementation details
- [ ] T101 Update `docs/users/getting-started.md` with account creation and document source linking guide

**Checkpoint**: Feature complete - all user stories implemented with security, tests, and documentation

---

## Dependencies

### Story Completion Order

```
Phase 2 (Foundation)
    ↓
US1 (Account Creation) ──┬──→ US2 (Google Drive Sources)
    ↓                    │
US5 (Session Mgmt) ──────┼──→ US3 (GitHub Sources)
    ↓                    │
US6 (Link Providers) ────┘    ↓
                         US4 (Manage Sources)
                              ↓
                         Phase 9 (Polish)
```

### Parallel Execution Opportunities

**Within each phase, tasks marked with [P] can run in parallel.**

**Cross-story parallels** (after foundation is complete):
- US2 (Google Drive) and US3 (GitHub) can be developed in parallel
- US5 (Session Management) can be developed in parallel with US2/US3
- US4 (Manage Sources) depends on both US2 and US3
- US6 (Link Providers) can be developed in parallel with US2-US5

### Critical Path

1. **Foundation** (T007-T028) → Blocks everything
2. **US1** (T032-T040) → Blocks US2, US3, US5, US6
3. **US2 + US3** (T044-T059) → Can run in parallel, both block US4
4. **US4** (T062-T068) → Depends on US2 and US3
5. **US5** (T071-T077) → Can run parallel with US2-US4
6. **US6** (T080-T086) → Can run parallel with US2-US5
7. **Polish** (T087-T101) → Final phase

---

## Implementation Strategy

### MVP Scope

**Minimum Viable Product**: Complete Phase 2 (Foundation) + US1 (Account Creation) + US2 (Google Drive Sources)

With this scope:
- Users can create accounts via Google OAuth
- Users can link Google Drive folders
- Users can view and study flashcards from linked sources
- Basic session management works

### Incremental Delivery

1. **Sprint 1**: Foundation + US1 (Account Creation)
   - Deliverable: Users can sign up and sign in

2. **Sprint 2**: US2 (Google Drive Sources)
   - Deliverable: Users can link Google Drive folders and see content

3. **Sprint 3**: US3 (GitHub Sources) + US5 (Session Management)
   - Deliverable: Users can link GitHub repos, sessions persist

4. **Sprint 4**: US4 (Manage Sources) + US6 (Link Providers)
   - Deliverable: Full source management, multiple providers

5. **Sprint 5**: Polish + Documentation
   - Deliverable: Production-ready feature with security hardening

---

## Task Count Summary

| Phase | Tasks | Story |
|-------|-------|-------|
| Phase 1: Setup | 6 | - |
| Phase 2: Foundation | 22 | - |
| Phase 3: US1 | 12 | Account Creation (P1) |
| Phase 4: US2 | 11 | Google Drive Sources (P1) |
| Phase 5: US3 | 9 | GitHub Sources (P1) |
| Phase 6: US4 | 9 | Manage Sources (P2) |
| Phase 7: US5 | 9 | Session Management (P2) |
| Phase 8: US6 | 9 | Link Providers (P3) |
| Phase 9: Polish | 15 | Cross-cutting |
| **Total** | **102** | - |

**Parallelizable Tasks**: ~45% (46 tasks marked with [P])

**Estimated Effort**: 4-5 sprints (2-3 weeks per sprint with 1-2 developers)
