namespace Mnemi.Application.Ports.Source;

/// <summary>
/// Represents a change event detected in a document source.
/// </summary>
/// <remarks>
/// This record captures both the type of change (Created, Modified, Deleted) and the full
/// document content (if available). For Deleted events, the DocumentContent will have an empty Content field.
/// </remarks>
public record DocumentChange(
    /// <summary>
    /// The type of change that occurred (Created, Modified, or Deleted).
    /// </summary>
    DocumentChangeType ChangeType,

    /// <summary>
    /// The document involved in this change, including its full content.
    /// For Created and Modified events, Content is populated.
    /// For Deleted events, Content is empty but Id is preserved.
    /// </summary>
    DocumentContent Document,

    /// <summary>
    /// The UTC timestamp indicating when this change was detected by the source.
    /// </summary>
    DateTime DetectedAt
)
{
    /// <summary>
    /// Validates that the DocumentChange has consistent property values.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when change type is invalid or document is inconsistent.</exception>
    public DocumentChange Validate()
    {
        if (!Enum.IsDefined(typeof(DocumentChangeType), ChangeType))
            throw new ArgumentException($"ChangeType '{ChangeType}' is not a valid DocumentChangeType.", nameof(ChangeType));

        Document.Validate();

        // For Deleted events, Content should be empty
        if (ChangeType == DocumentChangeType.Deleted && !string.IsNullOrEmpty(Document.Content))
            throw new ArgumentException("Deleted events must have empty Content.", nameof(Document));

        return this;
    }
}
