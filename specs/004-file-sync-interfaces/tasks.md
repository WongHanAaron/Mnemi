# Implementation Tasks: File Synchronization Interfaces

**Branch**: `004-read-file-sync` | **Date**: May 2, 2026  
**Feature**: File Synchronization Interfaces (specs/004-file-sync-interfaces/)  
**Generated From**: Specification, Data Model, Research, and Interface Contracts

---

## Overview

This document contains all implementation tasks required to deliver the file synchronization interfaces feature. Tasks are organized by phase and user story to enable independent, parallel development where possible.

**Total Tasks**: 48  
**MVP Scope**: Phase 1 (Setup) + Phase 2 (Foundational) + Phase 3 (US1 & US2)  
**Estimated Duration**: 7-10 days (2 developers)

---

## Phase 1: Setup & Project Initialization

> Projects must be created before any source code can be implemented

### Create Project Files

- [X] T001 Create `src/Adapter.Source.FileSystem/src/Adapter.Source.FileSystem.csproj` with .NET 8 targeting and xUnit test framework reference
- [X] T002 Create `src/Adapter.Source.GoogleDrive/src/Adapter.Source.GoogleDrive.csproj` with Google.Apis.Drive.v3 NuGet dependency
- [X] T003 Create `tests/Adapter.Source.FileSystem.Tests/Adapter.Source.FileSystem.Tests.csproj` with xUnit and Moq dependencies
- [X] T004 Create `tests/Adapter.Source.GoogleDrive.Tests/Adapter.Source.GoogleDrive.Tests.csproj` with xUnit and Moq dependencies
- [X] T005 Update `Mnemi.sln` to include all four new projects

### Create Directory Structure

- [X] T006 [P] Create `src/Adapter.Source.FileSystem/src/` directory structure
- [X] T007 [P] Create `src/Adapter.Source.GoogleDrive/src/` directory structure
- [X] T008 [P] Create `tests/Adapter.Source.FileSystem.Tests/` directory structure
- [X] T009 [P] Create `tests/Adapter.Source.GoogleDrive.Tests/` directory structure

---

## Phase 2: Foundational - Core Interfaces & Models

> These must be completed before adapter implementations can begin

### Application Layer - Enumerations

- [X] T010 Create `src/Application/Ports/DocumentChangeType.cs` enum with Created, Modified, Deleted values
- [X] T011 Create `src/Application/Ports/DocumentSourceAdapterType.cs` enum with FileSystem, GoogleDrive values

### Application Layer - Records & Classes

- [X] T012 Create `src/Application/Ports/DocumentContent.cs` record with Id, Content, LastModified, Encoding properties per data-model.md
- [X] T013 Create `src/Application/Ports/DocumentChange.cs` record with ChangeType, Document, DetectedAt properties per data-model.md
- [X] T014 Create `src/Application/Ports/DocumentSourceConfig.cs` class with AdapterType, SourceId, AdapterSpecificConfig properties

### Application Layer - Exception Hierarchy

- [X] T015 Create `src/Application/Ports/Exceptions/DocumentSourceException.cs` base exception class with SourceId and AdapterType context
- [X] T016 [P] Create `src/Application/Ports/Exceptions/DocumentSourceConfigurationException.cs` derived exception
- [X] T017 [P] Create `src/Application/Ports/Exceptions/DocumentSourceAuthenticationException.cs` derived exception
- [X] T018 [P] Create `src/Application/Ports/Exceptions/DocumentSourceAccessException.cs` derived exception
- [X] T019 [P] Create `src/Application/Ports/Exceptions/DocumentSourceTransientException.cs` derived exception with RetryAfterSeconds

### Application Layer - Core Interface

- [X] T020 Create `src/Application/Ports/IDocumentSource.cs` interface with:
  - `ReadAllAsync(CancellationToken)` → `IAsyncEnumerable<DocumentContent>`
  - `SubscribeToChangesAsync(CancellationToken)` → `IAsyncEnumerable<DocumentChange>`
  - XML documentation per idocument-source-contract.md

