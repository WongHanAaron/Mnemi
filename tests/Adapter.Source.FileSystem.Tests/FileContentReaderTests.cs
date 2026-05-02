using System.Text;
using Mnemi.Application.Ports.Source;
using Mnemi.Application.Ports.Source.Exceptions;
using Xunit;

namespace Mnemi.Adapter.Source.FileSystem.Tests;

public class FileContentReaderTests : IAsyncLifetime
{
    private string _tempDirectory;
    private FileContentReader _reader;

    public Task InitializeAsync()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), $"mnemi-reader-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDirectory);
        _reader = new FileContentReader();
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        if (Directory.Exists(_tempDirectory))
            Directory.Delete(_tempDirectory, recursive: true);
        return Task.CompletedTask;
    }

    [Fact]
    public async Task ReadFileAsync_ReadsUTF8FileCorrectly()
    {
        // Arrange
        var content = "Hello, World! ñáéíóú";
        var filePath = Path.Combine(_tempDirectory, "utf8.txt");
        await File.WriteAllTextAsync(filePath, content, Encoding.UTF8);

        // Act
        var result = await _reader.ReadFileAsync(filePath, CancellationToken.None);

        // Assert
        Assert.Equal(content, result.Content);
        Assert.Equal("utf-8", result.Encoding);
    }

    [Fact]
    public async Task ReadFileAsync_DetectsBOMEncoding_UTF16()
    {
        // Arrange
        var content = "UTF-16 Content";
        var filePath = Path.Combine(_tempDirectory, "utf16.txt");
        await File.WriteAllTextAsync(filePath, content, Encoding.Unicode);

        // Act
        var result = await _reader.ReadFileAsync(filePath, CancellationToken.None);

        // Assert
        Assert.Equal(content, result.Content);
        Assert.NotNull(result.Encoding);
    }

    [Fact]
    public async Task ReadFileAsync_ThrowsAccessException_WhenFileNotFound()
    {
        // Arrange
        var nonexistentPath = Path.Combine(_tempDirectory, "nonexistent.txt");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DocumentSourceAccessException>(
            () => _reader.ReadFileAsync(nonexistentPath, CancellationToken.None));
        Assert.Equal("file-system", ex.SourceId);
        Assert.Equal(DocumentSourceAdapterType.FileSystem, ex.AdapterType);
    }

    [Fact]
    public async Task ReadFileAsync_PreservesLineEndings()
    {
        // Arrange
        var contentWithCRLF = "Line 1\r\nLine 2\r\nLine 3";
        var filePath = Path.Combine(_tempDirectory, "crlf.txt");
        await File.WriteAllTextAsync(filePath, contentWithCRLF);

        // Act
        var result = await _reader.ReadFileAsync(filePath, CancellationToken.None);

        // Assert
        Assert.Equal(contentWithCRLF, result.Content);
    }

    [Fact]
    public async Task ReadFileAsync_RespectsCancellationToken()
    {
        // Arrange
        var largeContent = string.Join("\n", Enumerable.Range(0, 10000).Select(_ => "Line"));
        var filePath = Path.Combine(_tempDirectory, "large.txt");
        await File.WriteAllTextAsync(filePath, largeContent);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => _reader.ReadFileAsync(filePath, cts.Token));
    }

    [Fact]
    public async Task ReadFileAsync_ReturnsValidDocumentContent()
    {
        // Arrange
        var content = "Test content";
        var filePath = Path.Combine(_tempDirectory, "test.txt");
        await File.WriteAllTextAsync(filePath, content);

        // Act
        var result = await _reader.ReadFileAsync(filePath, CancellationToken.None);

        // Assert
        Assert.NotEmpty(result.Id);
        Assert.Equal(content, result.Content);
        Assert.True(result.LastModified <= DateTime.UtcNow);
        Assert.NotEmpty(result.Encoding);
    }
}
