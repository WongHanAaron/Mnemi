using Mnemi.Application.Ports.Source;

namespace Mnemi.Application.Ports.Source.Exceptions;

/// <summary>
/// Thrown when access is denied to a document source or its content.
/// </summary>
public class DocumentSourceAccessException : DocumentSourceException
{
    /// <summary>
    /// Creates a new instance of DocumentSourceAccessException.
    /// </summary>
    public DocumentSourceAccessException(string message, string sourceId, DocumentSourceAdapterType adapterType)
        : base(message, sourceId, adapterType)
    {
    }

    /// <summary>
    /// Creates a new instance of DocumentSourceAccessException with inner exception.
    /// </summary>
    public DocumentSourceAccessException(
        string message,
        string sourceId,
        DocumentSourceAdapterType adapterType,
        Exception? innerException)
        : base(message, sourceId, adapterType, innerException)
    {
    }
}