### Update Application Project

- [X] T021 Update `src/Application/Application.csproj` to include new Ports folder in compilation

---

## Phase 3: User Story 1 - Read File Content from Directory (P1)

### File System Adapter - Core Implementation

- [X] T022 [P] [US1] Create `src/Adapter.Source.FileSystem/src/FileSystemDocumentSource.cs` implementing `IDocumentSource` with:
  - Constructor accepting `DocumentSourceConfig`
  - Config validation (DirectoryPath required, SearchPattern default to "*.md")
  - `ReadAllAsync()` using `Directory.EnumerateFiles()`
  - `SubscribeToChangesAsync()` stub returning empty enumerable
- [X] T023 [P] [US1] Create `src/Adapter.Source.FileSystem/src/FileSystemConfigValidator.cs` to validate adapter-specific config parameters

### File System Adapter - File Reading Logic

- [X] T024 [P] [US1] Create `src/Adapter.Source.FileSystem/src/FileContentReader.cs` with:
  - `ReadFileAsync(filePath, cancellationToken)` method
  - Encoding detection (BOM or UTF-8 default)
  - Exception handling for access denied, file not found
- [X] T025 [P] [US1] Implement `FileSystemDocumentSource.ReadAllAsync()` to:
  - Enumerate files in directory with extension filter
  - Call `FileContentReader` for each file
  - Yield `DocumentContent` with proper encoding
  - Respect `CancellationToken`

### File System Adapter - Unit Tests

- [X] T026 [US1] Create `tests/Adapter.Source.FileSystem.Tests/FileSystemDocumentSourceReadAllTests.cs` with:
  - Test: ReadAllAsync returns all matching files from temp directory
  - Test: ReadAllAsync respects extension filter (*.md only)
  - Test: ReadAllAsync throws on invalid directory path
  - Test: ReadAllAsync throws on permission denied
  - Test: ReadAllAsync respects CancellationToken
  - Test: ReadAllAsync returns empty for empty directory
  - Test: Encoding is correctly preserved/detected
- [X] T027 [US1] Create `tests/Adapter.Source.FileSystem.Tests/FileContentReaderTests.cs` with:
  - Test: Reads UTF-8 file correctly
  - Test: Detects BOM encoding (UTF-16, UTF-8)
  - Test: Handles missing file exception
  - Test: Handles access denied exception
  - Test: Preserves line endings (CRLF vs LF)

### Google Drive Adapter - Core Implementation

- [X] T028 [P] [US1] Create `src/Adapter.Source.GoogleDrive/src/GoogleDriveDocumentSource.cs` implementing `IDocumentSource` with:
  - Constructor accepting `DocumentSourceConfig`
  - Config validation (FolderId required, AuthToken required)
  - `ReadAllAsync()` using Google Drive API
  - `SubscribeToChangesAsync()` stub returning empty enumerable
- [X] T029 [P] [US1] Create `src/Adapter.Source.GoogleDrive/src/GoogleDriveConfigValidator.cs` to validate adapter-specific config

### Google Drive Adapter - API Wrapper

- [X] T030 [P] [US1] Create `src/Adapter.Source.GoogleDrive/src/GoogleDriveClient.cs` wrapping `DriveService` with:
  - Constructor accepting `DriveService`
  - `ListFilesInFolderAsync(folderId, cancellationToken)` method using pagination
  - `DownloadFileAsStringAsync(fileId, cancellationToken)` method
  - Exception mapping to `DocumentSourceException` hierarchy
  - Proper handling of Google API quota limits
- [X] T031 [P] [US1] Implement `GoogleDriveDocumentSource.ReadAllAsync()` to:
  - Call `GoogleDriveClient.ListFilesInFolderAsync()`
  - Paginate through results (pageSize=100)
  - Call `DownloadFileAsStringAsync()` for each file
  - Yield `DocumentContent` with encoding (always UTF-8 for Google Drive)
  - Respect `CancellationToken`

