# DocumentChange Contract

**Date**: May 2, 2026  
**Type**: Data Model Contract  
**Related**: [IDocumentSource Contract](idocument-source-contract.md), [DocumentContent Contract](document-content-contract.md)

## Overview

`DocumentChange` is a read-only record type that represents a single change event in a monitored source directory. It is yielded by `IDocumentSource.SubscribeToChangesAsync()` as documents are created, modified, or deleted.

---

## Type Definition

```csharp
namespace Mnemi.Application.Ports
{
    /// <summary>
    /// Represents a single change event detected in a monitored directory.
    /// Includes full document content and metadata for immediate consumer processing.
    /// </summary>
    public record DocumentChange
    {
        /// <summary>
        /// Type of change that occurred: Created, Modified, or Deleted.
        /// </summary>
        public required DocumentChangeType ChangeType { get; init; }
        
        /// <summary>
        /// The document that was changed.
        /// 
        /// For Created events: Full document content with current state
        /// For Modified events: Full document content with updated text and metadata
        /// For Deleted events: Document with empty content; Id identifies the deleted file
        /// 
        /// Consumers can process document immediately without separate read operation.
        /// </summary>
        public required DocumentContent Document { get; init; }
        
        /// <summary>
        /// ISO 8601 UTC timestamp when the change was detected by the adapter.
        /// 
        /// Note: This is when the adapter detected the change, not when it occurred.
        /// For file system: Should be within 100-500ms of actual change
        /// For Google Drive: May be delayed by polling interval (5-10 seconds)
        /// 
        /// Use Document.LastModified for the actual source modification time.
        /// </summary>
        public required DateTime DetectedAt { get; init; }
    }
}
```

---

## Enumeration: DocumentChangeType

```csharp
public enum DocumentChangeType
{
    /// <summary>New document was created in the source</summary>
    Created = 1,
    
    /// <summary>Existing document was modified (content or metadata changed)</summary>
    Modified = 2,
    
    /// <summary>Document was deleted from the source</summary>
    Deleted = 3
}
```

---

## Field Specifications

### ChangeType

| Property | Value |
|----------|-------|
| Type | `DocumentChangeType` (enum) |
| Required | Yes |
| Immutable | Yes (init-only) |
| Allowed Values | Created (1), Modified (2), Deleted (3) |

**Rules**:
- Must be a valid enum value
- Renames in source are represented as Deleted + Created (per spec)
- No other change types (e.g., "Renamed", "Moved") are used

**Examples**:
```csharp
// New file created
new DocumentChange { ChangeType = DocumentChangeType.Created, ... }

// Existing file modified
new DocumentChange { ChangeType = DocumentChangeType.Modified, ... }

// File deleted
new DocumentChange { ChangeType = DocumentChangeType.Deleted, ... }
```

---

### Document

| Property | Value |
|----------|-------|
| Type | `DocumentContent` (record) |
| Required | Yes |
| Immutable | Yes (init-only) |
| Null | Not allowed |

**Rules**:

**For Created Events**:
- Include full document with content
- Document represents the newly created file as it appears in source
- Example: File just uploaded, or new markdown file created locally

**For Modified Events**:
- Include full document with updated content
- Content is the current state from source (not diff)
- LastModified reflects the modification time
- Example: File content edited, or markdown updated

**For Deleted Events**:
- Document.Id identifies the deleted file
- Document.Content should be empty string
- Document.LastModified may be from before deletion (last known time)
- Document.Encoding can be empty or from last known state
- Example: File removed from directory or Google Drive folder

**Benefits of Including Full Content**:
- Consumers can process the document immediately without round-trip
- Enables reactive UI updates directly from change notifications
- Reduces latency in data flow
- Simplifies consumer logic (no separate read needed)

---

### DetectedAt

| Property | Value |
|----------|-------|
| Type | `DateTime` |
| Required | Yes |
| Immutable | Yes (init-only) |
| Format | ISO 8601 UTC |
| Kind | Must be UTC (DateTimeKind.Utc) |
| Precision | Per-millisecond (or available precision) |
| Future | Should never be in future |

