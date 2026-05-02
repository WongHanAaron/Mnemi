namespace Mnemi.Application.Ports.Source;

/// <summary>
/// Represents the content of a single document from a document source.
/// </summary>
/// <remarks>
/// This record encapsulates a document's identity, content, modification time, and encoding.
/// It is used both for read operations and within change notifications.
/// </remarks>
public record DocumentContent(
    /// <summary>
    /// The unique identifier of the document within its source.
    /// For file system sources, this is typically the full path or relative path.
    /// For Google Drive sources, this is the file ID.
    /// </summary>
    string Id,

    /// <summary>
    /// The complete content of the document.
    /// For deleted documents in change notifications, this will be empty.
    /// </summary>
    string Content,

    /// <summary>
    /// The UTC timestamp indicating when the document was last modified.
    /// </summary>
    DateTime LastModified,

    /// <summary>
    /// The encoding name of the document content (e.g., "utf-8", "utf-16").
    /// Must be a valid .NET encoding name recognized by System.Text.Encoding.GetEncoding().
    /// </summary>
    string Encoding
)
{
    /// <summary>
    /// Validates that the DocumentContent has valid property values.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when Id is empty or Encoding is invalid.</exception>
    public DocumentContent Validate()
    {
        if (string.IsNullOrWhiteSpace(Id))
            throw new ArgumentException("Id cannot be empty or whitespace.", nameof(Id));

        try
        {
            System.Text.Encoding.GetEncoding(Encoding);
        }
        catch (ArgumentException)
        {
            throw new ArgumentException($"Encoding '{Encoding}' is not a valid .NET encoding name.", nameof(Encoding));
        }

        return this;
    }
}
