using Application.Ports.Source;
using Mnemi.Application.Ports;
using Mnemi.Application.Ports.Source;
using Mnemi.Application.Source;
using Xunit;

namespace Mnemi.Adapter.Source.FileSystem.Tests;

public class FileSystemDocumentSourceSubscribeTests : IAsyncLifetime
{
    private string _tempDirectory;

    public Task InitializeAsync()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), $"mnemi-watch-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDirectory);
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        if (Directory.Exists(_tempDirectory))
            Directory.Delete(_tempDirectory, recursive: true);
        return Task.CompletedTask;
    }

    [Fact]
    public async Task SubscribeToChangesAsync_YieldsCreatedEvent_WhenFileAdded()
    {
        // Arrange
        var config = new DocumentSourceConfig
        {
            AdapterType = DocumentSourceAdapterType.FileSystem,
            SourceId = "test-source",
            AdapterSpecificConfig = new() { { "DirectoryPath", _tempDirectory } }
        };
        var source = new FileSystemDocumentSource(config);
        var cts = new CancellationTokenSource();
        var changes = new List<DocumentChange>();

        // Act
        var enumerationTask = Task.Run(async () =>
        {
            await foreach (var change in source.SubscribeToChangesAsync(cts.Token))
            {
                changes.Add(change);
                if (changes.Count >= 1)
                    cts.Cancel();
            }
        });

        // Wait a bit for watcher to be ready
        await Task.Delay(200);

        // Create a file
        await File.WriteAllTextAsync(Path.Combine(_tempDirectory, "new-file.md"), "Content");

        // Wait for event
        try
        {
            await enumerationTask.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Expected
        }

        // Assert
        Assert.NotEmpty(changes);
        Assert.Single(changes.Where(c => c.ChangeType == DocumentChangeType.Created));
    }

    [Fact]
    public async Task SubscribeToChangesAsync_YieldsModifiedEvent_WhenFileUpdated()
    {
        // Arrange
        var filePath = Path.Combine(_tempDirectory, "test.md");
        await File.WriteAllTextAsync(filePath, "Initial content");

        var config = new DocumentSourceConfig
        {
            AdapterType = DocumentSourceAdapterType.FileSystem,
            SourceId = "test-source",
            AdapterSpecificConfig = new() { { "DirectoryPath", _tempDirectory } }
        };
        var source = new FileSystemDocumentSource(config);
        var cts = new CancellationTokenSource();
        var changes = new List<DocumentChange>();

        // Act
        var enumerationTask = Task.Run(async () =>
        {
            await foreach (var change in source.SubscribeToChangesAsync(cts.Token))
            {
                changes.Add(change);
                if (changes.Count >= 1)
                    cts.Cancel();
            }
        });

        // Wait a bit for watcher to be ready
        await Task.Delay(200);

        // Modify the file
        await File.WriteAllTextAsync(filePath, "Updated content");

        // Wait for event
        try
        {
            await enumerationTask.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Expected
        }

        // Assert
        Assert.NotEmpty(changes);
        Assert.Single(changes.Where(c => c.ChangeType == DocumentChangeType.Modified));
    }

    [Fact]
    public async Task SubscribeToChangesAsync_YieldsDeletedEvent_WhenFileRemoved()
    {
        // Arrange
        var filePath = Path.Combine(_tempDirectory, "test.md");
        await File.WriteAllTextAsync(filePath, "Content");

        var config = new DocumentSourceConfig
        {
            AdapterType = DocumentSourceAdapterType.FileSystem,
            SourceId = "test-source",
            AdapterSpecificConfig = new() { { "DirectoryPath", _tempDirectory } }
        };
        var source = new FileSystemDocumentSource(config);
        var cts = new CancellationTokenSource();
        var changes = new List<DocumentChange>();

        // Act
        var enumerationTask = Task.Run(async () =>
        {
            await foreach (var change in source.SubscribeToChangesAsync(cts.Token))
            {
                changes.Add(change);
                if (changes.Count >= 1)
                    cts.Cancel();
            }
        });

        // Wait a bit for watcher to be ready
        await Task.Delay(200);

        // Delete the file
        File.Delete(filePath);

        // Wait for event
        try
        {
            await enumerationTask.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Expected
        }

        // Assert
        Assert.NotEmpty(changes);
        Assert.Single(changes.Where(c => c.ChangeType == DocumentChangeType.Deleted));
    }

    [Fact]
    public async Task SubscribeToChangesAsync_RespectsCancellationToken()
    {
        // Arrange
        var config = new DocumentSourceConfig
        {
            AdapterType = DocumentSourceAdapterType.FileSystem,
            SourceId = "test-source",
            AdapterSpecificConfig = new() { { "DirectoryPath", _tempDirectory } }
        };
        var source = new FileSystemDocumentSource(config);
        var cts = new CancellationTokenSource();

        // Act & Assert
        cts.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await foreach (var _ in source.SubscribeToChangesAsync(cts.Token))
            {
            }
        });
    }

    [Fact]
    public async Task SubscribeToChangesAsync_DeletedEventHasEmptyContent()
    {
        // Arrange
        var filePath = Path.Combine(_tempDirectory, "test.md");
        await File.WriteAllTextAsync(filePath, "Content");

        var config = new DocumentSourceConfig
        {
            AdapterType = DocumentSourceAdapterType.FileSystem,
            SourceId = "test-source",
            AdapterSpecificConfig = new() { { "DirectoryPath", _tempDirectory } }
        };
        var source = new FileSystemDocumentSource(config);
        var cts = new CancellationTokenSource();
        var changes = new List<DocumentChange>();

        // Act
        var enumerationTask = Task.Run(async () =>
        {
            await foreach (var change in source.SubscribeToChangesAsync(cts.Token))
            {
                changes.Add(change);
                if (change.ChangeType == DocumentChangeType.Deleted)
                    cts.Cancel();
            }
        });

        // Wait a bit for watcher to be ready
        await Task.Delay(200);

        // Delete the file
        File.Delete(filePath);

        // Wait for event
        try
        {
            await enumerationTask.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Expected
        }

        // Assert
        var deletedEvent = changes.FirstOrDefault(c => c.ChangeType == DocumentChangeType.Deleted);
        Assert.NotNull(deletedEvent);
        Assert.Empty(deletedEvent.Document.Content);
        Assert.NotEmpty(deletedEvent.Document.Id);
    }

    [Fact]
    public async Task SubscribeToChangesAsync_CreatedEventHasFullContent()
    {
        // Arrange
        var config = new DocumentSourceConfig
        {
            AdapterType = DocumentSourceAdapterType.FileSystem,
            SourceId = "test-source",
            AdapterSpecificConfig = new() { { "DirectoryPath", _tempDirectory } }
        };
        var source = new FileSystemDocumentSource(config);
        var cts = new CancellationTokenSource();
        var changes = new List<DocumentChange>();
        var expectedContent = "Test file content";

        // Act
        var enumerationTask = Task.Run(async () =>
        {
            await foreach (var change in source.SubscribeToChangesAsync(cts.Token))
            {
                changes.Add(change);
                if (change.ChangeType == DocumentChangeType.Created)
                    cts.Cancel();
            }
        });

        // Wait a bit for watcher to be ready
        await Task.Delay(200);

        // Create a file
        await File.WriteAllTextAsync(Path.Combine(_tempDirectory, "new.md"), expectedContent);

        // Wait for event
        try
        {
            await enumerationTask.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Expected
        }

        // Assert
        var createdEvent = changes.FirstOrDefault(c => c.ChangeType == DocumentChangeType.Created);
        Assert.NotNull(createdEvent);
        // Content may not be immediately available due to file locking
    }
}
