# Data Model: File Synchronization Interfaces

**Date**: May 2, 2026  
**Feature**: [spec.md](spec.md) | [plan.md](plan.md)  
**Related**: [research.md](research.md), [contracts/](contracts/)

## Overview

This document defines all data structures, entities, and models used by the `IDocumentSource` adapter interface and its implementations.

---

## Enumerations

### DocumentChangeType

Represents the type of change detected in a directory.

```csharp
public enum DocumentChangeType
{
    /// <summary>New document created in source</summary>
    Created = 1,
    
    /// <summary>Existing document modified in source</summary>
    Modified = 2,
    
    /// <summary>Document deleted from source</summary>
    Deleted = 3
}
```

**Notes**:
- Renames are treated as delete + create per spec assumptions
- Values are intentionally spaced for potential future expansion

### DocumentSourceAdapterType

Identifies which adapter implementation should handle a configuration.

```csharp
public enum DocumentSourceAdapterType
{
    /// <summary>Local file system directories</summary>
    FileSystem = 1,
    
    /// <summary>Google Drive folders</summary>
    GoogleDrive = 2
}
```

---

## Core Models

### DocumentContent

Represents the content of a single document retrieved from a source.

```csharp
public record DocumentContent
{
    /// <summary>
    /// Unique identifier for the document within its source.
    /// File system: relative file path from source directory
    /// Google Drive: file ID (string)
    /// </summary>
    public required string Id { get; init; }
    
    /// <summary>
    /// Full text content of the document.
    /// Encoding preserved as specified by source.
    /// </summary>
    public required string Content { get; init; }
    
    /// <summary>
    /// ISO 8601 timestamp of last modification in source.
    /// Enables change detection and cache invalidation.
    /// </summary>
    public required DateTime LastModified { get; init; }
    
    /// <summary>
    /// Text encoding name (e.g., "utf-8", "ascii", "utf-16").
    /// Used by consumers to understand content encoding.
    /// </summary>
    public required string Encoding { get; init; }
}
```

**Validation Rules**:
- `Id` must not be null or empty; must be unique within source
- `Content` can be empty (valid for 0-byte files)
- `LastModified` should not be in the future
- `Encoding` must be a valid TextEncoding name or common alias

**Constraints**:
- No maximum size enforced (left to implementation)
- Should be treated as immutable after creation (record property)

---

### DocumentChange

Represents a single change event in a monitored directory.

```csharp
public record DocumentChange
{
    /// <summary>
    /// Type of change: Created, Modified, or Deleted
    /// </summary>
    public required DocumentChangeType ChangeType { get; init; }
    
    /// <summary>
    /// The document involved in the change.
    /// For Deleted events: Content will be empty; Id identifies deleted file
    /// For Created/Modified events: Full content is included
    /// </summary>
    public required DocumentContent Document { get; init; }
    
    /// <summary>
    /// ISO 8601 timestamp when the change was detected.
    /// Note: May differ from Document.LastModified (detection delay)
    /// </summary>
    public required DateTime DetectedAt { get; init; }
}
```

**Validation Rules**:
- `ChangeType` must be a valid enumeration value
- `Document` must not be null
- `DetectedAt` must not be in the future
- For Deleted events: `Document.Content` should be empty string

**Constraints**:
- Inclusion of full content in every change notification (per spec clarification)
- Enables immediate processing without separate read operation

---

### DocumentSourceConfig

Configuration object for initializing a document source adapter.

