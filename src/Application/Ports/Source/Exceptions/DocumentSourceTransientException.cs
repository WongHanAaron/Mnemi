using Mnemi.Application.Ports.Source;

namespace Mnemi.Application.Ports.Source.Exceptions;

/// <summary>
/// Thrown when a transient error occurs that may be recovered with retry.
/// </summary>
public class DocumentSourceTransientException : DocumentSourceException
{
    /// <summary>
    /// The number of seconds to wait before retrying the operation.
    /// </summary>
    public int RetryAfterSeconds { get; }

    /// <summary>
    /// Creates a new instance of DocumentSourceTransientException.
    /// </summary>
    public DocumentSourceTransientException(
        string message,
        string sourceId,
        DocumentSourceAdapterType adapterType,
        int retryAfterSeconds = 0)
        : base(message, sourceId, adapterType)
    {
        RetryAfterSeconds = retryAfterSeconds;
    }

    /// <summary>
    /// Creates a new instance of DocumentSourceTransientException with inner exception.
    /// </summary>
    public DocumentSourceTransientException(
        string message,
        string sourceId,
        DocumentSourceAdapterType adapterType,
        int retryAfterSeconds,
        Exception? innerException)
        : base(message, sourceId, adapterType, innerException)
    {
        RetryAfterSeconds = retryAfterSeconds;
    }
}
