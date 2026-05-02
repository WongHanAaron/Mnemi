using Google.Apis.Drive.v3.Data;
using File = Google.Apis.Drive.v3.Data.File;
using System.Collections.Generic;
using Mnemi.Application.Ports;
using Moq;
using Xunit;
using Application.Ports.Source;
using Mnemi.Application.Source;
using Mnemi.Application.Ports.Source;

namespace Mnemi.Adapter.Source.GoogleDrive.Tests;

public class GoogleDriveDocumentSourceSubscribeTests
{
    private readonly Mock<IGoogleDriveClient> _clientMock;

    public GoogleDriveDocumentSourceSubscribeTests()
    {
        _clientMock = new Mock<IGoogleDriveClient>();
    }

    private static async System.Collections.Generic.IAsyncEnumerable<T> ToAsyncEnumerable<T>(System.Collections.Generic.IEnumerable<T> source)
    {
        foreach (var item in source)
            yield return item;
    }

    private static async System.Collections.Generic.IAsyncEnumerable<T> EmptyAsync<T>()
    {
        yield break;
    }

    [Fact]
    public async Task SubscribeToChangesAsync_YieldsCreatedEvent_WhenNewFile()
    {
        var initialFiles = new List<File>();
        var updatedFiles = new List<File>
        {
            new() { Id = "file1", Name = "new.md", MimeType = "text/plain", ModifiedTime = DateTime.UtcNow }
        };

        var callCount = 0;
        _clientMock.Setup(c => c.ListFilesInFolderAsync("folder123", It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                callCount++;
                var result = callCount == 1 ? initialFiles : updatedFiles;
                return ToAsyncEnumerable(result);
            });

        _clientMock.Setup(c => c.DownloadFileAsStringAsync("file1", It.IsAny<CancellationToken>()))
            .ReturnsAsync("new content");

        var config = new DocumentSourceConfig
        {
            AdapterType = DocumentSourceAdapterType.GoogleDrive,
            SourceId = "test-source",
            AdapterSpecificConfig = new()
            {
                { "FolderId", "folder123" },
                { "AuthToken", "token123" },
                { "PollingIntervalMs", 100 }
            }
        };
        var source = new GoogleDriveDocumentSource(config, _clientMock.Object);
        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(2));

        var changes = new List<DocumentChange>();
        await foreach (var change in source.SubscribeToChangesAsync(cts.Token))
        {
            changes.Add(change);
            cts.Cancel();
        }

        Assert.Single(changes);
        Assert.Equal(DocumentChangeType.Created, changes[0].ChangeType);
        Assert.Equal("file1", changes[0].Document.Id);
        Assert.Equal("new content", changes[0].Document.Content);
    }

    [Fact]
    public async Task SubscribeToChangesAsync_YieldsModifiedEvent_WhenFileUpdated()
    {
        var initialTime = DateTime.UtcNow.AddMinutes(-5);
        var updatedTime = DateTime.UtcNow;

        var initialFiles = new List<File>
        {
            new() { Id = "file1", Name = "doc.md", MimeType = "text/plain", ModifiedTime = initialTime }
        };
        var updatedFiles = new List<File>
        {
            new() { Id = "file1", Name = "doc.md", MimeType = "text/plain", ModifiedTime = updatedTime }
        };

        var callCount = 0;
        _clientMock.Setup(c => c.ListFilesInFolderAsync("folder123", It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                callCount++;
                var result = callCount == 1 ? initialFiles : updatedFiles;
                return ToAsyncEnumerable(result);
            });

        _clientMock.Setup(c => c.DownloadFileAsStringAsync("file1", It.IsAny<CancellationToken>()))
            .ReturnsAsync("updated content");

        var config = new DocumentSourceConfig
        {
            AdapterType = DocumentSourceAdapterType.GoogleDrive,
            SourceId = "test-source",
            AdapterSpecificConfig = new()
            {
                { "FolderId", "folder123" },
                { "AuthToken", "token123" },
                { "PollingIntervalMs", 100 }
            }
        };
        var source = new GoogleDriveDocumentSource(config, _clientMock.Object);
        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(2));

        var changes = new List<DocumentChange>();
        await foreach (var change in source.SubscribeToChangesAsync(cts.Token))
        {
            changes.Add(change);
            cts.Cancel();
        }

        Assert.Single(changes);
        Assert.Equal(DocumentChangeType.Modified, changes[0].ChangeType);
        Assert.Equal("file1", changes[0].Document.Id);
    }

    [Fact]
    public async Task SubscribeToChangesAsync_YieldsDeletedEvent_WhenFileRemoved()
    {
        var initialFiles = new List<File>
        {
            new() { Id = "file1", Name = "doc.md", MimeType = "text/plain", ModifiedTime = DateTime.UtcNow }
        };
        var updatedFiles = new List<File>();

        var callCount = 0;
        _clientMock.Setup(c => c.ListFilesInFolderAsync("folder123", It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                callCount++;
                var result = callCount == 1 ? initialFiles : updatedFiles;
                return ToAsyncEnumerable(result);
            });

        var config = new DocumentSourceConfig
        {
            AdapterType = DocumentSourceAdapterType.GoogleDrive,
            SourceId = "test-source",
            AdapterSpecificConfig = new()
            {
                { "FolderId", "folder123" },
                { "AuthToken", "token123" },
                { "PollingIntervalMs", 100 }
            }
        };
        var source = new GoogleDriveDocumentSource(config, _clientMock.Object);
        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(2));

        var changes = new List<DocumentChange>();
        await foreach (var change in source.SubscribeToChangesAsync(cts.Token))
        {
            changes.Add(change);
            cts.Cancel();
        }

        Assert.Single(changes);
        Assert.Equal(DocumentChangeType.Deleted, changes[0].ChangeType);
        Assert.Equal("file1", changes[0].Document.Id);
        Assert.Equal(string.Empty, changes[0].Document.Content);
    }

    [Fact]
    public async Task SubscribeToChangesAsync_RespectsCancellationToken()
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
                { "AuthToken", "token123" },
                { "PollingIntervalMs", 100 }
            }
        };
        var source = new GoogleDriveDocumentSource(config, _clientMock.Object);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var changes = new List<DocumentChange>();
        await foreach (var change in source.SubscribeToChangesAsync(cts.Token))
        {
            changes.Add(change);
        }

        Assert.Empty(changes);
    }
}
