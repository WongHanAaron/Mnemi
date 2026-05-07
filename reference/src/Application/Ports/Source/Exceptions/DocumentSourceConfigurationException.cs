using Mnemi.Application.Ports.Source;

namespace Mnemi.Application.Ports.Source.Exceptions;

/// <summary>
/// Thrown when a document source configuration is invalid or incomplete.
/// </summary>
public class DocumentSourceConfigurationException : DocumentSourceException
{
    /// <summary>
    /// Creates a new instance of DocumentSourceConfigurationException.
    /// </summary>
    public DocumentSourceConfigurationException(string message, string sourceId, DocumentSourceAdapterType adapterType)
        : base(message, sourceId, adapterType)
    {
    }

    /// <summary>
    /// Creates a new instance of DocumentSourceConfigurationException with inner exception.
    /// </summary>
    public DocumentSourceConfigurationException(
        string message,
        string sourceId,
        DocumentSourceAdapterType adapterType,
        Exception? innerException)
        : base(message, sourceId, adapterType, innerException)
    {
    }
}
