using Mnemi.Application.Ports.Source;

namespace Mnemi.Application.Ports.Source.Exceptions;

/// <summary>
/// Base exception for all document source related errors.
/// </summary>
public class DocumentSourceException : Exception
{
    /// <summary>
    /// The unique identifier of the source where the error occurred.
    /// </summary>
    public string SourceId { get; }

    /// <summary>
    /// The type of adapter that was in use when the error occurred.
    /// </summary>
    public DocumentSourceAdapterType AdapterType { get; }

    /// <summary>
    /// Creates a new instance of DocumentSourceException.
    /// </summary>
    public DocumentSourceException(string message, string sourceId, DocumentSourceAdapterType adapterType)
        : base(message)
    {
        SourceId = sourceId;
        AdapterType = adapterType;
    }

    /// <summary>
    /// Creates a new instance of DocumentSourceException with inner exception.
    /// </summary>
    public DocumentSourceException(
        string message,
        string sourceId,
        DocumentSourceAdapterType adapterType,
        Exception? innerException)
        : base(message, innerException)
    {
        SourceId = sourceId;
        AdapterType = adapterType;
    }
}
