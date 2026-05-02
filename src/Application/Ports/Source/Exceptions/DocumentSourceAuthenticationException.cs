using Mnemi.Application.Ports.Source;

namespace Mnemi.Application.Ports.Source.Exceptions;

/// <summary>
/// Thrown when authentication fails for a document source.
/// </summary>
public class DocumentSourceAuthenticationException : DocumentSourceException
{
    /// <summary>
    /// Creates a new instance of DocumentSourceAuthenticationException.
    /// </summary>
    public DocumentSourceAuthenticationException(string message, string sourceId, DocumentSourceAdapterType adapterType)
        : base(message, sourceId, adapterType)
    {
    }

    /// <summary>
    /// Creates a new instance of DocumentSourceAuthenticationException with inner exception.
    /// </summary>
    public DocumentSourceAuthenticationException(
        string message,
        string sourceId,
        DocumentSourceAdapterType adapterType,
        Exception? innerException)
        : base(message, sourceId, adapterType, innerException)
    {
    }
}
