using Google.Apis.Drive.v3;
using Mnemi.Application.Ports.Source;

namespace Mnemi.Adapter.Source.GoogleDrive;

/// <summary>
/// Factory for creating configured GoogleDriveDocumentSource instances.
/// </summary>
public static class GoogleDriveAdapterFactory
{
    /// <summary>
    /// Creates a GoogleDriveDocumentSource configured to read files from a Google Drive folder.
    /// </summary>
    /// <param name="sourceId">Unique identifier for this source</param>
    /// <param name="folderId">Google Drive folder ID to scan</param>
    /// <param name="authToken">OAuth token with Drive read permissions</param>
    /// <param name="pollingIntervalMs">Poll interval in milliseconds (default: 5000)</param>
    /// <returns>Configured IDocumentSource instance</returns>
    /// <exception cref="ArgumentException">
    /// Thrown if sourceId, folderId, or authToken are empty
    /// </exception>
    public static IDocumentSource CreateFromFolder(
        string sourceId,
        string folderId,
        string authToken,
        int pollingIntervalMs = 5000)
    {
        if (string.IsNullOrWhiteSpace(sourceId))
            throw new ArgumentException("Source ID cannot be empty", nameof(sourceId));

        if (string.IsNullOrWhiteSpace(folderId))
            throw new ArgumentException("Folder ID cannot be empty", nameof(folderId));

        if (string.IsNullOrWhiteSpace(authToken))
            throw new ArgumentException("Auth token cannot be empty", nameof(authToken));

        if (pollingIntervalMs < 100)
            throw new ArgumentException("Polling interval must be at least 100ms", nameof(pollingIntervalMs));

        var config = new DocumentSourceConfig
        {
            AdapterType = DocumentSourceAdapterType.GoogleDrive,
            SourceId = sourceId,
            AdapterSpecificConfig = new()
            {
                { "FolderId", folderId },
                { "AuthToken", authToken },
                { "PollingIntervalMs", pollingIntervalMs }
            }
        };

        return new GoogleDriveDocumentSource(config);
    }

    /// <summary>
    /// Creates a GoogleDriveDocumentSource with custom configuration.
    /// </summary>
    /// <param name="config">Full configuration object</param>
    /// <returns>Configured IDocumentSource instance</returns>
    /// <exception cref="DocumentSourceConfigurationException">
    /// Thrown if configuration is invalid
    /// </exception>
    public static IDocumentSource CreateFromConfig(DocumentSourceConfig config)
    {
        return new GoogleDriveDocumentSource(config);
    }

    /// <summary>
    /// Creates a GoogleDriveDocumentSource with an injected DriveService (useful for testing).
    /// </summary>
    /// <param name="config">Full configuration object</param>
    /// <param name="driveService">Mock or real DriveService instance</param>
    /// <returns>Configured IDocumentSource instance</returns>
    public static IDocumentSource CreateFromConfig(DocumentSourceConfig config, DriveService driveService)
    {
        return new GoogleDriveDocumentSource(config, driveService);
    }
}
