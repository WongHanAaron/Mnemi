using Mnemi.Application.Ports;
using Mnemi.Application.Ports.Source;
using Mnemi.Application.Ports.Source.Exceptions;

namespace Mnemi.Adapter.Source.GoogleDrive;

/// <summary>
/// Monitors a Google Drive folder for changes using polling.
/// </summary>
public class GoogleDriveWatcherService
{
    private readonly IGoogleDriveClient _client;
    private readonly string _folderId;
    private readonly string _sourceId;
    private readonly int _pollingIntervalMs;
    private Dictionary<string, DateTimeOffset>? _previousSnapshot;

    public GoogleDriveWatcherService(
        IGoogleDriveClient client,
        string folderId,
        string sourceId,
        int pollingIntervalMs = 5000)
    {
        _client = client;
        _folderId = folderId;
        _sourceId = sourceId;
        _pollingIntervalMs = pollingIntervalMs;
    }

    /// <summary>
    /// Polls the folder and yields detected changes.
    /// </summary>
    public IAsyncEnumerable<DocumentChange> GetChangesAsync(CancellationToken cancellationToken)
    {
        return GetChangesCoreAsync(cancellationToken);
    }

    private async IAsyncEnumerable<DocumentChange> GetChangesCoreAsync(CancellationToken cancellationToken)
    {
        _previousSnapshot = null;

        while (!cancellationToken.IsCancellationRequested)
        {
            // Get current snapshot
            var currentSnapshot = new Dictionary<string, DateTimeOffset>();
            await foreach (var file in _client.ListFilesInFolderAsync(_folderId, cancellationToken))
            {
                currentSnapshot[file.Id] = file.ModifiedTimeDateTimeOffset ?? DateTimeOffset.MaxValue;
            }

            // Compare with previous snapshot
            if (_previousSnapshot != null)
            {
                // Detect created files
                foreach (var fileId in currentSnapshot.Keys.Where(id => !_previousSnapshot.ContainsKey(id)))
                {
                    string content;
                    try
                    {
                        content = await _client.DownloadFileAsStringAsync(fileId, cancellationToken);
                    }
                    catch
                    {
                        content = "";
                    }

                    yield return new DocumentChange(
                        ChangeType: DocumentChangeType.Created,
                        Document: new DocumentContent(
                            Id: fileId,
                            Content: content,
                            LastModified: currentSnapshot[fileId],
                            Encoding: "utf-8"
                        ),
                        DetectedAt: DateTime.UtcNow
                    );
                }

                // Detect modified files
                foreach (var fileId in currentSnapshot.Keys.Where(id => _previousSnapshot.ContainsKey(id)))
                {
                    if (currentSnapshot[fileId] != _previousSnapshot[fileId])
                    {
                        string content;
                        try
                        {
                            content = await _client.DownloadFileAsStringAsync(fileId, cancellationToken);
                        }
                        catch
                        {
                            content = "";
                        }

                        yield return new DocumentChange(
                            ChangeType: DocumentChangeType.Modified,
                            Document: new DocumentContent(
                                Id: fileId,
                                Content: content,
                                LastModified: currentSnapshot[fileId],
                                Encoding: "utf-8"
                            ),
                            DetectedAt: DateTime.UtcNow
                        );
                    }
                }

                // Detect deleted files
                foreach (var fileId in _previousSnapshot.Keys.Where(id => !currentSnapshot.ContainsKey(id)))
                {
                    yield return new DocumentChange(
                        ChangeType: DocumentChangeType.Deleted,
                        Document: new DocumentContent(
                            Id: fileId,
                            Content: "",
                            LastModified: DateTime.UtcNow,
                            Encoding: "utf-8"
                        ),
                        DetectedAt: DateTime.UtcNow
                    );
                }
            }

            _previousSnapshot = currentSnapshot;

            // Wait before next poll
            try
            {
                await Task.Delay(_pollingIntervalMs, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                yield break;
            }
        }
    }
}
