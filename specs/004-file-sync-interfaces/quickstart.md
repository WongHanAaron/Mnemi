# Quick Start: File Synchronization Interfaces

**Date**: May 2, 2026  
**Feature**: [spec.md](spec.md) | [plan.md](plan.md)  
**For Implementers**: Use this guide while developing the adapters

---

## Overview

The `IDocumentSource` adapter interface provides a unified way to read documents and subscribe to changes from multiple sources. This guide walks through the implementation approach and key integration points.

## Core Concepts

### 1. Adapter Pattern

The design uses a **pluggable adapter pattern**:
- **Interface** (`IDocumentSource`) lives in `Application/Ports/` layer
- **Implementations** (`FileSystem`, `GoogleDrive`) live in separate `Adapter.Source.*` projects
- **Configuration** via `DocumentSourceConfig` with adapter-specific dictionary

### 2. Streaming with IAsyncEnumerable

Both operations (`ReadAllAsync`, `SubscribeToChangesAsync`) return `IAsyncEnumerable<T>`:
- Enables lazy evaluation (documents streamed one at a time)
- Memory-efficient for large directories
- Natural async/await integration with Blazor

### 3. Change Notification Content

Each change notification includes **full document content**, not just metadata:
- Consumers can immediately process the document without separate read call
- Enables reactive UI updates directly from change events
- Reduces round-trips to source

---

## Implementation Checklist

### Phase 1: Core Interface (Application Layer)

**File**: `src/Application/Ports/IDocumentSource.cs`

```csharp
public interface IDocumentSource
{
    IAsyncEnumerable<DocumentContent> ReadAllAsync(CancellationToken cancellationToken = default);
    IAsyncEnumerable<DocumentChange> SubscribeToChangesAsync(CancellationToken cancellationToken = default);
}
```

**Supporting Types**:
- `DocumentContent` (record)
- `DocumentChange` (record)
- `DocumentSourceConfig` (class)
- `DocumentChangeType` (enum)
- `DocumentSourceAdapterType` (enum)
- Exception hierarchy

**Deliverable**: All types compile and are discoverable in Application project

---

### Phase 2: File System Adapter

**Project**: `src/Adapter.Source.FileSystem/src/Adapter.Source.FileSystem.csproj`

**Key Classes**:

1. **FileSystemDocumentSource** (implements `IDocumentSource`)
   - Constructor: Accepts `DocumentSourceConfig`
   - `ReadAllAsync()`: Uses `Directory.EnumerateFiles()` with filter
   - `SubscribeToChangesAsync()`: Uses `FileSystemWatcher`

2. **FileSystemWatcherService**
   - Encapsulates `FileSystemWatcher` lifecycle
   - Handles debouncing of rapid changes
   - Converts `FileSystemEventArgs` to `DocumentChange`

**Implementation Approach**:

```csharp
public class FileSystemDocumentSource : IDocumentSource
{
    private readonly string _directoryPath;
    private readonly string _searchPattern;
    
    public FileSystemDocumentSource(DocumentSourceConfig config)
    {
        // Validate config, extract DirectoryPath and SearchPattern
    }
    
    public async IAsyncEnumerable<DocumentContent> ReadAllAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var file in Directory.EnumerateFiles(_directoryPath, _searchPattern))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var content = await ReadFileAsync(file, cancellationToken);
            yield return content;
        }
    }
    
    public async IAsyncEnumerable<DocumentChange> SubscribeToChangesAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using (var watcher = new FileSystemWatcher(_directoryPath, _searchPattern))
        {
            // Setup channels for events
            // Yield DocumentChange objects as events arrive
        }
    }
}
```

**Testing**:
- Create temporary directories during test setup
- Create/modify/delete test files
- Verify change detection within 1-2 seconds
- Test extension filtering
- Test cancellation token support

---

### Phase 3: Google Drive Adapter

**Project**: `src/Adapter.Source.GoogleDrive/src/Adapter.Source.GoogleDrive.csproj`

**Dependencies**:
- NuGet: `Google.Apis.Drive.v3`
- NuGet: `Google.Apis.Auth`

**Key Classes**:

1. **GoogleDriveDocumentSource** (implements `IDocumentSource`)
   - Constructor: Accepts `DocumentSourceConfig` with OAuth2 token
   - `ReadAllAsync()`: Calls `Drive.Files.List()` with pagination
   - `SubscribeToChangesAsync()`: Polls folder every 5-10 seconds

2. **GoogleDriveClient**
   - Wraps `DriveService` API
   - Handles authentication and token refresh
   - Provides methods for listing and monitoring files

**Implementation Approach**:

```csharp
public class GoogleDriveDocumentSource : IDocumentSource
{
    private readonly DriveService _driveService;
    private readonly string _folderId;
    private readonly int _pollingIntervalMs;
    
    public GoogleDriveDocumentSource(DocumentSourceConfig config)
    {
        // Initialize DriveService from token
        // Extract FolderId and PollingInterval
    }
    
    public async IAsyncEnumerable<DocumentContent> ReadAllAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var request = _driveService.Files.List();
        request.Q = $"'{_folderId}' in parents";
        request.PageSize = 100; // Pagination
        
        foreach (var file in await request.ExecuteAsync(cancellationToken))
        {
            var content = await DownloadFileAsync(file, cancellationToken);
            yield return content;
        }
    }
    
    public async IAsyncEnumerable<DocumentChange> SubscribeToChangesAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var previousState = new Dictionary<string, DateTime>();
        
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(_pollingIntervalMs, cancellationToken);
            
            // Compare current folder state with previousState
            // Yield DocumentChange for created/modified/deleted files
        }
    }
}
```