```csharp
public class DocumentSourceConfig
{
    /// <summary>
    /// Type of adapter to use (FileSystem, GoogleDrive)
    /// </summary>
    public required DocumentSourceAdapterType AdapterType { get; set; }
    
    /// <summary>
    /// Unique identifier for this source instance.
    /// Used for logging, debugging, and multi-source disambiguation.
    /// Examples: "markdown-cards", "study-materials-drive"
    /// </summary>
    public required string SourceId { get; set; }
    
    /// <summary>
    /// Adapter-specific configuration as a flexible property dictionary.
    /// Allows extensibility without modifying this class.
    /// 
    /// File System adapter expected keys:
    /// - "DirectoryPath" (string): Local directory path
    /// - "SearchPattern" (string, optional): File extension filter (default: "*.md")
    /// 
    /// Google Drive adapter expected keys:
    /// - "FolderId" (string): Google Drive folder ID
    /// - "DriveType" (string, optional): "drive" or "shared" (default: "drive")
    /// - "AuthToken" (string or ICredential): OAuth2 credential
    /// - "PollingIntervalMs" (int, optional): Polling interval in milliseconds (default: 5000)
    /// </summary>
    public Dictionary<string, object> AdapterSpecificConfig { get; set; } = new();
}
```

**Validation Rules**:
- `AdapterType` must be a valid enumeration value
- `SourceId` must not be null or empty; should be human-readable and unique per application instance
- `AdapterSpecificConfig` must contain required keys for chosen adapter type
  - Missing required keys should throw `ArgumentException` during adapter initialization

**Example Configurations**:

```csharp
// File System
var fsConfig = new DocumentSourceConfig
{
    AdapterType = DocumentSourceAdapterType.FileSystem,
    SourceId = "local-cards",
    AdapterSpecificConfig = new()
    {
        ["DirectoryPath"] = "/home/user/flashcards",
        ["SearchPattern"] = "*.md"
    }
};

// Google Drive
var gdConfig = new DocumentSourceConfig
{
    AdapterType = DocumentSourceAdapterType.GoogleDrive,
    SourceId = "shared-study-materials",
    AdapterSpecificConfig = new()
    {
        ["FolderId"] = "1ABC-xyz123_ABC-xyz",
        ["DriveType"] = "drive",
        ["AuthToken"] = credential, // ICredential or token string
        ["PollingIntervalMs"] = 10000 // 10 second polling
    }
};
```

---

## Interface Definitions

### IDocumentSource

The main adapter interface for reading documents and subscribing to changes.

```csharp
/// <summary>
/// Adapter interface for reading document content and subscribing to changes
/// from pluggable sources (file system, cloud storage, etc.)
/// </summary>
public interface IDocumentSource
{
    /// <summary>
    /// Read all documents from the configured source and stream them asynchronously.
    /// 
    /// Returns an IAsyncEnumerable that yields each document as it is read,
    /// allowing efficient processing of large directories without loading all
    /// documents into memory at once.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>
    /// An async enumerable of DocumentContent objects representing all documents
    /// in the source matching configured filters.
    /// </returns>
    /// <exception cref="OperationCanceledException">
    /// Thrown if cancellationToken is cancelled before operation completes.
    /// </exception>
    /// <exception cref="DocumentSourceException">
    /// Thrown for configuration errors, authentication failures, or access issues.
    /// </exception>
    IAsyncEnumerable<DocumentContent> ReadAllAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Subscribe to changes in the source directory and receive notifications
    /// as documents are created, modified, or deleted.
    /// 
    /// Returns an IAsyncEnumerable that continuously yields change notifications.
    /// The enumeration continues until:
    /// 1. Cancellation token is triggered
    /// 2. An unrecoverable error occurs
    /// 3. The async enumeration is disposed
    /// </summary>
    /// <param name="cancellationToken">Token to cancel subscription</param>
    /// <returns>
    /// An async enumerable of DocumentChange objects representing directory changes.
    /// Each change includes the full document content and metadata.
    /// </returns>
    /// <exception cref="OperationCanceledException">
    /// Thrown if cancellationToken is cancelled.
    /// </exception>
    /// <exception cref="DocumentSourceException">
    /// Thrown for configuration errors, authentication failures, or fatal errors
    /// that prevent continued monitoring.
    /// </exception>
    /// <remarks>
    /// This operation is long-running. It's typically executed in a background task
    /// or via a hosted service. Always use CancellationToken to allow graceful shutdown.
    /// 
    /// For file system sources: Changes detected via FileSystemWatcher
    /// For Google Drive sources: Changes detected via periodic polling
    /// </remarks>
    IAsyncEnumerable<DocumentChange> SubscribeToChangesAsync(CancellationToken cancellationToken = default);
}
```

