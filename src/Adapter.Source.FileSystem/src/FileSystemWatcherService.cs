using System.Threading.Channels;
using Mnemi.Application.Ports.Exceptions;
using Mnemi.Application.Ports.Source;

namespace Mnemi.Adapter.Source.FileSystem;

/// <summary>
/// Monitors a directory for file changes using FileSystemWatcher with debouncing.
/// </summary>
public class FileSystemWatcherService : IAsyncDisposable
{
    private readonly FileSystemWatcher _watcher;
    private readonly string _directoryPath;
    private readonly string _sourceId;
    private readonly Channel<DocumentChange> _changeChannel;
    private readonly CancellationTokenSource _disposalCts;
    private readonly Dictionary<string, DateTime> _recentChanges;
    private readonly int _debounceDelayMs;
    private readonly object _lockObj = new();

    public FileSystemWatcherService(string directoryPath, string sourceId, int debounceDelayMs = 150)
    {
        _directoryPath = directoryPath;
        _sourceId = sourceId;
        _debounceDelayMs = debounceDelayMs;
        _recentChanges = new Dictionary<string, DateTime>();
        _changeChannel = Channel.CreateUnbounded<DocumentChange>();
        _disposalCts = new CancellationTokenSource();

        _watcher = new FileSystemWatcher(directoryPath)
        {
            Filter = "*.md",
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Size,
            IncludeSubdirectories = true
        };

        _watcher.Created += OnFileChanged;
        _watcher.Changed += OnFileChanged;
        _watcher.Deleted += OnFileChanged;
        _watcher.Error += OnWatcherError;
    }

    /// <summary>
    /// Yields change events from the watched directory.
    /// </summary>
    public async IAsyncEnumerable<DocumentChange> GetChangesAsync(CancellationToken cancellationToken)
    {
        _watcher.EnableRaisingEvents = true;

        try
        {
            await foreach (var change in _changeChannel.Reader.ReadAllAsync(cancellationToken))
            {
                yield return change;
            }
        }
        finally
        {
            _watcher.EnableRaisingEvents = false;
        }
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        try
        {
            // Debounce rapid changes
            lock (_lockObj)
            {
                var now = DateTime.UtcNow;
                if (_recentChanges.TryGetValue(e.FullPath, out var lastChange))
                {
                    if ((now - lastChange).TotalMilliseconds < _debounceDelayMs)
                        return;
                }
                _recentChanges[e.FullPath] = now;
            }

            DocumentChangeType? changeType = e.ChangeType switch
            {
                WatcherChangeTypes.Created => DocumentChangeType.Created,
                WatcherChangeTypes.Changed => DocumentChangeType.Modified,
                WatcherChangeTypes.Deleted => DocumentChangeType.Deleted,
                _ => null
            };

            if (changeType is null)
                return;

            // For deleted files, create DocumentContent with empty body
            DocumentContent document;
            if (changeType == DocumentChangeType.Deleted)
            {
                document = new DocumentContent(
                    Id: Path.GetRelativePath(_directoryPath, e.FullPath),
                    Content: "",
                    LastModified: DateTime.UtcNow,
                    Encoding: "utf-8"
                );
            }
            else
            {
                // For created/modified, try to read the file
                try
                {
                    var reader = new FileContentReader();
                    document = reader.ReadFileAsync(e.FullPath, CancellationToken.None).Result;
                }
                catch
                {
                    // If we can't read the file yet, report with empty content
                    document = new DocumentContent(
                        Id: Path.GetRelativePath(_directoryPath, e.FullPath),
                        Content: "",
                        LastModified: DateTime.UtcNow,
                        Encoding: "utf-8"
                    );
                }
            }

            var change = new DocumentChange(
                ChangeType: changeType!.Value,
                Document: document,
                DetectedAt: DateTime.UtcNow
            );

            _ = _changeChannel.Writer.TryWrite(change);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error processing file change: {ex.Message}");
        }
    }

    private void OnWatcherError(object sender, ErrorEventArgs e)
    {
        var exception = e.GetException();
        System.Diagnostics.Debug.WriteLine($"FileSystemWatcher error: {exception?.Message}");
    }

    public async ValueTask DisposeAsync()
    {
        _watcher?.Dispose();
        _changeChannel.Writer.TryComplete();
        _disposalCts?.Dispose();
        await Task.CompletedTask;
    }
}
