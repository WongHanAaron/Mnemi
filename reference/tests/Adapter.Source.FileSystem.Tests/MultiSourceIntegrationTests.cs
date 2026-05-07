using System.Diagnostics;
using Mnemi.Adapter.Source.FileSystem;
using Mnemi.Application.Ports.Source;
using Xunit;

namespace Mnemi.Adapter.Source.FileSystem.Tests;

public class MultiSourceIntegrationTests : IAsyncLifetime
{
    private readonly string _sourceDir1;
    private readonly string _sourceDir2;

    public MultiSourceIntegrationTests()
    {
        _sourceDir1 = Path.Combine(Path.GetTempPath(), $"mnemi-test-1-{Guid.NewGuid():N}");
        _sourceDir2 = Path.Combine(Path.GetTempPath(), $"mnemi-test-2-{Guid.NewGuid():N}");
    }

    public Task InitializeAsync()
    {
        Directory.CreateDirectory(_sourceDir1);
        Directory.CreateDirectory(_sourceDir2);
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        try
        {
            if (Directory.Exists(_sourceDir1))
                Directory.Delete(_sourceDir1, recursive: true);
            if (Directory.Exists(_sourceDir2))
                Directory.Delete(_sourceDir2, recursive: true);
        }
        catch
        {
            // Ignore cleanup errors during disposal
        }
        return Task.CompletedTask;
    }

    [Fact]
    public async Task MultipleSourcesIndependent_DifferentDirectories()
    {
        // Arrange: Create two separate file system sources
        var source1 = FileSystemAdapterFactory.CreateMarkdownSource("source-1", _sourceDir1);
        var source2 = FileSystemAdapterFactory.CreateMarkdownSource("source-2", _sourceDir2);

        var file1Path = Path.Combine(_sourceDir1, "doc1.md");
        var file2Path = Path.Combine(_sourceDir2, "doc2.md");

        File.WriteAllText(file1Path, "# Document 1");
        File.WriteAllText(file2Path, "# Document 2");

        // Act: Read all documents from both sources
        var source1Docs = new List<DocumentContent>();
        var source2Docs = new List<DocumentContent>();

        await foreach (var doc in source1.ReadAllAsync(CancellationToken.None))
            source1Docs.Add(doc);

        await foreach (var doc in source2.ReadAllAsync(CancellationToken.None))
            source2Docs.Add(doc);

        // Assert: Each source only sees its own files
        Assert.Single(source1Docs);
        Assert.Single(source2Docs);
        Assert.NotEqual(source1Docs[0].Id, source2Docs[0].Id);
    }

