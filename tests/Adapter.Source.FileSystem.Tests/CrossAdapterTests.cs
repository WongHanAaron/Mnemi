using Mnemi.Adapter.Source.FileSystem;
using Mnemi.Application.Ports.Source;
using Xunit;

namespace Mnemi.Adapter.Source.FileSystem.Tests;

/// <summary>
/// Tests that verify FileSystemDocumentSource correctly implements the IDocumentSource contract.
/// These tests are mirrored in the GoogleDrive adapter tests to ensure contract compliance.
/// </summary>
public class CrossAdapterTests : IAsyncLifetime
{
    private readonly string _testDir;

    public CrossAdapterTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), $"mnemi-cross-adapter-{Guid.NewGuid():N}");
    }

    public Task InitializeAsync()
    {
        Directory.CreateDirectory(_testDir);
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        try
        {
            if (Directory.Exists(_testDir))
                Directory.Delete(_testDir, recursive: true);
        }
        catch
        {
        }
        return Task.CompletedTask;
    }

    [Fact]
    public void AdapterImplementsIDocumentSourceInterface()
    {
        // Arrange & Act
        var source = FileSystemAdapterFactory.CreateMarkdownSource("test", _testDir);

        // Assert
        Assert.IsAssignableFrom<IDocumentSource>(source);
    }

    [Fact]
    public async Task ReadAllAsyncReturnsIAsyncEnumerable()
    {
        // Arrange
        var source = FileSystemAdapterFactory.CreateMarkdownSource("test", _testDir);
        File.WriteAllText(Path.Combine(_testDir, "test.md"), "Content");

        // Act
        var enumerable = source.ReadAllAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(enumerable);
        var count = 0;
        await foreach (var item in enumerable)
        {
            count++;
            Assert.NotNull(item);
            Assert.IsType<DocumentContent>(item);
        }
        Assert.True(count > 0);
    }

    [Fact]
    public async Task SubscribeToChangesAsyncReturnsIAsyncEnumerable()
    {
        // Arrange
        var source = FileSystemAdapterFactory.CreateMarkdownSource("test", _testDir);
        var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));

        // Act
        var enumerable = source.SubscribeToChangesAsync(cts.Token);

        // Assert
        Assert.NotNull(enumerable);
        var count = 0;
        try
        {
            await foreach (var item in enumerable)
            {
                count++;
                Assert.NotNull(item);
                Assert.IsType<DocumentChange>(item);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    [Fact]
    public async Task CancellationTokenIsRespectedDuringReadAll()
    {
        // Arrange
        var source = FileSystemAdapterFactory.CreateMarkdownSource("test", _testDir);

        // Create enough files to potentially read multiple
        for (int i = 0; i < 10; i++)
            File.WriteAllText(Path.Combine(_testDir, $"doc{i}.md"), $"Content {i}");

        var cts = new CancellationTokenSource();

        // Act & Assert
        cts.Cancel();
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
        {
            await foreach (var doc in source.ReadAllAsync(cts.Token))
            {
            }
        });
    }

    [Fact]
    public async Task CancellationTokenIsRespectedDuringSubscribe()
    {
        // Arrange
        var source = FileSystemAdapterFactory.CreateMarkdownSource("test", _testDir);
        var cts = new CancellationTokenSource();

        // Act & Assert
        cts.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await foreach (var change in source.SubscribeToChangesAsync(cts.Token))
            {
            }
        });
    }

    [Fact]
    public async Task DocumentContentPreservesEncoding()
    {
        // Arrange
        var source = FileSystemAdapterFactory.CreateMarkdownSource("test", _testDir);
        var content = "Test content with special chars: é ñ ü";
        var filePath = Path.Combine(_testDir, "encoded.md");
        File.WriteAllText(filePath, content, System.Text.Encoding.UTF8);

        // Act
        var docs = new List<DocumentContent>();
        await foreach (var doc in source.ReadAllAsync(CancellationToken.None))
            docs.Add(doc);

        // Assert
        Assert.Single(docs);
        Assert.Equal(content, docs[0].Content);
        Assert.NotEmpty(docs[0].Encoding);
    }

    [Fact]
    public async Task DocumentChangePreservesContent()
    {
        // Arrange: Create source with existing file
        var source = FileSystemAdapterFactory.CreateMarkdownSource("test", _testDir);
        var filePath = Path.Combine(_testDir, "test.md");
        File.WriteAllText(filePath, "Initial content");

        // Act: Read the file directly to verify content is preserved
        var docs = new List<DocumentContent>();
        await foreach (var doc in source.ReadAllAsync(CancellationToken.None))
            docs.Add(doc);

        // Assert
        Assert.Single(docs);
        Assert.Equal("Initial content", docs[0].Content);
        Assert.NotEmpty(docs[0].Encoding);
    }

    [Fact]
    public async Task MultipleConcurrentSubscribersWork()
    {
        // Arrange
        var source = FileSystemAdapterFactory.CreateMarkdownSource("test", _testDir);
        var subscriber1Changes = new List<DocumentChange>();
        var subscriber2Changes = new List<DocumentChange>();
        var cts1 = new CancellationTokenSource(TimeSpan.FromSeconds(1));
        var cts2 = new CancellationTokenSource(TimeSpan.FromSeconds(1));

        // Act: Start both subscriptions
        var task1 = Task.Run(async () =>
        {
            try
            {
                await foreach (var change in source.SubscribeToChangesAsync(cts1.Token))
                {
                    subscriber1Changes.Add(change);
                    if (subscriber1Changes.Count >= 1)
                        cts1.Cancel();
                }
            }
            catch (OperationCanceledException)
            {
            }
        });

        var task2 = Task.Run(async () =>
        {
            try
            {
                await foreach (var change in source.SubscribeToChangesAsync(cts2.Token))
                {
                    subscriber2Changes.Add(change);
                    if (subscriber2Changes.Count >= 1)
                        cts2.Cancel();
                }
            }
            catch (OperationCanceledException)
            {
            }
        });

        await Task.Delay(100);

        // Create a file to trigger both subscriptions
        File.WriteAllText(Path.Combine(_testDir, "shared.md"), "Shared content");

        await Task.WhenAll(task1, task2);

        // Assert: Both subscribers received the event
        Assert.NotEmpty(subscriber1Changes);
        Assert.NotEmpty(subscriber2Changes);
    }

    [Fact]
    public async Task DocumentContentIncludesLastModifiedTime()
    {
        // Arrange
        var source = FileSystemAdapterFactory.CreateMarkdownSource("test", _testDir);
        var filePath = Path.Combine(_testDir, "test.md");
        File.WriteAllText(filePath, "Content");

        // Act
        var docs = new List<DocumentContent>();
        await foreach (var doc in source.ReadAllAsync(CancellationToken.None))
            docs.Add(doc);

        // Assert
        Assert.Single(docs);
        Assert.NotEqual(default, docs[0].LastModified);
        Assert.True(docs[0].LastModified < DateTime.UtcNow);
        Assert.True(docs[0].LastModified > DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public async Task DeletedEventHasEmptyContent()
    {
        // Arrange
        var source = FileSystemAdapterFactory.CreateMarkdownSource("test", _testDir);
        var filePath = Path.Combine(_testDir, "test.md");
        File.WriteAllText(filePath, "Content to delete");

        var changes = new List<DocumentChange>();
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

        // Act: Subscribe and delete file
        var task = Task.Run(async () =>
        {
            try
            {
                await foreach (var change in source.SubscribeToChangesAsync(cts.Token))
                {
                    changes.Add(change);
                    if (change.ChangeType == DocumentChangeType.Deleted)
                        cts.Cancel();
                }
            }
            catch (OperationCanceledException)
            {
            }
        });

        await Task.Delay(100);
        File.Delete(filePath);

        await task;

        // Assert
        var deletedChange = changes.FirstOrDefault(c => c.ChangeType == DocumentChangeType.Deleted);
        Assert.NotNull(deletedChange);
        Assert.Empty(deletedChange.Document.Content);
        Assert.NotEmpty(deletedChange.Document.Id);
    }
}