### Google Drive Adapter - Unit Tests

- [X] T032 [US1] Create `tests/Adapter.Source.GoogleDrive.Tests/GoogleDriveDocumentSourceReadAllTests.cs` with mock `DriveService`:
  - Test: ReadAllAsync returns all files from mocked folder
  - Test: ReadAllAsync handles pagination correctly (>100 files)
  - Test: ReadAllAsync throws on invalid folder ID
  - Test: ReadAllAsync throws on authentication failure (invalid token)
  - Test: ReadAllAsync respects CancellationToken
  - Test: ReadAllAsync returns empty for empty folder
- [X] T033 [US1] Create `tests/Adapter.Source.GoogleDrive.Tests/GoogleDriveClientTests.cs` with mocked API responses:
  - Test: ListFilesInFolderAsync parses response correctly
  - Test: DownloadFileAsStringAsync retrieves content
  - Test: API quota errors are handled gracefully
  - Test: Network timeout throws DocumentSourceTransientException

---

## Phase 4: User Story 2 - Subscribe to Directory Changes (P1)

### File System Adapter - Change Detection

- [X] T034 [P] [US2] Create `src/Adapter.Source.FileSystem/src/FileSystemWatcherService.cs` with:
  - Wraps `FileSystemWatcher` lifecycle
  - Debounces rapid changes (100-200ms window)
  - Converts `FileSystemEventArgs` to `DocumentChange`
  - Handles Created, Modified, Deleted events
  - Thread-safe event collection
- [X] T035 [P] [US2] Implement `FileSystemDocumentSource.SubscribeToChangesAsync()` to:
  - Initialize `FileSystemWatcherService`
  - Yield change events as `DocumentChange` records
  - Include full document content for Created/Modified
  - Include empty content for Deleted (with Id preserved)
  - Respect `CancellationToken` and clean up watchers
  - Support multiple concurrent subscribers

### File System Adapter - Change Detection Tests

- [X] T036 [US2] Create `tests/Adapter.Source.FileSystem.Tests/FileSystemDocumentSourceSubscribeTests.cs` with:
  - Test: SubscribeToChanges yields Created event when file is added
  - Test: SubscribeToChanges yields Modified event when file is updated
  - Test: SubscribeToChanges yields Deleted event when file is removed
  - Test: Event includes full document content for Created/Modified
  - Test: Event has empty content for Deleted
  - Test: Multiple subscribers on same source receive same events
  - Test: SubscribeToChanges respects CancellationToken
  - Test: Rapid consecutive changes are debounced (no duplicates)
  - Test: Events are yielded within <1 second of file operation
- [X] T037 [US2] Create `tests/Adapter.Source.FileSystem.Tests/FileSystemWatcherServiceTests.cs` with:
  - Test: Service detects file creation
  - Test: Service detects file modification
  - Test: Service detects file deletion
  - Test: Service debounces rapid changes
  - Test: Service cleanup on cancellation

### Google Drive Adapter - Change Detection via Polling

- [X] T038 [P] [US2] Create `src/Adapter.Source.GoogleDrive/src/GoogleDriveWatcherService.cs` with:
  - Polls folder using configured interval (default 5000ms, configurable)
  - Tracks file snapshot (Id → LastModified) between polls
  - Detects Created (new Id), Modified (LastModified changed), Deleted (Id missing)
  - Converts changes to `DocumentChange` records
  - Exponential backoff for transient errors
- [X] T039 [P] [US2] Implement `GoogleDriveDocumentSource.SubscribeToChangesAsync()` to:
  - Initialize `GoogleDriveWatcherService`
  - Poll at configured interval via Task.Delay()
  - Yield `DocumentChange` for each detected change
  - Include full document content for Created/Modified
  - Include Id with empty content for Deleted
  - Respect `CancellationToken`
  - Support multiple concurrent subscribers

