using Mnemi.Application.Ports;
using Mnemi.Application.Ports.Source;
using Mnemi.Application.Ports.Source.Exceptions;
using Xunit;

namespace Mnemi.Adapter.Source.FileSystem.Tests;

public class FileSystemDocumentSourceReadAllTests : IAsyncLifetime
{
    private string _tempDirectory;

    public Task InitializeAsync()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), $"mnemi-test-{Guid.NewGuid()}");
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
    public async Task ReadAllAsync_ReturnsAllMarkdownFiles_WhenFilesExist()
    {
        // Arrange
        await File.WriteAllTextAsync(Path.Combine(_tempDirectory, "file1.md"), "Content 1");
        await File.WriteAllTextAsync(Path.Combine(_tempDirectory, "file2.md"), "Content 2");
        await File.WriteAllTextAsync(Path.Combine(_tempDirectory, "file3.txt"), "Content 3");

        var config = new DocumentSourceConfig
        {
            AdapterType = DocumentSourceAdapterType.FileSystem,
            SourceId = "test-source",
            AdapterSpecificConfig = new() { { "DirectoryPath", _tempDirectory } }
        };
        var source = new FileSystemDocumentSource(config);

        // Act
        var documents = new List<DocumentContent>();
        await foreach (var doc in source.ReadAllAsync(CancellationToken.None))
        {
            documents.Add(doc);
        }

        // Assert
        Assert.Equal(2, documents.Count);
        Assert.All(documents, doc => Assert.True(doc.Id.EndsWith(".md")));
    }

    [Fact]
    public async Task ReadAllAsync_ReturnsEmpty_WhenNoFilesMatch()
    {
        // Arrange
        await File.WriteAllTextAsync(Path.Combine(_tempDirectory, "file.txt"), "Content");

        var config = new DocumentSourceConfig
        {
            AdapterType = DocumentSourceAdapterType.FileSystem,
            SourceId = "test-source",
            AdapterSpecificConfig = new() { { "DirectoryPath", _tempDirectory } }
        };
        var source = new FileSystemDocumentSource(config);

        // Act
        var documents = new List<DocumentContent>();
        await foreach (var doc in source.ReadAllAsync(CancellationToken.None))
        {
            documents.Add(doc);
        }

        // Assert
        Assert.Empty(documents);
    }

    [Fact]
    public async Task ReadAllAsync_RespectsCancellationToken()
    {
        // Arrange
        for (int i = 0; i < 10; i++)
            await File.WriteAllTextAsync(Path.Combine(_tempDirectory, $"file{i}.md"), "Content");

        var config = new DocumentSourceConfig
        {
            AdapterType = DocumentSourceAdapterType.FileSystem,
            SourceId = "test-source",
            AdapterSpecificConfig = new() { { "DirectoryPath", _tempDirectory } }
        };
        var source = new FileSystemDocumentSource(config);
        var cts = new CancellationTokenSource();

        // Act & Assert
        var count = 0;
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
        {
            await foreach (var doc in source.ReadAllAsync(cts.Token))
            {
                count++;
                if (count == 3)
                    cts.Cancel();
            }
        });
        Assert.True(count >= 3);
    }

    [Fact]
    public void ReadAllAsync_ThrowsAccessException_WhenDirectoryDoesNotExist()
    {
        // Arrange
        var config = new DocumentSourceConfig
        {
            AdapterType = DocumentSourceAdapterType.FileSystem,
            SourceId = "test-source",
            AdapterSpecificConfig = new() { { "DirectoryPath", "/nonexistent/path" } }
        };

        // Act & Assert
        var ex = Assert.Throws<DocumentSourceConfigurationException>(
            () => new FileSystemDocumentSource(config));
        Assert.Equal("test-source", ex.SourceId);
    }

    [Fact]
    public async Task ReadAllAsync_PreservesEncoding_InContent()
    {
        // Arrange
        var testContent = "Test content with UTF-8: ñáéíóú";
        await File.WriteAllTextAsync(Path.Combine(_tempDirectory, "utf8.md"), testContent, System.Text.Encoding.UTF8);

        var config = new DocumentSourceConfig
        {
            AdapterType = DocumentSourceAdapterType.FileSystem,
            SourceId = "test-source",
            AdapterSpecificConfig = new() { { "DirectoryPath", _tempDirectory } }
        };
        var source = new FileSystemDocumentSource(config);

        // Act
        var documents = new List<DocumentContent>();
        await foreach (var doc in source.ReadAllAsync(CancellationToken.None))
        {
            documents.Add(doc);
        }

        // Assert
        Assert.Single(documents);
        Assert.Equal(testContent, documents[0].Content);
        Assert.Equal("utf-8", documents[0].Encoding);
    }

    [Fact]
    public async Task ReadAllAsync_IncludesLastModifiedTime()
    {
        // Arrange
        var filePath = Path.Combine(_tempDirectory, "test.md");
        await File.WriteAllTextAsync(filePath, "Content");
        var fileInfo = new FileInfo(filePath);
        var expectedModified = fileInfo.LastWriteTimeUtc;

        var config = new DocumentSourceConfig
        {
            AdapterType = DocumentSourceAdapterType.FileSystem,
            SourceId = "test-source",
            AdapterSpecificConfig = new() { { "DirectoryPath", _tempDirectory } }
        };
        var source = new FileSystemDocumentSource(config);

        // Act
        var documents = new List<DocumentContent>();
        await foreach (var doc in source.ReadAllAsync(CancellationToken.None))
        {
            documents.Add(doc);
        }

        // Assert
        Assert.Single(documents);
        Assert.Equal(expectedModified, documents[0].LastModified, TimeSpan.FromSeconds(1));
    }
}
