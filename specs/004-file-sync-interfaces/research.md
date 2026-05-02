# Research Findings: File Synchronization Interfaces

**Date**: May 2, 2026  
**Feature**: [spec.md](spec.md) | [plan.md](plan.md)  
**Status**: Complete (no NEEDS CLARIFICATION items)

## Summary

All architectural decisions have been clarified through the specification session. This document consolidates research findings on implementation approaches for both adapters.

---

## File System Implementation Research

### Technology: System.IO.FileSystemWatcher

**Decision**: Use `System.IO.FileSystemWatcher` for file system change detection

**Rationale**:
- Built-in to .NET framework (no external dependencies)
- Efficient real-time monitoring (uses OS-level file system notifications)
- Available on Windows, macOS, and Linux via .NET
- Proven, stable API widely used in production applications
- Supports multiple file filters and recursive directory monitoring

**Limitations & Mitigations**:
| Limitation | Impact | Mitigation |
|-----------|--------|-----------|
| Can miss rapid changes | Rapid file changes might not be captured | Implement debouncing (100-200ms) and re-scan on startup |
| Buffer overflow under high churn | Overflowing internal buffer → missed events | Use reasonable buffer size, document limits (100+ files/sec) |
| Platform-specific quirks | Windows vs. Linux behavior differences | Test on target platforms; handle exceptions gracefully |
| No initial state notification | Consumer doesn't know about pre-existing files | `ReadAllAsync()` provides initial state separately |

**Implementation Pattern**:
```csharp
FileSystemWatcher watcher = new FileSystemWatcher(path, filter)
{
    EnableRaisingEvents = true
};

// Wire up events: Created, Changed, Deleted, Renamed
// Debounce rapid changes
// Yield changes as IAsyncEnumerable<DocumentChange>
```

**Testing Approach**:
- Use temporary directories created during test setup
- Create/modify/delete files with controlled timing
- Verify watcher captures all operations within 1-2 seconds
- Test with extension filters to ensure only .md files are tracked

---

## Google Drive Implementation Research

### Primary Decision: Polling via Google.Apis.Drive.v3

**Decision**: Use polling with Google Drive API v3 (not webhooks) for MVP

**Rationale**:
- **Simpler architecture**: No need for public endpoint or webhook infrastructure
- **Sufficient latency**: 5-10 second polling meets "near real-time" requirement (vs. <1s for file system)
- **Lower operational overhead**: No webhook registration/renewal complexity
- **Easier testing**: Mock API responses without external infrastructure
- **Google.Apis.Drive v3 maturity**: Well-documented, widely used, stable

**Alternatives Considered**:

| Alternative | Pros | Cons | Reason Rejected |
|-------------|------|------|-----------------|
| Google Drive Push Notifications (Webhooks) | True real-time (< 1s latency) | Requires public HTTPS endpoint, webhook management, complex registration flow | Too complex for MVP; polling sufficient |
| Google Workspace Events API | Newer, more flexible | Limited availability, less mature | Not yet broadly available |
| Simple polling with batch.get() | Simple, stable | High API quota usage | Inefficient; would hit quota quickly |
| Google Drive Activity API | Tracks all changes | Different API surface, quota implications | Overkill for document source use case |

**Implementation Pattern**:
```csharp
// 1. Initialize DriveService with OAuth2 token
// 2. List files in folder with updated after timestamp
// 3. Yield changes as IAsyncEnumerable<DocumentChange>
// 4. Poll every 5-10 seconds (configurable)
// 5. Handle quota limits gracefully
```

**Authentication Approach**:
- Assume host application has already obtained OAuth2 token
- Adapter accepts `GoogleCredential` or `ICredential` in config
- Focus: Document source reading, not auth flow handling
- Respect Google's rate limiting (1000 requests/100 seconds)

**Known Challenges**:
| Challenge | Solution |
|-----------|----------|
| Detecting file modifications | Use `modifiedTime` field; track last-seen timestamp |
| Handling moved/renamed files | Treat as delete + create events (per spec assumption) |
| Network timeouts | Retry with exponential backoff; use CancellationToken |
| Large folders (1000+ files) | Paginate results; stream incrementally |
| API quota exhaustion | Log warnings; implement backoff strategy |

