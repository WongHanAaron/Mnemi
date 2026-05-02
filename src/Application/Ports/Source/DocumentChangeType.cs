namespace Mnemi.Application.Ports.Source;

/// <summary>
/// Represents the type of change detected in a document source.
/// </summary>
public enum DocumentChangeType
{
    /// <summary>
    /// A new document was created in the source.
    /// </summary>
    Created = 1,

    /// <summary>
    /// An existing document was modified in the source.
    /// </summary>
    Modified = 2,

    /// <summary>
    /// An existing document was deleted from the source.
    /// </summary>
    Deleted = 3
}
