# DocumentContent Contract

**Date**: May 2, 2026  
**Type**: Data Model Contract  
**Related**: [IDocumentSource Contract](idocument-source-contract.md), [DocumentChange Contract](document-change-contract.md)

## Overview

`DocumentContent` is a read-only record type that represents a single document retrieved from a source. It serves as the data transfer object (DTO) between adapters and consumers.

---

## Type Definition

```csharp
namespace Mnemi.Application.Ports
{
    /// <summary>
    /// Represents the content of a single document from a source.
    /// Immutable value object designed for safe sharing across async contexts.
    /// </summary>
    public record DocumentContent
    {
        /// <summary>
        /// Unique identifier for the document within its source.
        /// 
        /// File system adapter: Relative path from monitored directory (e.g., "cards/spanish-vocab.md")
        /// Google Drive adapter: File ID (e.g., "1ABC-xyz123")
        /// 
        /// This ID is stable across the document's lifetime and uniquely identifies it within the source.
        /// </summary>
        public required string Id { get; init; }
        
        /// <summary>
        /// Full text content of the document.
        /// 
        /// Encoding: Preserved as returned by source, specified in Encoding property
        /// Size: No maximum enforced; implementations may choose to limit (e.g., max 100MB)
        /// Content: Can be empty for 0-byte files; never null
        /// 
        /// For markdown sources, this is the raw markdown text (not parsed).
        /// </summary>
        public required string Content { get; init; }
        
        /// <summary>
        /// ISO 8601 timestamp of the document's last modification in the source.
        /// 
        /// Format: "2026-05-02T14:30:00Z" (UTC with 'Z' suffix)
        /// Accuracy: Per-second precision for file system; per-second for Google Drive
        /// Usage: Change detection, cache invalidation, sort ordering
        /// 
        /// Should not be in the future (implementation should validate and clamp if necessary).
        /// </summary>
        public required DateTime LastModified { get; init; }
        
        /// <summary>
        /// Name of the text encoding used to read the document.
        /// 
        /// Common values: "utf-8", "utf-16", "ascii", "iso-8859-1"
        /// Format: Standard encoding name accepted by System.Text.Encoding.GetEncoding(name)
        /// Usage: Consumers can use this to re-encode or validate text
        /// 
        /// Default for file system adapter: "utf-8"
        /// Default for Google Drive adapter: "utf-8"
        /// </summary>
        public required string Encoding { get; init; }
    }
}
```

---

## Field Specifications

### Id

| Property | Value |
|----------|-------|
| Type | `string` |
| Required | Yes |
| Immutable | Yes (init-only) |
| Null/Empty | Not allowed |
| Uniqueness | Must be unique within source |
| Stability | Stable across document lifetime |

**File System Adapter Rules**:
- Relative path from monitored directory
- Example: `"cards/verbs.md"` for file at `/home/user/flashcards/cards/verbs.md`
- Should be normalized: forward slashes on all platforms
- Must be URL-safe (encode special characters if needed)

**Google Drive Adapter Rules**:
- File ID from Google Drive API
- Example: `"1a2b3c4d5e6f7g8h9i0j1k2l3m4n5o6p"`
- Stable across API calls; does not change when file is renamed
- Unique within Google account

---

### Content

| Property | Value |
|----------|-------|
| Type | `string` |
| Required | Yes |
| Immutable | Yes (init-only) |
| Null | Not allowed |
| Empty | Allowed (for 0-byte files) |
| Maximum Length | Implementation-defined; typically no limit |

**Rules**:
- Raw content as read from source (for markdown files, not pre-parsed)
- Encoding conversion applied per `Encoding` property
- Line endings preserved as-is from source (CRLF on Windows, LF elsewhere)
- No additional processing or normalization

**Example**:
```
# Spanish Vocabulary
## Family

- padre: father
- madre: mother
```

---

### LastModified

| Property | Value |
|----------|-------|
| Type | `DateTime` |
| Required | Yes |
| Immutable | Yes (init-only) |
| Format | ISO 8601 with UTC timezone |
| Precision | Per-second (or finer if source supports) |
| Kind | Must be UTC (Kind = DateTimeKind.Utc) |

**Rules**:
- Always represent in UTC (never local time)
- If source provides only date (no time), use 00:00:00 UTC
- Should never be in the future (validate and clamp if necessary)
- Used for change detection: newer LastModified → document modified

**Examples**:
```csharp
DateTime.Parse("2026-05-02T14:30:00Z");  // May 2, 2026 at 2:30 PM UTC
DateTime.Parse("2026-05-02T00:00:00Z");  // Midnight UTC (date only)
```

---

### Encoding

