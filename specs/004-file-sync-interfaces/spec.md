# Feature Specification: File Synchronization Interfaces

**Feature Branch**: `004-file-sync-interfaces`  
**Created**: May 2, 2026  
**Status**: Draft  
**Input**: User description: "I would like to implement an interface for file content reading and subscription to changes from a directory. I would like there to be 2 implementations: A file system based synchronization and a google drive based synchronization."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Read File Content from Directory (Priority: P1)

The system needs to read file content from a configured directory source via `IDocumentSource`, supporting both local file system and Google Drive sources. This foundational capability enables the flashcard app to load markdown files and extract card definitions using async streaming.

**Why this priority**: Core functionality required before any other features can ingest external content. Blocks integration with both file system and Google Drive sources.

**Independent Test**: Can be fully tested by calling `ReadAllAsync()` on both file system and Google Drive sources, iterating through the `IAsyncEnumerable<DocumentContent>`, and verifying that content is streamed correctly.

**Acceptance Scenarios**:

1. **Given** a configured file system `IDocumentSource`, **When** calling `ReadAllAsync()` and iterating through the returned `IAsyncEnumerable<DocumentContent>`, **Then** all documents from the directory are streamed with correct text encoding
2. **Given** a configured Google Drive `IDocumentSource` with authentication, **When** calling `ReadAllAsync()`, **Then** documents are retrieved and streamed one at a time
3. **Given** a document that doesn't exist, **When** iterating through `ReadAllAsync()`, **Then** that document is skipped or an error is appropriately handled
4. **Given** a file system source is configured with extension filters, **When** calling `ReadAllAsync()`, **Then** only files matching the filter are included in the stream

---

### User Story 2 - Subscribe to Directory Changes (Priority: P1)

The system needs to subscribe to changes in a directory (file created, modified, deleted) from both local file system and Google Drive via `IDocumentSource.SubscribeToChangesAsync()`. Changes are streamed as `IAsyncEnumerable<DocumentChange>` including full document content. This enables near-real-time updates when card definitions are added or modified externally.

**Why this priority**: Essential for keeping the flashcard data synchronized without requiring manual refresh. Equally critical for both sources to ensure consistent UX regardless of content location.

**Independent Test**: Can be fully tested by creating/modifying/deleting a file in the source directory, calling `SubscribeToChangesAsync()`, iterating through the change stream, and verifying that subscribers receive the appropriate change notification with full content within an acceptable timeframe.

**Acceptance Scenarios**:

1. **Given** a subscription is active via `SubscribeToChangesAsync()` on a file system source, **When** a new markdown file is added, **Then** the change stream yields a `DocumentChange` with type "Created", file path, timestamp, and full document content
2. **Given** a subscription is active on a file system source, **When** an existing file is modified, **Then** a `DocumentChange` with type "Modified", file path, timestamp, and updated full content is yielded
3. **Given** a subscription is active on a file system source, **When** a file is deleted, **Then** a `DocumentChange` with type "Deleted", file path, and timestamp is yielded
4. **Given** a subscription is active on a Google Drive source, **When** files change, **Then** similar `DocumentChange` notifications are yielded as the file system source
5. **Given** a subscription stream via `CancellationToken`, **When** the token is cancelled, **Then** the stream terminates and no further notifications are yielded
6. **Given** multiple subscriptions are active on the same source, **When** a change occurs, **Then** all active subscription streams receive the same change notification

---

### User Story 3 - Manage Multiple Sync Sources (Priority: P2)

The system should support configuring multiple independent `IDocumentSource` instances simultaneously, allowing users to sync card definitions from both local directories and Google Drive in the same session with separate streaming subscriptions for each source.

**Why this priority**: Enables flexibility in content organization but not required for MVP. Users can work with one source at a time if needed.

**Independent Test**: Can be fully tested by creating two different sources, calling `SubscribeToChangesAsync()` on each independently, triggering changes in each source's directory, and verifying that notifications are received only from the source where changes occurred.

**Acceptance Scenarios**:

1. **Given** both file system and Google Drive `IDocumentSource` instances are active, **When** changes occur in one source, **Then** only subscribers listening to that source's stream receive notifications
2. **Given** multiple file system sources pointing to different directories, **When** they're configured simultaneously with independent subscriptions, **Then** each subscription stream receives only changes from its respective directory

---

### Edge Cases