### Google Drive Adapter - Change Detection Tests

- [X] T040 [US2] Create `tests/Adapter.Source.GoogleDrive.Tests/GoogleDriveDocumentSourceSubscribeTests.cs` with mock polling:
  - Test: SubscribeToChanges yields Created event for new file
  - Test: SubscribeToChanges yields Modified event for updated file
  - Test: SubscribeToChanges yields Deleted event for removed file
  - Test: Event includes full document content for Created/Modified
  - Test: Event has empty content for Deleted
  - Test: Multiple subscribers receive same events
  - Test: SubscribeToChanges respects CancellationToken
  - Test: Polling occurs at configured interval
  - Test: Transient errors trigger exponential backoff
- [X] T041 [US2] Create `tests/Adapter.Source.GoogleDrive.Tests/GoogleDriveWatcherServiceTests.cs` with:
  - Test: Service detects file creation via polling
  - Test: Service detects file modification via LastModified change
  - Test: Service detects file deletion (missing from snapshot)
  - Test: Service uses correct polling interval
  - Test: Service implements exponential backoff on transient errors

---

## Phase 5: User Story 3 - Manage Multiple Sync Sources (P2)

### Multi-Source Configuration & Integration

- [X] T042 [P] [US3] Update `Application.csproj` to export `IDocumentSource` interface and models for public consumption
- [X] T043 [P] [US3] Create adapter factory helpers:
  - `src/Adapter.Source.FileSystem/src/FileSystemAdapterFactory.cs` to create configured instances
  - `src/Adapter.Source.GoogleDrive/src/GoogleDriveAdapterFactory.cs` to create configured instances
- [X] T044 [US3] Create integration test demonstrating multiple sources:
  - `tests/Adapter.Source.FileSystem.Tests/MultiSourceIntegrationTests.cs`
  - Configure two file system sources pointing to different directories
  - Verify subscriptions are independent
  - Verify changes in one source don't trigger notifications on other

### Google Drive & File System Integration Tests

- [X] T045 [US3] Create cross-adapter integration tests in `tests/Adapter.Source.FileSystem.Tests/CrossAdapterTests.cs`:
  - Test: Both adapters implement `IDocumentSource` contract identically
  - Test: Both adapters handle CancellationToken correctly
  - Test: Both adapters preserve encoding information
  - Test: Both adapters support concurrent subscribers
- [X] T046 [US3] Create `tests/Adapter.Source.GoogleDrive.Tests/CrossAdapterTests.cs` with same validation

---

## Phase 6: Polish & Cross-Cutting Concerns

### Documentation & Logging

- [X] T047 Add comprehensive XML documentation to all public types in:
  - `src/Application/Ports/IDocumentSource.cs`
  - `src/Adapter.Source.FileSystem/src/FileSystemDocumentSource.cs`
  - `src/Adapter.Source.GoogleDrive/src/GoogleDriveDocumentSource.cs`
- [X] T048 Add structured logging to:
  - File system adapter initialization and file reads
  - Google Drive adapter initialization, polling, and API calls
  - Change detection events (Created/Modified/Deleted)
  - Exception scenarios with context (source ID, file path, error details)

### Performance & Load Tests (Optional)

- [ ] T049 Create performance test: `ReadAllAsync()` streams 100+ documents in <2 seconds (file system)
- [ ] T050 Create performance test: `ReadAllAsync()` on Google Drive completes within <3 seconds
- [ ] T051 Create load test: Both adapters support 100+ concurrent subscribers without deadlock
- [ ] T052 Create memory profile: Streaming 1000-document directory uses <50MB heap

---

## Task Dependencies & Execution Order

### Critical Path (Blocking Order)

