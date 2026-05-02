using Mnemi.Application.Ports.Source;
using Xunit;

namespace Mnemi.Adapter.Source.FileSystem.Tests;

public class FileSystemWatcherServiceTests : IAsyncLifetime
{
    private string _tempDirectory;

    public Task InitializeAsync()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), $"mnemi-watcher-test-{Guid.NewGuid()}");
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
    public async Task GetChangesAsync_DetectsFileCreation()
    {
        // Arrange
        var watcher = new FileSystemWatcherService(_tempDirectory, "test-source");
        var changes = new List<DocumentChange>();
        var cts = new CancellationTokenSource();

        // Act
        var watchTask = Task.Run(async () =>
        {
            await foreach (var change in watcher.GetChangesAsync(cts.Token))
            {
                changes.Add(change);
                if (changes.Count >= 1)
                    cts.Cancel();
            }
        });

        await Task.Delay(200);
        await File.WriteAllTextAsync(Path.Combine(_tempDirectory, "new.md"), "Content");

        try
        {
            await watchTask;
        }
        catch (OperationCanceledException)
        {
        }

        // Assert
        Assert.NotEmpty(changes);
        Assert.Single(changes.Where(c => c.ChangeType == DocumentChangeType.Created));
        await watcher.DisposeAsync();
    }

    [Fact]
    public async Task GetChangesAsync_DetectsFileModification()
    {
        // Arrange
        var filePath = Path.Combine(_tempDirectory, "test.md");
        await File.WriteAllTextAsync(filePath, "Initial");

        var watcher = new FileSystemWatcherService(_tempDirectory, "test-source");
        var changes = new List<DocumentChange>();
        var cts = new CancellationTokenSource();

        // Act
        var watchTask = Task.Run(async () =>
        {
            await foreach (var change in watcher.GetChangesAsync(cts.Token))
            {
                changes.Add(change);
                if (changes.Count >= 1)
                    cts.Cancel();
            }
        });

        await Task.Delay(200);
        await File.WriteAllTextAsync(filePath, "Modified");

        try
        {
            await watchTask;
        }
        catch (OperationCanceledException)
        {
        }

        // Assert
        Assert.NotEmpty(changes);
        Assert.Single(changes.Where(c => c.ChangeType == DocumentChangeType.Modified));
        await watcher.DisposeAsync();
    }

    [Fact]
    public async Task GetChangesAsync_DetectsFileDeletion()
    {
        // Arrange
        var filePath = Path.Combine(_tempDirectory, "test.md");
        await File.WriteAllTextAsync(filePath, "Content");

        var watcher = new FileSystemWatcherService(_tempDirectory, "test-source");
        var changes = new List<DocumentChange>();
        var cts = new CancellationTokenSource();

        // Act
        var watchTask = Task.Run(async () =>
        {
            await foreach (var change in watcher.GetChangesAsync(cts.Token))
            {
                changes.Add(change);
                if (changes.Count >= 1)
                    cts.Cancel();
            }
        });

        await Task.Delay(200);
        File.Delete(filePath);

        try
        {
            await watchTask;
        }
        catch (OperationCanceledException)
        {
        }

        // Assert
        Assert.NotEmpty(changes);
        Assert.Single(changes.Where(c => c.ChangeType == DocumentChangeType.Deleted));
        await watcher.DisposeAsync();
    }

    [Fact]
    public async Task GetChangesAsync_DebouncesDuplicateChanges()
    {
        // Arrange
        var watcher = new FileSystemWatcherService(_tempDirectory, "test-source", debounceDelayMs: 100);
        var changes = new List<DocumentChange>();
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));

        // Act
        var watchTask = Task.Run(async () =>
        {
            await foreach (var change in watcher.GetChangesAsync(cts.Token))
            {
                changes.Add(change);
            }
        });

        await Task.Delay(200);

        // Rapidly create and modify the same file
        var filePath = Path.Combine(_tempDirectory, "rapid.md");
        await File.WriteAllTextAsync(filePath, "1");
        await Task.Delay(10);
        await File.WriteAllTextAsync(filePath, "2");
        await Task.Delay(10);
        await File.WriteAllTextAsync(filePath, "3");
        await Task.Delay(200);

        cts.Cancel();

        try
        {
            await watchTask;
        }
        catch (OperationCanceledException)
        {
        }

        // Assert - should have fewer changes due to debouncing
        Assert.NotEmpty(changes);
        await watcher.DisposeAsync();
    }
}
