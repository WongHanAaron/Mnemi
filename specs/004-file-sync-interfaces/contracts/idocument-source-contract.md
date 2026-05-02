# IDocumentSource Interface Contract

**Date**: May 2, 2026  
**Feature**: [spec.md](../spec.md) | [plan.md](../plan.md) | [data-model.md](../data-model.md)

## Contract Overview

`IDocumentSource` is the core adapter interface that all document source implementations must fulfill. This contract defines the interface behavior, method signatures, return types, exceptions, and usage expectations.

---

## Interface Definition

```csharp
namespace Mnemi.Application.Ports
{
    /// <summary>
    /// Adapter interface for reading documents and subscribing to changes
    /// from pluggable sources (file system, Google Drive, etc.)
    /// </summary>
    public interface IDocumentSource
    {
        /// <summary>
        /// Read all documents from the configured source and stream them asynchronously.
        /// </summary>
        IAsyncEnumerable<DocumentContent> ReadAllAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Subscribe to changes in the source directory.
        /// </summary>
        IAsyncEnumerable<DocumentChange> SubscribeToChangesAsync(CancellationToken cancellationToken = default);
    }
}
```

---

## Method Specifications

### ReadAllAsync(CancellationToken)

**Signature**:
```csharp
IAsyncEnumerable<DocumentContent> ReadAllAsync(CancellationToken cancellationToken = default)
```

**Purpose**: 
Stream all documents from the configured source. Used for initial load or periodic full sync of content.

**Behavior**:

| Aspect | Specification |
|--------|---------------|
| **Enumeration Order** | Implementation-defined; no guaranteed order |
| **Filtering** | Documents matching adapter-specific filters only (e.g., *.md for file system) |
| **Duplicates** | No duplicates; each document appears once |
| **Large Sets** | Streaming; not limited by memory (lazily evaluated) |
| **Empty Source** | Yields nothing (empty enumerable); does not throw |
| **Encoding** | Preserves original encoding; returned in DocumentContent.Encoding |

**Parameters**:

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `cancellationToken` | `CancellationToken` | No | `default` | Token to cancel operation |

**Return Type**: 
`IAsyncEnumerable<DocumentContent>` — Enumerable of all documents in source

**Exceptions**:

| Exception | Condition | Example |
|-----------|-----------|---------|
| `OperationCanceledException` | Cancellation token cancelled | User pressed cancel button |
| `DocumentSourceConfigurationException` | Invalid configuration | Directory path doesn't exist, invalid folder ID |
| `DocumentSourceAuthenticationException` | Auth failure | OAuth2 token expired, invalid credentials |
| `DocumentSourceAccessException` | Permission denied | No read access to directory/folder |
| `DocumentSourceException` | Unexpected error | File system watcher failure, API timeout |

**Contract Requirements** (both implementations must satisfy):

- ✅ Must not throw for empty directories
- ✅ Must respect CancellationToken and stop iteration gracefully
- ✅ Must preserve text encoding when reading files
- ✅ Must not load all documents into memory at once
- ✅ Must fail fast for configuration/authentication errors
- ✅ Should complete within ~2 seconds for 100+ documents (file system) or ~3 seconds (Google Drive)
- ✅ Must handle missing/inaccessible files gracefully (skip with logging)

**Usage Example**:

```csharp
var config = new DocumentSourceConfig
{
    AdapterType = DocumentSourceAdapterType.FileSystem,
    SourceId = "local-cards",
    AdapterSpecificConfig = new() { ["DirectoryPath"] = "/cards" }
};

var source = new FileSystemDocumentSource(config);

await foreach (var doc in source.ReadAllAsync(cancellationToken))
{
    Console.WriteLine($"Document: {doc.Id}, {doc.Content.Length} chars");
}
```

---

### SubscribeToChangesAsync(CancellationToken)

**Signature**:
```csharp
IAsyncEnumerable<DocumentChange> SubscribeToChangesAsync(CancellationToken cancellationToken = default)
```

**Purpose**: 
Long-running subscription that monitors the source directory and yields change notifications as documents are created, modified, or deleted.

**Behavior**:

| Aspect | Specification |
|--------|---------------|
| **Lifetime** | Continues until cancelled or fatal error occurs |
| **Change Types** | Created, Modified, Deleted (per DocumentChangeType enum) |
| **Content Included** | Full DocumentContent for every change |
| **Duplicates** | No duplicate notifications for single change; may debounce rapid changes |
| **Initial State** | Does NOT emit existing documents; only future changes |
| **Latency** | File system: <1 second; Google Drive: 5-10 seconds |
| **Ordering** | Chronological by detection time |

**Parameters**:

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `cancellationToken` | `CancellationToken` | No | `default` | Token to cancel subscription |

**Return Type**: 
`IAsyncEnumerable<DocumentChange>` — Continuous stream of change notifications

**Exceptions**:

| Exception | Condition | Recoverable? |
|-----------|-----------|--------------|
| `OperationCanceledException` | Cancellation token cancelled | Yes (consumer requested) |
| `DocumentSourceConfigurationException` | Invalid configuration | No (fatal) |
| `DocumentSourceAuthenticationException` | Auth failure | No (fatal) |
| `DocumentSourceAccessException` | Permissions revoked | No (fatal) |
| `DocumentSourceTransientException` | Network timeout, API rate limit | Possibly (consumer decides) |
| `DocumentSourceException` | Unexpected error | No (fatal) |

**Contract Requirements** (both implementations must satisfy):

