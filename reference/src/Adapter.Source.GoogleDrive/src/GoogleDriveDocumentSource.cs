using System;
using Google.Apis.Drive.v3;
using Mnemi.Application.Ports.Source;
using Mnemi.Application.Ports.Source.Exceptions;

namespace Mnemi.Adapter.Source.GoogleDrive;

/// <summary>
/// Google Drive based implementation of IDocumentSource that reads documents from a cloud folder.
/// </summary>
public class GoogleDriveDocumentSource : IDocumentSource
{
    private readonly DocumentSourceConfig _config;
    private readonly IGoogleDriveClient _client;
    private readonly string _folderId;

    /// <summary>
    /// Creates a new instance of GoogleDriveDocumentSource.
    /// </summary>
    /// <param name="config">The configuration for this source.</param>
    /// <exception cref="DocumentSourceConfigurationException">Thrown if configuration is invalid.</exception>
    public GoogleDriveDocumentSource(DocumentSourceConfig config)
    {
        GoogleDriveConfigValidator.Validate(config);

        _config = config;
        _folderId = GoogleDriveConfigValidator.GetFolderId(config);
        _client = new GoogleDriveClient(new DriveService(), config.SourceId);
    }

    /// <summary>
    /// Creates a new instance of GoogleDriveDocumentSource.
    /// </summary>
    /// <param name="config">The configuration for this source.</param>
    /// <param name="driveService">The configured Google Drive service.</param>
    /// <exception cref="DocumentSourceConfigurationException">Thrown if configuration is invalid.</exception>
    public GoogleDriveDocumentSource(DocumentSourceConfig config, DriveService driveService)
    {
        GoogleDriveConfigValidator.Validate(config);

        _config = config;
        _folderId = GoogleDriveConfigValidator.GetFolderId(config);
        _client = new GoogleDriveClient(driveService, config.SourceId);
    }

    /// &lt;summary&gt;
    /// Creates a new instance of GoogleDriveDocumentSource for testing.
    /// &lt;/summary&gt;
    /// &lt;param name="config"&gt;The configuration for this source.&lt;/param&gt;
    /// &lt;param name="client"&gt;The Google Drive client abstraction.&lt;/param&gt;
    /// &lt;exception cref="DocumentSourceConfigurationException"&gt;Thrown if configuration is invalid.&lt;/exception&gt;
    public GoogleDriveDocumentSource(DocumentSourceConfig config, IGoogleDriveClient client)
    {
        GoogleDriveConfigValidator.Validate(config);

        _config = config;
        _folderId = GoogleDriveConfigValidator.GetFolderId(config);
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    /// <summary>
    /// Reads all documents from the configured Google Drive folder.
    /// </summary>
    public IAsyncEnumerable<DocumentContent> ReadAllAsync(CancellationToken cancellationToken)
    {
        return ReadAllCoreAsync(cancellationToken);
    }

    private async IAsyncEnumerable<DocumentContent> ReadAllCoreAsync(CancellationToken cancellationToken)
    {
        await foreach (var file in _client.ListFilesInFolderAsync(_folderId, cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            DocumentContent documentContent;
            try
            {
                var content = await _client.DownloadFileAsStringAsync(file.Id, cancellationToken);
                documentContent = new DocumentContent(
                    Id: file.Id,
                    Content: content,
                    LastModified: file.ModifiedTime?.ToUniversalTime() ?? DateTime.UtcNow,
                    Encoding: "utf-8"
                );
            }
            catch (DocumentSourceException)
            {
                throw;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DocumentSourceException(
                    $"Error downloading file '{file.Name}' ({file.Id}) from Google Drive: {ex.Message}",
                    _config.SourceId,
                    _config.AdapterType,
                    ex);
            }

            yield return documentContent;
        }
    }

    /// <summary>
    /// Subscribes to changes in the Google Drive folder.
    /// </summary>
    public IAsyncEnumerable<DocumentChange> SubscribeToChangesAsync(CancellationToken cancellationToken)
    {
        return SubscribeToChangesCoreAsync(cancellationToken);
    }

    private async IAsyncEnumerable<DocumentChange> SubscribeToChangesCoreAsync(CancellationToken cancellationToken)
    {
        var pollingIntervalMs = GoogleDriveConfigValidator.GetPollingIntervalMs(_config);
        var watcher = new GoogleDriveWatcherService(_client, _folderId, _config.SourceId, pollingIntervalMs);

        await foreach (var change in watcher.GetChangesAsync(cancellationToken))
        {
            yield return change;
        }
    }
}