---

## Error Handling Design

### Strategy: Exception Throwing + Graceful Degradation

**Recoverable Errors** (should be thrown, let consumer decide on retry):
- Network timeout during polling
- Transient Google API errors (rate limit, temporary unavailability)
- File temporarily locked (file system)
- Folder permissions changed

**Non-Recoverable Errors** (throw immediately):
- Authentication failure (invalid token, expired, insufficient scopes)
- Invalid configuration (bad directory path, invalid folder ID)
- Unsupported file encoding
- Out of memory (streaming extremely large files)

**Implementation**:
- Wrap underlying exceptions in custom `DocumentSourceException` hierarchy
- Include context: source type, operation (read/subscribe), file/folder details
- Log all errors with appropriate severity level
- Support `CancellationToken` to allow consumers to cancel retry operations

---

## Performance Considerations

### File System

- **ReadAllAsync()**: Directory.EnumerateFiles() with filter → stream results
- **SubscribeToChangesAsync()**: FileSystemWatcher → yield events in real-time
- **Expected performance**: 100+ files in <2 seconds, change detection <1 second

**Optimization**:
- Enumerate files lazily to avoid loading all into memory
- Implement debouncing to avoid duplicate notifications from rapid changes
- Use appropriate buffer sizes for FileSystemWatcher

### Google Drive

- **ReadAllAsync()**: Drive.Files.List() with pagination → stream results
- **SubscribeToChangesAsync()**: Poll Drive.Files.List() every 5-10 seconds
- **Expected performance**: <3 seconds for auth + first stream; 5-10s polling interval

**Optimization**:
- Paginate results (pageSize=100) to balance latency and quota
- Cache `modifiedTime` from last poll to minimize API calls
- Use `spaces='drive'` filter to reduce unnecessary file types
- Implement exponential backoff for transient API errors

---

## Testing Strategy

### Unit Tests

**File System Adapter** (`tests/Adapter.Source.FileSystem.Tests/`):
- Mock file system via interfaces or use real temporary directories
- Test extension filtering (case-insensitivity)
- Test change detection (create, modify, delete events)
- Test cancellation token support
- Test error scenarios (access denied, file locked)

**Google Drive Adapter** (`tests/Adapter.Source.GoogleDrive.Tests/`):
- Mock Google Drive API responses (don't call real API)
- Test OAuth2 token handling
- Test pagination and large file lists
- Test change polling logic
- Test quota handling and backoff
- Test error scenarios (auth failure, network timeout)

**Coverage Target**: >80% for both adapters

### Integration Tests

- Test file system adapter with real temporary directories
- Test Google Drive adapter with mock responses (no real API calls)
- Verify both adapters fulfill `IDocumentSource` contract
- Test concurrent streams (100+ parallel subscriptions)

---

## Timeline & Dependencies

### Critical Path

1. **Phase 1 Completion** (now): Design artifacts & interface contracts
2. **Phase 2**: Task generation from design
3. **Phase 3**: Implementation
   - Application layer interfaces (1-2 days)
   - File system adapter (2-3 days)
   - Google Drive adapter (3-4 days)
   - Unit tests (2-3 days each adapter)
4. **Phase 4**: Integration testing & refinement (1-2 days)

### External Dependencies

- **Google Cloud Project Setup**: Required for Google Drive OAuth2
  - Requires credentials file (client_id, client_secret)
  - Out of scope for this feature; assumed pre-existing
- **Testing Infrastructure**: xUnit + Moq already available in project

---

## Recommendations

1. **Start with file system adapter**: Simpler, no external dependencies, faster to get working
2. **Validate streaming pattern**: Ensure `IAsyncEnumerable` works well with Blazor/UI layer
3. **Plan for scaling**: Consider adding webhook support (phase 2) if polling latency becomes issue
4. **Document configuration**: Provide clear examples for both adapters' configuration objects
5. **Add observability**: Implement logging/metrics for monitoring production usage

---

## Sign-Off

**Research Status**: ✅ Complete  
**No NEEDS CLARIFICATION items remain**  
**Ready for Phase 1 Design**: ✅ Yes  
**Ready for Phase 2 Task Generation**: ✅ Yes