**Rules**:
- Must be in UTC timezone
- Represents when the adapter detected the change
- May differ from Document.LastModified due to detection latency
- File system: Should be within 100-500ms of actual change
- Google Drive: May be delayed by polling interval (5-10 seconds)

**Difference from Document.LastModified**:
```
Document.LastModified = When file was actually changed in source
DetectedAt = When adapter detected the change
Latency = DetectedAt - Document.LastModified
```

**Examples**:
```csharp
// File modified at 14:30:00, detected at 14:30:00.050
new DocumentChange
{
    Document = new { LastModified = DateTime.Parse("2026-05-02T14:30:00Z"), ... },
    DetectedAt = DateTime.Parse("2026-05-02T14:30:00.050Z"), // 50ms later
    ...
}

// File modified at 14:30:00, polled at 14:30:05
new DocumentChange
{
    Document = new { LastModified = DateTime.Parse("2026-05-02T14:30:00Z"), ... },
    DetectedAt = DateTime.Parse("2026-05-02T14:30:05.000Z"), // 5s later (Google Drive polling)
    ...
}
```

---

## Validation Rules

| Field | Rule | Enforcement |
|-------|------|-------------|
| ChangeType | Must be valid enum | Exception in constructor |
| Document | Cannot be null | Exception in constructor |
| Document.Id | Cannot be empty (even for Deleted) | Exception in constructor |
| Document.Content | Empty for Deleted; otherwise has content | Validation warning if violated |
| DetectedAt | Kind must be UTC | Exception if not UTC |
| DetectedAt | Should not be future | Warning log if violated |

**Example Validation**:
```csharp
public record DocumentChange
{
    public required DocumentChangeType ChangeType { get; init; }
    
    public required DocumentContent Document
    {
        get;
        init
        {
            if (value == null)
                throw new ArgumentNullException(nameof(Document));
            if (string.IsNullOrWhiteSpace(value.Id))
                throw new ArgumentException("Document.Id cannot be empty");
            field = value;
        }
    }
    
    public required DateTime DetectedAt
    {
        get;
        init
        {
            if (value.Kind != DateTimeKind.Utc)
                throw new ArgumentException("DetectedAt must be UTC");
            if (value > DateTime.UtcNow.AddSeconds(1))
                System.Diagnostics.Debug.WriteLine("Warning: DetectedAt in future");
            field = value;
        }
    }
    
    // Custom validation for Deleted events
    public void Validate()
    {
        if (ChangeType == DocumentChangeType.Deleted && !string.IsNullOrEmpty(Document.Content))
        {
            System.Diagnostics.Debug.WriteLine("Warning: Deleted event has non-empty content");
        }
    }
}
```

---

## Change Event Patterns

### Pattern: New File Created

```csharp
new DocumentChange
{
    ChangeType = DocumentChangeType.Created,
    Document = new()
    {
        Id = "cards/new-file.md",
        Content = "# New Flashcards\n- term: definition",
        LastModified = DateTime.UtcNow.AddSeconds(-1),
        Encoding = "utf-8"
    },
    DetectedAt = DateTime.UtcNow
}
```

### Pattern: File Updated

```csharp
new DocumentChange
{
    ChangeType = DocumentChangeType.Modified,
    Document = new()
    {
        Id = "cards/spanish.md",
        Content = "# Updated Spanish Vocab\n- nuevo contenido",
        LastModified = DateTime.Parse("2026-05-02T14:30:00Z"),
        Encoding = "utf-8"
    },
    DetectedAt = DateTime.Parse("2026-05-02T14:30:00.100Z")
}
```

### Pattern: File Deleted

```csharp
new DocumentChange
{
    ChangeType = DocumentChangeType.Deleted,
    Document = new()
    {
        Id = "cards/removed-file.md",
        Content = "", // Empty for deleted
        LastModified = DateTime.Parse("2026-05-02T14:25:00Z"), // Last known time
        Encoding = "utf-8"
    },
    DetectedAt = DateTime.Parse("2026-05-02T14:30:00Z")
}
```

---

## Consumer Usage Patterns

### Pattern: Process Incoming Changes

