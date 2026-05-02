namespace Mnemi.Application.Ports.Source;

/// <summary>
/// Configuration for a document source adapter.
/// </summary>
/// <remarks>
/// This class provides a flexible configuration model that allows different adapter types
/// to be configured with their own specific parameters via the AdapterSpecificConfig dictionary.
/// This enables loose coupling between the core port interface and specific adapter implementations.
/// </remarks>
public class DocumentSourceConfig
{
    /// <summary>
    /// The type of adapter this configuration is for.
    /// </summary>
    public required DocumentSourceAdapterType AdapterType { get; init; }

    /// <summary>
    /// A unique identifier for this document source instance.
    /// Used for logging and error context.
    /// </summary>
    public required string SourceId { get; init; }

    /// <summary>
    /// Adapter-specific configuration parameters.
    /// </summary>
    /// <remarks>
    /// Common keys by adapter type:
    /// - FileSystem: "DirectoryPath" (required), "SearchPattern" (optional, default "*.md")
    /// - GoogleDrive: "FolderId" (required), "AuthToken" (required), "PollingIntervalMs" (optional, default 5000)
    /// </remarks>
    public Dictionary<string, object> AdapterSpecificConfig { get; init; } = new();

    /// <summary>
    /// Validates that the configuration has all required parameters for its adapter type.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when required configuration is missing.</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(SourceId))
            throw new ArgumentException("SourceId is required.", nameof(SourceId));

        switch (AdapterType)
        {
            case DocumentSourceAdapterType.FileSystem:
                if (!AdapterSpecificConfig.ContainsKey("DirectoryPath") || 
                    string.IsNullOrWhiteSpace(AdapterSpecificConfig["DirectoryPath"]?.ToString()))
                    throw new ArgumentException(
                        "FileSystem adapter requires 'DirectoryPath' in AdapterSpecificConfig.",
                        nameof(AdapterSpecificConfig));
                break;

            case DocumentSourceAdapterType.GoogleDrive:
                if (!AdapterSpecificConfig.ContainsKey("FolderId") ||
                    string.IsNullOrWhiteSpace(AdapterSpecificConfig["FolderId"]?.ToString()))
                    throw new ArgumentException(
                        "GoogleDrive adapter requires 'FolderId' in AdapterSpecificConfig.",
                        nameof(AdapterSpecificConfig));

                if (!AdapterSpecificConfig.ContainsKey("AuthToken") ||
                    string.IsNullOrWhiteSpace(AdapterSpecificConfig["AuthToken"]?.ToString()))
                    throw new ArgumentException(
                        "GoogleDrive adapter requires 'AuthToken' in AdapterSpecificConfig.",
                        nameof(AdapterSpecificConfig));
                break;

            default:
                throw new ArgumentException($"Unknown adapter type: {AdapterType}.", nameof(AdapterType));
        }
    }
}
