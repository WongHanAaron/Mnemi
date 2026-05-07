using Mnemi.Application.Ports.Source;
using Mnemi.Application.Ports.Source.Exceptions;

namespace Mnemi.Adapter.Source.GoogleDrive;

/// <summary>
/// Validates configuration parameters specific to the GoogleDrive adapter.
/// </summary>
internal class GoogleDriveConfigValidator
{
    /// <summary>
    /// Validates that GoogleDrive adapter configuration contains all required parameters.
    /// </summary>
    /// <param name="config">The DocumentSourceConfig to validate.</param>
    /// <exception cref="DocumentSourceConfigurationException">Thrown if required parameters are missing.</exception>
    public static void Validate(DocumentSourceConfig config)
    {
        if (config.AdapterType != DocumentSourceAdapterType.GoogleDrive)
            throw new DocumentSourceConfigurationException(
                "Attempted to validate non-GoogleDrive adapter configuration with GoogleDriveConfigValidator.",
                config.SourceId,
                config.AdapterType);

        const string folderIdKey = "FolderId";
        if (!config.AdapterSpecificConfig.TryGetValue(folderIdKey, out var folderIdObj) ||
            folderIdObj is not string folderId ||
            string.IsNullOrWhiteSpace(folderId))
        {
            throw new DocumentSourceConfigurationException(
                $"GoogleDrive adapter requires '{folderIdKey}' parameter in AdapterSpecificConfig.",
                config.SourceId,
                config.AdapterType);
        }

        const string authTokenKey = "AuthToken";
        if (!config.AdapterSpecificConfig.TryGetValue(authTokenKey, out var authTokenObj) ||
            authTokenObj is not string authToken ||
            string.IsNullOrWhiteSpace(authToken))
        {
            throw new DocumentSourceConfigurationException(
                $"GoogleDrive adapter requires '{authTokenKey}' parameter in AdapterSpecificConfig.",
                config.SourceId,
                config.AdapterType);
        }

        // PollingIntervalMs is optional, default to 5000
        if (config.AdapterSpecificConfig.TryGetValue("PollingIntervalMs", out var pollingIntervalObj))
        {
            if (pollingIntervalObj is not int pollingInterval || pollingInterval <= 0)
            {
                throw new DocumentSourceConfigurationException(
                    "If specified, 'PollingIntervalMs' must be a positive integer.",
                    config.SourceId,
                    config.AdapterType);
            }
        }
    }

    /// <summary>
    /// Gets the folder ID from configuration with validation.
    /// </summary>
    public static string GetFolderId(DocumentSourceConfig config)
    {
        return (config.AdapterSpecificConfig["FolderId"] as string)!;
    }

    /// <summary>
    /// Gets the auth token from configuration with validation.
    /// </summary>
    public static string GetAuthToken(DocumentSourceConfig config)
    {
        return (config.AdapterSpecificConfig["AuthToken"] as string)!;
    }

    /// <summary>
    /// Gets the polling interval from configuration, defaulting to 5000ms if not specified.
    /// </summary>
    public static int GetPollingIntervalMs(DocumentSourceConfig config)
    {
        return config.AdapterSpecificConfig.TryGetValue("PollingIntervalMs", out var interval)
            ? (interval as int?) ?? 5000
            : 5000;
    }
}
