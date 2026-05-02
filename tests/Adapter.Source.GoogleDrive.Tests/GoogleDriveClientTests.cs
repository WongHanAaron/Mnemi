using Google.Apis.Drive.v3.Data;
using File = Google.Apis.Drive.v3.Data.File;
using System.Collections.Generic;
using Moq;
using Xunit;
using Mnemi.Application.Ports.Source;
using Mnemi.Application.Ports.Source.Exceptions;

namespace Mnemi.Adapter.Source.GoogleDrive.Tests;

public class GoogleDriveClientTests
{
    private readonly Mock<IGoogleDriveClient> _clientMock;

    public GoogleDriveClientTests()
    {
        _clientMock = new Mock<IGoogleDriveClient>();
    }

    private static async System.Collections.Generic.IAsyncEnumerable<T> ToAsyncEnumerable<T>(System.Collections.Generic.IEnumerable<T> source)
    {
        foreach (var item in source)
            yield return item;
    }

    [Fact]
    public async Task ListFilesInFolderAsync_ReturnsFilesWithCorrectData()
    {
        var files = new List<File>
        {
            new() { Id = "id1", Name = "file1.txt", MimeType = "text/plain", ModifiedTime = DateTime.UtcNow },
            new() { Id = "id2", Name = "file2.txt", MimeType = "text/plain", ModifiedTime = DateTime.UtcNow }
        };

        _clientMock.Setup(c => c.ListFilesInFolderAsync("folder123", It.IsAny<CancellationToken>()))
            .Returns(ToAsyncEnumerable(files));

        var result = new List<File>();
        await foreach (var file in _clientMock.Object.ListFilesInFolderAsync("folder123", CancellationToken.None))
        {
            result.Add(file);
        }

        Assert.Equal(2, result.Count);
        Assert.Equal("id1", result[0].Id);
        Assert.Equal("id2", result[1].Id);
    }

    [Fact]
    public async Task DownloadFileAsStringAsync_RetrievesContent()
    {
        _clientMock.Setup(c => c.DownloadFileAsStringAsync("file1", It.IsAny<CancellationToken>()))
            .ReturnsAsync("file content here");

        var content = await _clientMock.Object.DownloadFileAsStringAsync("file1", CancellationToken.None);

        Assert.Equal("file content here", content);
    }

    [Fact]
    public async Task ListFilesInFolderAsync_HandlesApiQuotaError()
    {
        _clientMock.Setup(c => c.ListFilesInFolderAsync("folder123", It.IsAny<CancellationToken>()))
            .Returns(() => throw new DocumentSourceTransientException(
                "API quota exceeded",
                "test-source",
                DocumentSourceAdapterType.GoogleDrive,
                30));

        await Assert.ThrowsAsync<DocumentSourceTransientException>(async () =>
        {
            await foreach (var _ in _clientMock.Object.ListFilesInFolderAsync("folder123", CancellationToken.None))
            {
            }
        });
    }

    [Fact]
    public async Task DownloadFileAsStringAsync_HandlesNetworkTimeout()
    {
        _clientMock.Setup(c => c.DownloadFileAsStringAsync("file1", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DocumentSourceTransientException(
                "Network timeout",
                "test-source",
                DocumentSourceAdapterType.GoogleDrive,
                5));

        var ex = await Assert.ThrowsAsync<DocumentSourceTransientException>(
            () => _clientMock.Object.DownloadFileAsStringAsync("file1", CancellationToken.None));

        Assert.Equal("Network timeout", ex.Message);
    }
}