**Testing**:
- Mock Google Drive API responses (do NOT call real API in tests)
- Use `Moq` to create fake `DriveService` behavior
- Test pagination handling
- Test polling and change detection
- Test error scenarios (auth failure, rate limit, etc.)

---

## Integration Points

### 1. Application Layer Integration

The adapters will be used by application services (e.g., `IHomeDashboardService`):

```csharp
// In Application/Home/IHomeDashboardService.cs
public interface IHomeDashboardService
{
    Task LoadDocumentsAsync(IDocumentSource source);
    Task SubscribeToUpdatesAsync(IDocumentSource source);
}

// In Application/Home/HomeDashboardService.cs
public class HomeDashboardService : IHomeDashboardService
{
    public async Task LoadDocumentsAsync(IDocumentSource source)
    {
        await foreach (var doc in source.ReadAllAsync())
        {
            // Parse markdown, create cards, etc.
        }
    }
    
    public async Task SubscribeToUpdatesAsync(IDocumentSource source)
    {
        await foreach (var change in source.SubscribeToChangesAsync())
        {
            // Update UI with new/modified/deleted cards
        }
    }
}
```

### 2. Dependency Injection Setup

Register adapters in `Program.cs`:

```csharp
// In Ui.Web/Program.cs or Ui.Maui/Program.cs
builder.Services.AddScoped<FileSystemDocumentSource>();
builder.Services.AddScoped<GoogleDriveDocumentSource>();
builder.Services.AddScoped<IHomeDashboardService, HomeDashboardService>();
```

### 3. UI Integration (Blazor)

Use in Blazor components:

```csharp
// In Ui.Shared/Components/DocumentSourceSelector.razor.cs
@inject FileSystemDocumentSource FileSystemSource
@inject GoogleDriveDocumentSource GoogleDriveSource

@code {
    private IDocumentSource _currentSource;
    
    private async Task InitializeFileSystemSource()
    {
        var config = new DocumentSourceConfig
        {
            AdapterType = DocumentSourceAdapterType.FileSystem,
            SourceId = "local-cards",
            AdapterSpecificConfig = new() 
            { 
                ["DirectoryPath"] = "/cards",
                ["SearchPattern"] = "*.md"
            }
        };
        
        _currentSource = new FileSystemDocumentSource(config);
        await foreach (var doc in _currentSource.ReadAllAsync())
        {
            // Process document
        }
    }
}
```

---

## Testing Strategy

### Unit Tests (No External Resources)

**File System Adapter** (`tests/Adapter.Source.FileSystem.Tests/`):
- Mock file system using interfaces
- Or use real temporary directories (xUnit fixture)
- Test filtering, change detection, cancellation

**Google Drive Adapter** (`tests/Adapter.Source.GoogleDrive.Tests/`):
- Mock `DriveService` with Moq
- Simulate API responses without calling Google servers
- Test pagination, polling, error handling

### Integration Tests (Controlled Resources)

- Use real temporary directories (file system)
- Use mock API responses (Google Drive)
- Verify both adapters fulfill `IDocumentSource` contract
- Test concurrent subscribers

### Performance Tests

- File system: Measure ReadAllAsync with 100+ documents
- Google Drive: Measure polling and change detection latency
- Verify targets: <2s (file system), <3s (Google Drive)

---

## Common Pitfalls & Solutions

| Pitfall | Solution |
|---------|----------|
| Not respecting CancellationToken | Check token before yielding each item; use `[EnumeratorCancellation]` attribute |
| Loading all documents into memory | Use `IAsyncEnumerable` with `yield return` pattern |
| Not handling encoding preservation | Store encoding name in DocumentContent; use when reading files |
| Not debouncing file system events | Implement 100-200ms debounce window in FileSystemWatcher handler |
| Calling real Google API in tests | Always mock with Moq; never execute against production API |
| Not cleaning up resources on cancel | Use `using` statements for disposables (watchers, connections) |
| Including only metadata in changes | Include full DocumentContent in every change notification |
| Not supporting multiple concurrent subscribers | Design for multi-subscriber support from start |

---

## Key Metrics to Track

During implementation, measure:

1. **ReadAllAsync Performance**: 100+ documents in < 2 seconds
2. **Change Detection Latency**: <1 second for file system, 5-10 seconds for Google Drive
3. **Concurrent Subscribers**: Support 100+ concurrent streams without deadlock
4. **Error Scenarios**: All documented exceptions are thrown correctly
5. **Test Coverage**: >80% for both adapters

---

## Next Steps

1. **Specification Review**: Ensure this quickstart aligns with detailed [data-model.md](data-model.md)
2. **Contract Review**: Reference [idocument-source-contract.md](contracts/idocument-source-contract.md) for detailed method signatures
3. **Research Review**: Check [research.md](research.md) for implementation approach guidance
4. **Task Generation**: Run `/speckit.tasks` to generate detailed implementation tasks
5. **Implementation**: Follow tasks.md in sequential order

---

## Resources

- **C# Async Patterns**: https://docs.microsoft.com/en-us/archive/msdn-magazine/2013/march/async-await-best-practices-in-asynchronous-programming
- **IAsyncEnumerable**: https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.iasyncenumerable-1
- **FileSystemWatcher**: https://docs.microsoft.com/en-us/dotnet/api/system.io.filesystemwatcher
- **Google Drive API v3**: https://developers.google.com/drive/api/guides/about-sdk
- **xUnit for C#**: https://xunit.net/
- **Moq Mocking Framework**: https://github.com/moq/moq4

---

**Document Version**: 1.0  
**Last Updated**: May 2, 2026
