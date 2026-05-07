namespace Mnemi.Application.Ports.Source;

/// <summary>
/// Represents the content of a single document from a document source.
/// </summary>
/// <remarks>
/// This record encapsulates a document's identity, content, modification time, and encoding.
/// It is used both for read operations and within change notifications.
/// </remarks>
public record DocumentContent(
    string Id,
    string Content,
    DateTimeOffset LastModified,
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