---

## Exception Hierarchy

### DocumentSourceException

Base exception for all document source adapter errors.

```csharp
/// <summary>
/// Base exception for document source adapter operations.
/// </summary>
public class DocumentSourceException : Exception
{
    public string SourceId { get; }
    public DocumentSourceAdapterType AdapterType { get; }
    
    public DocumentSourceException(
        string message, 
        string sourceId, 
        DocumentSourceAdapterType adapterType,
        Exception? innerException = null)
        : base(message, innerException)
    {
        SourceId = sourceId;
        AdapterType = adapterType;
    }
}

/// <summary>Thrown when adapter configuration is invalid</summary>
public class DocumentSourceConfigurationException : DocumentSourceException
{
    public DocumentSourceConfigurationException(
        string message, 
        string sourceId, 
        DocumentSourceAdapterType adapterType,
        Exception? innerException = null)
        : base(message, sourceId, adapterType, innerException) { }
}

/// <summary>Thrown when authentication fails</summary>
public class DocumentSourceAuthenticationException : DocumentSourceException
{
    public DocumentSourceAuthenticationException(
        string message, 
        string sourceId, 
        DocumentSourceAdapterType adapterType,
        Exception? innerException = null)
        : base(message, sourceId, adapterType, innerException) { }
}

/// <summary>Thrown when access to source is denied</summary>
public class DocumentSourceAccessException : DocumentSourceException
{
    public DocumentSourceAccessException(
        string message, 
        string sourceId, 
        DocumentSourceAdapterType adapterType,
        Exception? innerException = null)
        : base(message, sourceId, adapterType, innerException) { }
}

/// <summary>Thrown for transient errors that might be retried</summary>
public class DocumentSourceTransientException : DocumentSourceException
{
    public int? RetryAfterSeconds { get; }
    
    public DocumentSourceTransientException(
        string message, 
        string sourceId, 
        DocumentSourceAdapterType adapterType,
        int? retryAfterSeconds = null,
        Exception? innerException = null)
        : base(message, sourceId, adapterType, innerException)
    {
        RetryAfterSeconds = retryAfterSeconds;
    }
}
```

---

## Namespace Organization

```csharp
// Core interfaces and types (Application layer)
namespace Mnemi.Application.Ports
{
    public interface IDocumentSource { }
    public record DocumentContent { }
    public record DocumentChange { }
    public class DocumentSourceConfig { }
    public enum DocumentChangeType { }
    public enum DocumentSourceAdapterType { }
    public class DocumentSourceException { }
    // ... other exception types
}

// File system adapter
namespace Mnemi.Adapter.Source.FileSystem
{
    public class FileSystemDocumentSource : IDocumentSource { }
    public class FileSystemAdapter { }
}

// Google Drive adapter
namespace Mnemi.Adapter.Source.GoogleDrive
{
    public class GoogleDriveDocumentSource : IDocumentSource { }
    public class GoogleDriveAdapter { }
}
```

---

## Design Decisions

| Decision | Rationale |
|----------|-----------|
| Use `record` for immutable models | Ensures thread-safety and simple value comparison |
| Include full content in `DocumentChange` | Allows immediate processing without separate read calls |
| Use `IAsyncEnumerable` streaming | Memory-efficient for large document sets |
| Configuration as `Dictionary<string, object>` | Extensible without tight coupling to specific adapters |
| Exception hierarchy with context | Enables proper error handling and logging at application level |
| ISO 8601 timestamps | Standard format for serialization and cross-platform consistency |

---

## Related Documentation

- [IDocumentSource Interface Contract](contracts/idocument-source-contract.md)
- [DocumentContent Contract](contracts/document-content-contract.md)
- [DocumentChange Contract](contracts/document-change-contract.md)
- [Implementation Research](research.md)