```
Setup (T001-T009)
  ↓
Foundational (T010-T021)
  ↓
US1 Phase (T022-T033) [parallel: FileSystem & Google Drive]
  ↓
US2 Phase (T034-T041) [parallel: FileSystem & Google Drive]
  ↓
US3 Phase (T042-T046)
  ↓
Polish (T047-T048)
```

### Parallelizable Tasks (by phase)

**Phase 1**: All directory/project creation tasks can run in parallel  
**Phase 2**: Enums, Records, Exceptions can run in parallel (except Application.csproj update at end)  
**Phase 3**: File system and Google Drive implementations can develop in parallel  
**Phase 4**: File system and Google Drive change detection can develop in parallel  
**Phase 5**: Multi-source integration tests can run after Phase 4  

### Recommended Team Allocation

**Developer A (File System Specialist)**:
- T006, T007, T008, T022-T027, T034-T037

**Developer B (Google Drive Specialist)**:
- T001-T005, T009, T028-T033, T038-T041

**Developer C (Core Architecture)**:
- T010-T021, T042-T048

---

## Acceptance Criteria & Independent Testing

### User Story 1 Independent Test

Acceptance: `ReadAllAsync()` can be fully tested independently by:
1. Create temporary directory with 3+ markdown files
2. Configure file system source
3. Call `ReadAllAsync()` and collect results
4. Verify: All files returned, encoding preserved, no duplicates
5. Repeat with Google Drive mock source
✅ **Independently testable**: Yes

### User Story 2 Independent Test

Acceptance: `SubscribeToChangesAsync()` can be fully tested independently by:
1. Create temporary directory with initial file
2. Subscribe via `SubscribeToChangesAsync()`
3. Add/modify/delete files in separate thread
4. Collect change notifications
5. Verify: Created/Modified/Deleted events received, includes full content
6. Repeat with Google Drive mock source
✅ **Independently testable**: Yes

### User Story 3 Independent Test

Acceptance: Multiple sources work independently by:
1. Create two file system sources pointing to different directories
2. Subscribe to both simultaneously
3. Make changes in each directory
4. Verify: Notifications only from affected source
✅ **Independently testable**: Yes

---

## Test Coverage Requirements

**Target**: >80% code coverage for both adapters

| Component | Type | Coverage Target |
|-----------|------|-----------------|
| FileSystemDocumentSource | Unit + Integration | >85% |
| GoogleDriveDocumentSource | Unit + Integration (mocked) | >85% |
| FileSystemWatcherService | Unit | >80% |
| GoogleDriveWatcherService | Unit | >80% |
| Application Layer (Ports) | Unit | >90% |

---

## Success Metrics

Upon completion:

- ✅ All 52 tasks completed
- ✅ All tests passing (unit, integration, contract)
- ✅ Code coverage >80% for both adapters
- ✅ File system: Change detection <1 second
- ✅ Google Drive: Polling successful within 5-10 second interval
- ✅ Both adapters handle 100+ concurrent subscribers
- ✅ Error scenarios logged and handled gracefully
- ✅ Documentation complete (XML + research.md updated)

---

## Blockers & Risks

| Risk | Mitigation |
|------|-----------|
| Google Drive API quota limits | Implement backoff; test with mocked responses |
| FileSystemWatcher buffer overflow | Implement debouncing; validate with rapid file ops |
| Cross-platform FileSystemWatcher differences | Test on target platforms; handle exceptions |
| Authentication token management | Assume caller provides valid token; document requirement |
| Large file handling | Implement streaming; test with >100MB files |

---

## Version History

| Version | Date | Status |
|---------|------|--------|
| 1.0 | 2026-05-02 | Initial task generation |

---

## Related Documentation

- [Feature Specification](spec.md)
- [Implementation Plan](plan.md)
- [Data Model](data-model.md)
- [Research Findings](research.md)
- [Interface Contracts](contracts/)
- [Quick Start Guide](quickstart.md)