    [Fact]
    public async Task MultipleSourceSubscriptions_AreIndependent()
    {
        // Arrange
        var source1 = FileSystemAdapterFactory.CreateMarkdownSource("source-1", _sourceDir1);
        var source2 = FileSystemAdapterFactory.CreateMarkdownSource("source-2", _sourceDir2);

        var source1Changes = new List<DocumentChange>();
        var source2Changes = new List<DocumentChange>();
        var cts1 = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        var cts2 = new CancellationTokenSource(TimeSpan.FromSeconds(2));

        // Act: Start subscriptions
        var task1 = Task.Run(async () =>
        {
            try
            {
                await foreach (var change in source1.SubscribeToChangesAsync(cts1.Token))
                {
                    source1Changes.Add(change);
                    if (source1Changes.Count >= 1)
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
                await foreach (var change in source2.SubscribeToChangesAsync(cts2.Token))
                {
                    source2Changes.Add(change);
                    if (source2Changes.Count >= 1)
                        cts2.Cancel();
                }
            }
            catch (OperationCanceledException)
            {
            }
        });

        // Wait for subscriptions to be ready
        await Task.Delay(100);

        // Modify only source 1
        var file1Path = Path.Combine(_sourceDir1, "new.md");
        File.WriteAllText(file1Path, "# New Document");

        // Wait for subscriptions to complete
        await Task.WhenAll(task1, task2);

        // Assert: Source 1 detected the change, Source 2 did not
        Assert.NotEmpty(source1Changes);
        Assert.Empty(source2Changes);
    }

    [Fact]
    public async Task ChangesInOneSource_DontTriggerOtherSource()
    {
        // Arrange
        var source1 = FileSystemAdapterFactory.CreateMarkdownSource("source-1", _sourceDir1);
        var source2 = FileSystemAdapterFactory.CreateMarkdownSource("source-2", _sourceDir2);

        var file1InitialPath = Path.Combine(_sourceDir1, "initial.md");
        File.WriteAllText(file1InitialPath, "Initial content");

        var source1Changes = new List<DocumentChange>();
        var source2Changes = new List<DocumentChange>();
        var cts1 = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));
        var cts2 = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));

        // Act: Subscribe to both sources and modify source 1
        var task1 = Task.Run(async () =>
        {
            try
            {
                await foreach (var change in source1.SubscribeToChangesAsync(cts1.Token))
                    source1Changes.Add(change);
            }
            catch (OperationCanceledException)
            {
            }
        });

        var task2 = Task.Run(async () =>
        {
            try
            {
                await foreach (var change in source2.SubscribeToChangesAsync(cts2.Token))
                    source2Changes.Add(change);
            }
            catch (OperationCanceledException)
            {
            }
        });

        // Wait for subscriptions to be ready
        await Task.Delay(50);

        // Create multiple changes in source 1 only
        for (int i = 0; i < 3; i++)
        {
            File.WriteAllText(Path.Combine(_sourceDir1, $"doc{i}.md"), $"Content {i}");
            await Task.Delay(50);
        }

        // Wait for subscriptions to complete
        await Task.WhenAll(task1, task2);

        // Assert: Source 1 received changes, Source 2 received none
        Assert.NotEmpty(source1Changes);
        Assert.Empty(source2Changes);
    }

    [Fact]
    public async Task MultipleSourcesCanCoexistWithDifferentSearchPatterns()
    {
        // Arrange: Create sources with different search patterns
        var source1 = FileSystemAdapterFactory.CreateMarkdownSource("source-1", _sourceDir1, "*.md");
        var source2 = FileSystemAdapterFactory.CreateMarkdownSource("source-2", _sourceDir1, "*.txt");

        File.WriteAllText(Path.Combine(_sourceDir1, "doc.md"), "Markdown");
        File.WriteAllText(Path.Combine(_sourceDir1, "note.txt"), "Text");

        // Act
        var source1Docs = new List<DocumentContent>();
        var source2Docs = new List<DocumentContent>();

        await foreach (var doc in source1.ReadAllAsync(CancellationToken.None))
            source1Docs.Add(doc);

        await foreach (var doc in source2.ReadAllAsync(CancellationToken.None))
            source2Docs.Add(doc);

        // Assert: Each source sees only files matching its pattern
        Assert.NotEmpty(source1Docs);
        Assert.NotEmpty(source2Docs);
        Assert.All(source1Docs, doc => Assert.Contains(".md", doc.Id));
        Assert.All(source2Docs, doc => Assert.Contains(".txt", doc.Id));
    }

    [Fact]
    public async Task FactoryMethodsCreateIndependentInstances()
    {
        // Arrange & Act
        var source1a = FileSystemAdapterFactory.CreateMarkdownSource("source-1", _sourceDir1);
        var source1b = FileSystemAdapterFactory.CreateMarkdownSource("source-1", _sourceDir1);

        File.WriteAllText(Path.Combine(_sourceDir1, "test.md"), "Content");

        var docs1a = new List<DocumentContent>();
        var docs1b = new List<DocumentContent>();

        await foreach (var doc in source1a.ReadAllAsync(CancellationToken.None))
            docs1a.Add(doc);

        await foreach (var doc in source1b.ReadAllAsync(CancellationToken.None))
            docs1b.Add(doc);

        // Assert: Both instances see the same content even though they're independent
        Assert.Equal(docs1a.Count, docs1b.Count);
        Assert.Equal(docs1a[0].Content, docs1b[0].Content);
    }
}