```csharp
await foreach (var change in source.SubscribeToChangesAsync(cts.Token))
{
    switch (change.ChangeType)
    {
        case DocumentChangeType.Created:
            await _cardService.AddCardsFromDocumentAsync(change.Document);
            break;
            
        case DocumentChangeType.Modified:
            await _cardService.UpdateCardsFromDocumentAsync(change.Document);
            break;
            
        case DocumentChangeType.Deleted:
            await _cardService.RemoveCardsFromDocumentAsync(change.Document.Id);
            break;
    }
    
    _logger.LogInformation(
        "Change detected: {ChangeType} for {DocumentId} at {DetectedAt}",
        change.ChangeType,
        change.Document.Id,
        change.DetectedAt);
}
```

### Pattern: Update UI Reactively

```csharp
// In Blazor component
private async Task MonitorChangesAsync()
{
    await foreach (var change in _source.SubscribeToChangesAsync(_cts.Token))
    {
        switch (change.ChangeType)
        {
            case DocumentChangeType.Created:
                await _cardCache.AddAsync(change.Document);
                break;
            case DocumentChangeType.Modified:
                await _cardCache.UpdateAsync(change.Document);
                break;
            case DocumentChangeType.Deleted:
                await _cardCache.RemoveAsync(change.Document.Id);
                break;
        }
        
        // Trigger UI refresh
        StateHasChanged();
    }
}
```

### Pattern: Batch Changes

```csharp
// Collect changes over time window
var changes = new List<DocumentChange>();
var timeout = DateTime.UtcNow.AddSeconds(5);

await foreach (var change in source.SubscribeToChangesAsync(cts.Token))
{
    changes.Add(change);
    
    if (DateTime.UtcNow > timeout || changes.Count >= 100)
    {
        // Process batch
        await _service.ProcessBatchAsync(changes);
        changes.Clear();
        timeout = DateTime.UtcNow.AddSeconds(5);
    }
}
```

---

## Serialization

### JSON

```json
{
  "changeType": "Modified",
  "document": {
    "id": "cards/spanish.md",
    "content": "# Spanish Vocabulary\n- padre: father",
    "lastModified": "2026-05-02T14:30:00Z",
    "encoding": "utf-8"
  },
  "detectedAt": "2026-05-02T14:30:00.100Z"
}
```

### Protocol Buffer (if needed)

```protobuf
message DocumentChange {
  DocumentChangeType change_type = 1;
  DocumentContent document = 2;
  string detected_at = 3;  // ISO 8601 string
}

enum DocumentChangeType {
  CREATED = 0;
  MODIFIED = 1;
  DELETED = 2;
}
```

---

## Testing Requirements

### Unit Tests

```csharp
[Fact]
public void DocumentChange_Created_ContainsFullContent()
{
    var doc = new DocumentContent { ... };
    var change = new DocumentChange
    {
        ChangeType = DocumentChangeType.Created,
        Document = doc,
        DetectedAt = DateTime.UtcNow
    };
    
    Assert.NotEmpty(change.Document.Content);
    Assert.Equal(doc.Id, change.Document.Id);
}

[Fact]
public void DocumentChange_Deleted_HasEmptyContent()
{
    var doc = new DocumentContent { Content = "", ... };
    var change = new DocumentChange
    {
        ChangeType = DocumentChangeType.Deleted,
        Document = doc,
        DetectedAt = DateTime.UtcNow
    };
    
    Assert.Empty(change.Document.Content);
}

[Fact]
public void DocumentChange_DetectedAt_MustBeUtc()
{
    var change = new DocumentChange
    {
        ChangeType = DocumentChangeType.Modified,
        Document = new() { ... },
        DetectedAt = DateTime.Parse("2026-05-02T14:30:00", CultureInfo.InvariantCulture)
        // Missing .Z timezone indicator - should fail
    };
    
    // Should throw or warn
}
```

---

## Performance Considerations

- **Immutability**: Safe to share across async contexts without copying
- **Equality**: Records provide value-based equality; can compare changes
- **Hashing**: Can be used in collections (HashSet, Dictionary keys)
- **Memory**: Including full content increases message size; acceptable per spec

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2026-05-02 | Initial contract |

---

## Related Documents

- [IDocumentSource Contract](idocument-source-contract.md)
- [DocumentContent Contract](document-content-contract.md)
- [Data Model](../data-model.md)
- [Research](../research.md)
