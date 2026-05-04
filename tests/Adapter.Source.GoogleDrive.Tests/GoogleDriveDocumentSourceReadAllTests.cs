using Google.Apis.Drive.v3.Data;
using File = Google.Apis.Drive.v3.Data.File;
using System.Collections.Generic;
using Mnemi.Application.Ports;
using Moq;
using Xunit;
using Mnemi.Application.Ports.Source;
using Mnemi.Application.Ports.Source.Exceptions;

namespace Mnemi.Adapter.Source.GoogleDrive.Tests;

public class GoogleDriveDocumentSourceReadAllTests
{
    private readonly Mock<IGoogleDriveClient> _clientMock;

    public GoogleDriveDocumentSourceReadAllTests()
    {
        _clientMock = new Mock<IGoogleDriveClient>();
    }

    private static async System.Collections.Generic.IAsyncEnumerable<T> ToAsyncEnumerable<T>(System.Collections.Generic.IEnumerable<T> source)
    {
        foreach (var item in source)
            yield return item;
    }

    [Fact]
    public async Task ReadAllAsync_ReturnsAllFilesFromFolder()
    {
        var files = new List<File>
        {
            new() { Id = "file1", Name = "doc1.md", MimeType = "text/plain", ModifiedTime = DateTime.UtcNow },
            new() { Id = "file2", Name = "doc2.md", MimeType = "text/plain", ModifiedTime = DateTime.UtcNow }
        };

        _clientMock.Setup(c => c.ListFilesInFolderAsync("folder123", It.IsAny<CancellationToken>()))
            .Returns(ToAsyncEnumerable(files));

        _clientMock.Setup(c => c.DownloadFileAsStringAsync("file1", It.IsAny<CancellationToken>()))
            .ReturnsAsync("content1");
        _clientMock.Setup(c => c.DownloadFileAsStringAsync("file2", It.IsAny<CancellationToken>()))
            .ReturnsAsync("content2");

        var config = new DocumentSourceConfig
        {
            AdapterType = DocumentSourceAdapterType.GoogleDrive,
            SourceId = "test-source",
            AdapterSpecificConfig = new()
            {
                { "FolderId", "folder123" },
                { "AuthToken", "token123" }
            }
        };
        var source = new GoogleDriveDocumentSource(config, _clientMock.Object);

        var documents = new List<DocumentContent>();
        await foreach (var doc in source.ReadAllAsync(CancellationToken.None))
        {
            documents.Add(doc);
        }

        Assert.Equal(2, documents.Count);
        Assert.Equal("file1", documents[0].Id);
        Assert.Equal("file2", documents[1].Id);
    }

    [Fact]
    public async Task ReadAllAsync_RespectsCancellationToken()
    {
        var files = new List<File>();
        for (int i = 0; i < 10; i++)
            files.Add(new File { Id = $"file{i}", Name = $"doc{i}.md", MimeType = "text/plain" });

        _clientMock.Setup(c => c.ListFilesInFolderAsync("folder123", It.IsAny<CancellationToken>()))
            .Returns(ToAsyncEnumerable(files));

        _clientMock.Setup(c => c.DownloadFileAsStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("content");

        var config = new DocumentSourceConfig
        {
            AdapterType = DocumentSourceAdapterType.GoogleDrive,
            SourceId = "test-source",
            AdapterSpecificConfig = new()
            {
                { "FolderId", "folder123" },
                { "AuthToken", "token123" }
            }
        };
        var source = new GoogleDriveDocumentSource(config, _clientMock.Object);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
        {
            await foreach (var doc in source.ReadAllAsync(cts.Token))
            {
            }
        });
    }

    [Fact]
    public void ReadAllAsync_ThrowsConfigurationException_WhenFolderIdMissing()
    {
        var config = new DocumentSourceConfig
        {
            AdapterType = DocumentSourceAdapterType.GoogleDrive,
            SourceId = "test-source",
            AdapterSpecificConfig = new()
            {
                { "AuthToken", "token123" }
            }
        };

        Assert.Throws<DocumentSourceConfigurationException>(
            () => new GoogleDriveDocumentSource(config, _clientMock.Object));
    }

    [Fact]
    public void ReadAllAsync_ThrowsConfigurationException_WhenAuthTokenMissing()
    {
        var config = new DocumentSourceConfig
        {
            AdapterType = DocumentSourceAdapterType.GoogleDrive,
            SourceId = "test-source",
            AdapterSpecificConfig = new()
            {
                { "FolderId", "folder123" }
            }
        };

        Assert.Throws<DocumentSourceConfigurationException>(
            () => new GoogleDriveDocumentSource(config, _clientMock.Object));
    }

    [Fact]
    public async Task ReadAllAsync_ReturnsEmpty_WhenFolderIsEmpty()
    {
        _clientMock.Setup(c => c.ListFilesInFolderAsync("folder123", It.IsAny<CancellationToken>()))
            .Returns(EmptyAsync<File>());

        var config = new DocumentSourceConfig
        {
            AdapterType = DocumentSourceAdapterType.GoogleDrive,
            SourceId = "test-source",
            AdapterSpecificConfig = new()
            {
                { "FolderId", "folder123" },
                { "AuthToken", "token123" }
            }
        };
        var source = new GoogleDriveDocumentSource(config, _clientMock.Object);

        var documents = new List<DocumentContent>();
        await foreach (var doc in source.ReadAllAsync(CancellationToken.None))
        {
            documents.Add(doc);
        }

        Assert.Empty(documents);
    }

    private static async System.Collections.Generic.IAsyncEnumerable<T> EmptyAsync<T>()
    {
        yield break;
    }
}