- ✅ Must not emit existing documents on subscription start
- ✅ Must yield changes with full document content included
- ✅ Must respect CancellationToken and terminate gracefully
- ✅ Must handle transient errors (network, rate limits) appropriately
- ✅ Must NOT require manual unsubscription; CancellationToken is sole control mechanism
- ✅ Must detect changes within specified latency targets
- ✅ Must support multiple concurrent subscribers on same source

**File System Implementation Notes**:
- Use FileSystemWatcher for real-time notification
- Implement debouncing (100-200ms) to prevent duplicate notifications
- Handle FileSystemWatcher buffer overflow gracefully

**Google Drive Implementation Notes**:
- Poll folder every 5-10 seconds (configurable)
- Compare file modifiedTime to previous snapshot
- Treat missing files as Deleted events

**Usage Example**:

```csharp
var config = new DocumentSourceConfig
{
    AdapterType = DocumentSourceAdapterType.GoogleDrive,
    SourceId = "shared-materials",
    AdapterSpecificConfig = new()
    {
        ["FolderId"] = "1ABC-xyz",
        ["AuthToken"] = credential
    }
};

var source = new GoogleDriveDocumentSource(config);
var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5)); // 5-minute subscription

await foreach (var change in source.SubscribeToChangesAsync(cts.Token))
{
    switch (change.ChangeType)
    {
        case DocumentChangeType.Created:
            Console.WriteLine($"New document: {change.Document.Id}");
            break;
        case DocumentChangeType.Modified:
            Console.WriteLine($"Updated: {change.Document.Id}");
            break;
        case DocumentChangeType.Deleted:
            Console.WriteLine($"Deleted: {change.Document.Id}");
            break;
    }
}
```

---

## Adapter Implementation Requirements

### Both Implementations MUST

1. **Implement the full interface** with both `ReadAllAsync` and `SubscribeToChangesAsync`
2. **Handle CancellationToken properly**
   - Check token before yielding each item
   - Throw `OperationCanceledException` if cancelled
   - Clean up resources (watchers, connections) on cancellation
3. **Provide meaningful error messages** with source context
4. **Support streaming** (use `yield return` or `IAsyncEnumerable` properly)
5. **Not require explicit disposal** beyond token cancellation
6. **Handle edge cases**:
   - Empty directories
   - No matching files
   - Access denied scenarios
   - Very large files (tested with >100MB)
7. **Include full DocumentContent in changes** (not just metadata)

### File System Implementation MUST

- Use `System.IO.FileSystemWatcher` for real-time monitoring
- Support extension filtering (e.g., *.md only)
- Detect Created, Modified, and Deleted events
- Debounce rapid consecutive changes
- Handle file encoding preservation

### Google Drive Implementation MUST

- Authenticate using provided OAuth2 token
- Support folder monitoring via Drive API v3
- Implement polling with configurable interval
- Handle API rate limiting gracefully
- Detect file changes via modifiedTime comparison

---

## Compatibility Matrix

| Scenario | File System | Google Drive | Notes |
|----------|-------------|--------------|-------|
| ReadAllAsync 100 docs | <2 seconds | <3 seconds | Performance target |
| Change detection | <1 second | 5-10 seconds | Due to polling |
| Concurrent subscribers | Yes | Yes | Multiple subscribers on same source |
| Large directories | Yes | Yes | Streaming handles large sets |
| Empty source | Returns empty | Returns empty | No exception thrown |
| Permission denied | Throws exception | Throws exception | Configuration error |
| Network interruption | File system N/A | Handled gracefully | Retry with backoff |
| Token cancellation | Clean stop | Clean stop | Resources cleaned up |

---

## Testing Requirements

All implementations MUST pass these contract tests:

```csharp
[Theory]
[InlineData("*.md")]
[InlineData("*.txt")]
public async Task ReadAllAsync_ReturnsAllMatchingDocuments(string extension)
{
    // Setup: Create source with known files
    // Act: Call ReadAllAsync and collect results
    // Assert: All matching files returned, count matches expectation
}

[Fact]
public async Task ReadAllAsync_RespectsCancel CancellationToken()
{
    // Setup: Create large dataset, cts with short timeout
    // Act: Iterate until CancellationToken fires
    // Assert: OperationCanceledException thrown, iteration stopped
}

[Fact]
public async Task ReadAllAsync_EmptyDirectoryReturnsEmpty()
{
    // Setup: Empty source
    // Act: Call ReadAllAsync
    // Assert: Enumerable returns immediately with no items
}

[Fact]
public async Task SubscribeToChanges_YieldsCreatedEvent()
{
    // Setup: Source with subscription
    // Act: Create new file, await change notification
    // Assert: DocumentChange received with type Created and full content
}

[Fact]
public async Task SubscribeToChanges_YieldsModifiedEvent()
{
    // Similar to Created test, but modifying existing file
}

[Fact]
public async Task SubscribeToChanges_YieldsDeletedEvent()
{
    // Similar to Created test, but deleting file
}

[Fact]
public async Task SubscribeToChanges_DoesNotYieldExistingDocumentsOnStart()
{
    // Setup: Source with pre-existing files
    // Act: Subscribe, immediate modification
    // Assert: Only new change yielded, existing files NOT emitted
}

[Fact]
public async Task SubscribeToChanges_SupportsConcurrentSubscribers()
{
    // Setup: Multiple subscribers on same source
    // Act: Make changes, collect from all subscribers
    // Assert: All subscribers receive same notifications
}
```

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2026-05-02 | Initial contract definition |

---

## Related Documents

- [DocumentContent Contract](document-content-contract.md)
- [DocumentChange Contract](document-change-contract.md)
- [Data Model](../data-model.md)
- [Research](../research.md)
