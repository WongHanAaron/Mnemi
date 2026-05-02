namespace Mnemi.Application.Ports.Source;

/// <summary>
/// Represents a source for reading and subscribing to document changes.
/// </summary>
/// <remarks>
/// This interface defines the contract for all document source adapters (file system, Google Drive, etc.).
/// It provides two complementary streaming operations:
/// - ReadAllAsync: For one-time bulk reading of all documents from the source
/// - SubscribeToChangesAsync: For continuous monitoring of document changes
///
/// Both methods support cancellation via CancellationToken and follow the async streaming pattern using IAsyncEnumerable&lt;T&gt;.
/// </remarks>
public interface IDocumentSource
{
    /// <summary>
    /// Reads all documents from the source and yields them as a stream.
    /// </summary>
    /// <param name="cancellationToken">
    /// A token that can be used to request cancellation of the operation.
    /// When signaled, the enumeration will terminate gracefully.
    /// </param>
    /// <returns>
    /// An async enumerable of DocumentContent records representing all documents in the source.
    /// The enumeration is memory-efficient and processes documents one at a time.
    /// </returns>
    /// <exception cref="DocumentSourceConfigurationException">
    /// Thrown if the adapter is not properly configured.
    /// </exception>
    /// <exception cref="DocumentSourceAuthenticationException">
    /// Thrown if authentication with the source fails (e.g., invalid credentials for Google Drive).
    /// </exception>
    /// <exception cref="DocumentSourceAccessException">
    /// Thrown if access to the source is denied (e.g., permission denied for a file system directory).
    /// </exception>
    /// <exception cref="DocumentSourceTransientException">
    /// Thrown if a transient error occurs that may be recovered with retry (e.g., network timeout).
    /// </exception>
    /// <remarks>
    /// Behavior by adapter type:
    /// - FileSystem: Enumerates files in the configured directory matching the search pattern.
    /// - GoogleDrive: Lists files in the configured folder using pagination, downloading each file's content.
    ///
    /// If cancellation is requested, the enumeration terminates cleanly without throwing OperationCanceledException.
    /// </remarks>
    IAsyncEnumerable<DocumentContent> ReadAllAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Subscribes to changes in the source and yields them as a continuous stream.
    /// </summary>
    /// <param name="cancellationToken">
    /// A token that can be used to request cancellation of the subscription.
    /// When signaled, the enumeration will terminate and subscription will be cleaned up.
    /// </param>
    /// <returns>
    /// An async enumerable of DocumentChange records representing changes detected in the source.
    /// The stream continues until cancellation is requested or an unrecoverable error occurs.
    /// </returns>
    /// <exception cref="DocumentSourceConfigurationException">
    /// Thrown if the adapter is not properly configured.
    /// </exception>
    /// <exception cref="DocumentSourceAuthenticationException">
    /// Thrown if authentication with the source fails.
    /// </exception>
    /// <exception cref="DocumentSourceAccessException">
    /// Thrown if access to the source is denied.
    /// </exception>
    /// <exception cref="DocumentSourceTransientException">
    /// Thrown if a transient error occurs during monitoring.
    /// </exception>
    /// <remarks>
    /// Behavior by adapter type:
    /// - FileSystem: Uses FileSystemWatcher to detect local file changes (Created, Modified, Deleted).
    ///   Changes are typically detected within milliseconds but may be debounced for rapid consecutive changes.
    /// - GoogleDrive: Uses polling to detect changes in the configured folder.
    ///   The polling interval is configurable and defaults to 5 seconds.
    ///
    /// For all changes (Created, Modified, Deleted), the DocumentChange includes the full document content.
    /// For Deleted events, the content is empty but the document ID is preserved.
    ///
    /// Multiple concurrent subscribers are supported. Each subscription is independent and receives its own stream of events.
    ///
    /// If cancellation is requested, the subscription terminates cleanly and resources are released.
    /// </remarks>
    IAsyncEnumerable<DocumentChange> SubscribeToChangesAsync(CancellationToken cancellationToken);
}
