using System;
using Mnemi.Application.Ports.Source;
using Mnemi.Application.Ports.Source.Exceptions;

namespace Mnemi.Adapter.Source.FileSystem;

/// <summary>
/// File system based implementation of IDocumentSource that reads documents from a local directory.
/// </summary>
public class FileSystemDocumentSource : IDocumentSource
{
    private readonly DocumentSourceConfig _config;
    private readonly string _directoryPath;
    private readonly string _searchPattern;
    private readonly FileContentReader _contentReader;

    /// <summary>
    /// Creates a new instance of FileSystemDocumentSource.
    /// </summary>
    /// <param name="config">The configuration for this source.</param>
    /// <exception cref="DocumentSourceConfigurationException">Thrown if configuration is invalid.</exception>
    public FileSystemDocumentSource(DocumentSourceConfig config)
    {
        FileSystemConfigValidator.Validate(config);

        _config = config;
        _directoryPath = FileSystemConfigValidator.GetDirectoryPath(config);
        _searchPattern = FileSystemConfigValidator.GetSearchPattern(config);
        _contentReader = new FileContentReader();
    }

    /// <summary>
    /// Reads all documents from the configured directory.
    /// </summary>
    public IAsyncEnumerable<DocumentContent> ReadAllAsync(CancellationToken cancellationToken)
    {
        return ReadAllCoreAsync(cancellationToken);
    }

    private async IAsyncEnumerable<DocumentContent> ReadAllCoreAsync(CancellationToken cancellationToken)
    {
        IEnumerable<string> files;
        try
        {
            files = Directory.EnumerateFiles(_directoryPath, _searchPattern, SearchOption.AllDirectories);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new DocumentSourceAccessException(
                $"Access denied when enumerating directory: '{_directoryPath}'.",
                _config.SourceId,
                _config.AdapterType,
                ex);
        }
        catch (DirectoryNotFoundException ex)
        {
            throw new DocumentSourceAccessException(
                $"Directory not found: '{_directoryPath}'.",
                _config.SourceId,
                _config.AdapterType,
                ex);
        }
        catch (Exception ex)
        {
            throw new DocumentSourceException(
                $"Error enumerating files in '{_directoryPath}': {ex.Message}",
                _config.SourceId,
                _config.AdapterType,
                ex);
        }

        foreach (var filePath in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            DocumentContent documentContent;
            try
            {
                documentContent = await _contentReader.ReadFileAsync(filePath, cancellationToken);
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
                    $"Error reading file '{filePath}' during enumeration: {ex.Message}",
                    _config.SourceId,
                    _config.AdapterType,
                    ex);
            }

            yield return documentContent;
        }
    }

    /// <summary>
    /// Subscribes to changes in the directory.
    /// </summary>
    public IAsyncEnumerable<DocumentChange> SubscribeToChangesAsync(CancellationToken cancellationToken)
    {
        return SubscribeToChangesCoreAsync(cancellationToken);
    }

    private async IAsyncEnumerable<DocumentChange> SubscribeToChangesCoreAsync(CancellationToken cancellationToken)
    {
        FileSystemWatcherService? watcher = null;
        try
        {
            watcher = new FileSystemWatcherService(_directoryPath, _config.SourceId);
        }
        catch (Exception ex)
        {
            throw new DocumentSourceException(
                $"Error monitoring directory '{_directoryPath}' for changes: {ex.Message}",
                _config.SourceId,
                _config.AdapterType,
                ex);
        }

        try
        {
            await foreach (var change in watcher.GetChangesAsync(cancellationToken))
            {
                yield return change;
            }
        }
        finally
        {
            if (watcher != null)
                await watcher.DisposeAsync();
        }
    }
}
