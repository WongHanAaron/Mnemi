using Mnemi.Application.Ports.Source;
using Mnemi.Application.Ports.Source.Exceptions;

namespace Mnemi.Adapter.Source.FileSystem;

/// <summary>
/// Validates configuration parameters specific to the FileSystem adapter.
/// </summary>
internal class FileSystemConfigValidator
{
    /// <summary>
    /// Validates that FileSystem adapter configuration contains all required parameters.
    /// </summary>
    /// <param name="config">The DocumentSourceConfig to validate.</param>
    /// <exception cref="DocumentSourceConfigurationException">Thrown if required parameters are missing.</exception>
    public static void Validate(DocumentSourceConfig config)
    {
        if (config.AdapterType != DocumentSourceAdapterType.FileSystem)
            throw new DocumentSourceConfigurationException(
                "Attempted to validate non-FileSystem adapter configuration with FileSystemConfigValidator.",
                config.SourceId,
                config.AdapterType);

        const string directoryPathKey = "DirectoryPath";
        if (!config.AdapterSpecificConfig.TryGetValue(directoryPathKey, out var directoryPathObj) ||
            directoryPathObj is not string directoryPath ||
            string.IsNullOrWhiteSpace(directoryPath))
        {
            throw new DocumentSourceConfigurationException(
                $"FileSystem adapter requires '{directoryPathKey}' parameter in AdapterSpecificConfig.",
                config.SourceId,
                config.AdapterType);
        }

        if (!Directory.Exists(directoryPath))
        {
            throw new DocumentSourceConfigurationException(
                $"Configured directory path does not exist: '{directoryPath}'.",
                config.SourceId,
                config.AdapterType);
        }

        // SearchPattern is optional, default to "*.md"
        if (config.AdapterSpecificConfig.TryGetValue("SearchPattern", out var searchPatternObj))
        {
            if (searchPatternObj is not string searchPattern || string.IsNullOrWhiteSpace(searchPattern))
            {
                throw new DocumentSourceConfigurationException(
                    "If specified, 'SearchPattern' must be a non-empty string.",
                    config.SourceId,
                    config.AdapterType);
            }
        }
    }

    /// <summary>
    /// Gets the directory path from configuration with validation.
    /// </summary>
    public static string GetDirectoryPath(DocumentSourceConfig config)
    {
        return (config.AdapterSpecificConfig["DirectoryPath"] as string)!;
    }

    /// <summary>
    /// Gets the search pattern from configuration, defaulting to "*.md" if not specified.
    /// </summary>
    public static string GetSearchPattern(DocumentSourceConfig config)
    {
        return config.AdapterSpecificConfig.TryGetValue("SearchPattern", out var pattern)
            ? (pattern as string)!
            : "*.md";
    }
}
