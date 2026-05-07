using Mnemi.Application.Ports.Source;

namespace Mnemi.Adapter.Source.FileSystem;

/// <summary>
/// Factory for creating configured FileSystemDocumentSource instances.
/// </summary>
public static class FileSystemAdapterFactory
{
    /// <summary>
    /// Creates a FileSystemDocumentSource configured to read markdown files from a directory.
    /// </summary>
    /// <param name="sourceId">Unique identifier for this source</param>
    /// <param name="directoryPath">Absolute or relative path to the directory to scan</param>
    /// <param name="searchPattern">File search pattern (default: "*.md")</param>
    /// <returns>Configured IDocumentSource instance</returns>
    /// <exception cref="ArgumentException">
    /// Thrown if sourceId is empty or directoryPath is empty
    /// </exception>
    public static IDocumentSource CreateMarkdownSource(
        string sourceId,
        string directoryPath,
        string searchPattern = "*.md")
    {
        if (string.IsNullOrWhiteSpace(sourceId))
            throw new ArgumentException("Source ID cannot be empty", nameof(sourceId));

        if (string.IsNullOrWhiteSpace(directoryPath))
            throw new ArgumentException("Directory path cannot be empty", nameof(directoryPath));

        var config = new DocumentSourceConfig
        {
            AdapterType = DocumentSourceAdapterType.FileSystem,
            SourceId = sourceId,
            AdapterSpecificConfig = new()
            {
                { "DirectoryPath", directoryPath },
                { "SearchPattern", searchPattern }
            }
        };

        return new FileSystemDocumentSource(config);
    }

    /// <summary>
    /// Creates a FileSystemDocumentSource with custom configuration.
    /// </summary>
    /// <param name="config">Full configuration object</param>
    /// <returns>Configured IDocumentSource instance</returns>
    /// <exception cref="DocumentSourceConfigurationException">
    /// Thrown if configuration is invalid
    /// </exception>
    public static IDocumentSource CreateFromConfig(DocumentSourceConfig config)
    {
        return new FileSystemDocumentSource(config);
    }
}