- What happens when a file is moved or renamed (treated as delete + create or as single rename event)?
- How does the system handle network interruptions for Google Drive connections?
- What is the behavior when trying to read/subscribe to a source that requires authentication but credentials are not provided?
- How are duplicate notifications handled if a file is modified multiple times in quick succession?
- What happens when Google Drive folder structure changes (permissions revoked, folder deleted)?
- How does the system handle very large files in either source?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide an `IDocumentSource` adapter interface behind which file content reading and change subscription are implemented
- **FR-002**: `IDocumentSource.ReadAllAsync(CancellationToken)` MUST return `IAsyncEnumerable<DocumentContent>` to stream all documents from a configured source
- **FR-003**: `IDocumentSource.SubscribeToChangesAsync(CancellationToken)` MUST return `IAsyncEnumerable<DocumentChange>` to stream file changes (created, modified, deleted) with full document content included
- **FR-004**: System MUST define `DocumentSourceConfig` configuration object supporting adapter-specific settings via properties dictionary (e.g., path for file system, folder ID for Google Drive)
- **FR-005**: System MUST implement `IDocumentSource` for local file system directories in a dedicated `Adapter.Source.FileSystem` project
- **FR-006**: System MUST implement `IDocumentSource` for Google Drive folders in a dedicated `Adapter.Source.GoogleDrive` project
- **FR-007**: File system implementation MUST support filtering files by extension (e.g., only markdown files)
- **FR-008**: File system implementation MUST use efficient change detection (watch file system events rather than polling)
- **FR-009**: Google Drive implementation MUST authenticate using OAuth2 before accessing files
- **FR-010**: Google Drive implementation MUST handle authentication token refresh automatically
- **FR-011**: Both implementations MUST handle errors gracefully and provide meaningful error messages
- **FR-012**: `DocumentContent` MUST preserve original text encoding when reading files
- **FR-013**: `DocumentChange` MUST include: change type (Created/Modified/Deleted), file path/name, timestamp, and full document content for modified/created events
- **FR-014**: Both implementations MUST support graceful cancellation via `CancellationToken` parameter on streaming methods

### Key Entities

- **DocumentContent**: Represents a document from a source including: identifier/path, content text, last modified timestamp, and encoding
- **DocumentChange**: Represents a change event with: change type (Created/Modified/Deleted), document identifier/path, timestamp, and full document content
- **DocumentSourceConfig**: Configuration object for initializing an `IDocumentSource` adapter, containing base properties (adapter type, source identifier) and adapter-specific properties dictionary for customization
- **IDocumentSource**: Unified adapter interface defining contract for reading documents and subscribing to changes from a configured source
- **Adapter.Source.FileSystem**: Concrete implementation of `IDocumentSource` for local file system directories
- **Adapter.Source.GoogleDrive**: Concrete implementation of `IDocumentSource` for Google Drive folders

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: File system `IDocumentSource.ReadAllAsync()` can stream and parse a 100+ document directory in under 2 seconds (including all async overhead)
- **SC-002**: File system `SubscribeToChangesAsync()` detects and yields change notifications to the stream within 1 second of file occurrence
- **SC-003**: Google Drive `IDocumentSource` successfully authenticates, initializes, and begins streaming within 3 seconds
- **SC-004**: Both implementations handle at least 100 concurrent streams (ReadAllAsync + SubscribeToChangesAsync) without deadlock or data loss
- **SC-005**: Both implementations have > 95% uptime and gracefully handle all documented error scenarios (network failures, permission errors, etc.)
- **SC-006**: Implementations achieve >80% code coverage with unit tests that verify streaming behavior, cancellation, and error conditions

## Assumptions

- File system sources point to local directories with standard file permissions
- Google Drive sources require user to have already authorized the application via OAuth2
- File extensions used for filtering are case-insensitive (e.g., .md, .MD both match)
- "File change" refers to create/modify/delete; rename is treated as delete + create
- Network connectivity is assumed stable during runtime (brief disconnections should be recoverable)
- The application handles one authentication session per Google Drive source (no multi-user concurrent access to same source)
- Markdown files are UTF-8 encoded or use standard text encodings (ASCII, UTF-16, etc.)
- All streaming operations use `IAsyncEnumerable<T>` for consistent async iteration patterns
- File system watchers are available on the target platform (Windows/macOS/Linux)
- Configuration objects support extensibility through adapter-specific properties dictionaries to avoid tight coupling

## Clarifications

### Session 2026-05-02

- Q: What structure should define directory/folder source configuration? → A: Configuration object (`DocumentSourceConfig`) with base properties and adapter-specific dictionary for flexibility
- Q: How should change subscriptions work with async patterns? → A: `IAsyncEnumerable<DocumentChange>` with `CancellationToken` parameter for modern async/await patterns
- Q: Should read-all operations stream or batch-load documents? → A: `IAsyncEnumerable<DocumentContent>` for streaming to support large directories efficiently
- Q: How to handle different configuration requirements for file system vs. Google Drive? → A: Generic config object with adapter-specific properties dictionary for loose coupling
- Q: Should change notifications include document content or just metadata? → A: Include full `DocumentContent` in change notifications to enable immediate processing without separate read calls