| Property | Value |
|----------|-------|
| Type | `string` |
| Required | Yes |
| Immutable | Yes (init-only) |
| Format | Standard .NET encoding name |
| Case | Case-insensitive; standardize to lowercase |
| Validation | Must be accepted by `System.Text.Encoding.GetEncoding()` |

**Valid Values**:
- `"utf-8"` (recommended default)
- `"utf-16"` / `"utf-16le"` / `"utf-16be"`
- `"ascii"` / `"us-ascii"`
- `"iso-8859-1"` / `"latin1"`
- `"cp1252"` (Windows-1252)
- And others supported by .NET

**Rules**:
- Standardize to lowercase: `"UTF-8"` → `"utf-8"`
- File system adapter: Detect from BOM if present; default to "utf-8"
- Google Drive adapter: Google Drive returns UTF-8; default to "utf-8"
- Store the actual encoding used, not assumed

**Usage**:
```csharp
var encoding = System.Text.Encoding.GetEncoding(doc.Encoding);
var bytes = encoding.GetBytes(doc.Content);
```

---

## Validation Rules

| Field | Rule | Enforcement |
|-------|------|-------------|
| Id | Not null or empty | Exception in constructor |
| Id | Must be unique per source | Consumer's responsibility to verify |
| Content | Can be empty string | Allowed (0-byte files) |
| Content | Cannot be null | Exception in constructor |
| LastModified | Kind must be UTC | Exception if not UTC |
| LastModified | Should not be future | Warning log if violated; clamp if possible |
| Encoding | Must be valid .NET encoding name | Exception in constructor |

**Example Validation**:
```csharp
public record DocumentContent
{
    public required string Id 
    { 
        get; 
        init
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Id cannot be empty");
            field = value;
        }
    }
    
    // ... other properties
    
    public required DateTime LastModified
    {
        get;
        init
        {
            if (value.Kind != DateTimeKind.Utc)
                throw new ArgumentException("LastModified must be UTC");
            if (value > DateTime.UtcNow.AddSeconds(5))
                System.Diagnostics.Debug.WriteLine("Warning: LastModified in future");
            field = value;
        }
    }
}
```

---

## Usage Examples

### File System Adapter

```csharp
var doc = new DocumentContent
{
    Id = "cards/spanish.md",
    Content = "# Spanish Vocab\n- hola: hello",
    LastModified = File.GetLastWriteTimeUtc("./cards/spanish.md"),
    Encoding = "utf-8"
};
```

### Google Drive Adapter

```csharp
var file = await driveService.Files.Get(fileId).ExecuteAsync();
var content = await DownloadFileAsStringAsync(fileId);

var doc = new DocumentContent
{
    Id = file.Id,
    Content = content,
    LastModified = file.ModifiedTime?.ToUniversalTime() ?? DateTime.UtcNow,
    Encoding = "utf-8"
};
```

### Consumer Usage

```csharp
await foreach (var doc in source.ReadAllAsync())
{
    // Safe to compare documents by Id
    if (doc.Id == previousDoc.Id)
    {
        // Check if content changed
        if (doc.LastModified > previousDoc.LastModified)
        {
            // Document was updated
        }
    }
    
    // Use encoding for special handling
    if (doc.Encoding != "utf-8")
    {
        Console.WriteLine($"Warning: {doc.Id} uses non-UTF-8 encoding: {doc.Encoding}");
    }
    
    // Process content
    var lines = doc.Content.Split('\n');
}
```

---

## Serialization

### JSON

```json
{
  "id": "cards/spanish.md",
  "content": "# Spanish Vocabulary\n- padre: father",
  "lastModified": "2026-05-02T14:30:00Z",
  "encoding": "utf-8"
}
```

### XML

```xml
<DocumentContent>
  <Id>cards/spanish.md</Id>
  <Content>...content...</Content>
  <LastModified>2026-05-02T14:30:00Z</LastModified>
  <Encoding>utf-8</Encoding>
</DocumentContent>
```

---

## Performance Considerations

- **Memory**: Content can be large (up to 100MB+); streaming via `IAsyncEnumerable` prevents loading all documents at once
- **Immutability**: Records are safe to share across async contexts
- **Equality**: Records provide value-based equality by default
- **Hashing**: Records are hashable and can be used in collections

---

## Compatibility Matrix

| Source | Id Format | LastModified Precision | Encoding |
|--------|-----------|------------------------|----------|
| File System | Relative path | Per-second | From file BOM or UTF-8 |
| Google Drive | File ID | Per-second | UTF-8 |

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2026-05-02 | Initial contract |

---

## Related Documents

- [IDocumentSource Contract](idocument-source-contract.md)
- [DocumentChange Contract](document-change-contract.md)
- [Data Model](../data-model.md)
