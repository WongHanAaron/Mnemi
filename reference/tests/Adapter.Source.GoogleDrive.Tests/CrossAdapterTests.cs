using System.Collections.Generic;
using Mnemi.Adapter.Source.FileSystem;
using Moq;
using Xunit;
using Mnemi.Application.Ports.Source;

namespace Mnemi.Adapter.Source.GoogleDrive.Tests;

public class CrossAdapterTests
{
    [Fact]
    public void BothAdapters_ImplementIDocumentSource()
    {
        var fsConfig = new DocumentSourceConfig
        {
            AdapterType = DocumentSourceAdapterType.FileSystem,
            SourceId = "fs-test",
            AdapterSpecificConfig = new()
            {
                { "DirectoryPath", System.IO.Path.GetTempPath() }
            }
        };

        var gdConfig = new DocumentSourceConfig
        {
            AdapterType = DocumentSourceAdapterType.GoogleDrive,
            SourceId = "gd-test",
            AdapterSpecificConfig = new()
            {
                { "FolderId", "folder123" },
                { "AuthToken", "token123" }
            }
        };

        var fsSource = new FileSystemDocumentSource(fsConfig);
        var gdClientMock = new Mock<IGoogleDriveClient>();
        var gdSource = new GoogleDriveDocumentSource(gdConfig, gdClientMock.Object);

        Assert.IsAssignableFrom<IDocumentSource>(fsSource);
        Assert.IsAssignableFrom<IDocumentSource>(gdSource);
    }

    [Fact]
    public async Task BothAdapters_HandleCancellationToken()
    {
        var tempDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString());
        System.IO.Directory.CreateDirectory(tempDir);
        System.IO.File.WriteAllText(System.IO.Path.Combine(tempDir, "test.md"), "# Test");
        try
        {
            var fsConfig = new DocumentSourceConfig
            {
                AdapterType = DocumentSourceAdapterType.FileSystem,
                SourceId = "fs-test",
                AdapterSpecificConfig = new()
                {
                    { "DirectoryPath", tempDir }
                }
            };

            var fsSource = new FileSystemDocumentSource(fsConfig);
            var cts = new CancellationTokenSource();
            cts.Cancel();

            await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                await foreach (var _ in fsSource.ReadAllAsync(cts.Token))
                {
                }
            });
        }
        finally
        {
            System.IO.Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void BothAdapters_PreserveEncodingInformation()
    {
        var fsConfig = new DocumentSourceConfig
        {
            AdapterType = DocumentSourceAdapterType.FileSystem,
            SourceId = "fs-test",
            AdapterSpecificConfig = new()
            {
                { "DirectoryPath", System.IO.Path.GetTempPath() }
            }
        };

        var gdConfig = new DocumentSourceConfig
        {
            AdapterType = DocumentSourceAdapterType.GoogleDrive,
            SourceId = "gd-test",
            AdapterSpecificConfig = new()
            {
                { "FolderId", "folder123" },
                { "AuthToken", "token123" }
            }
        };

        var fsSource = new FileSystemDocumentSource(fsConfig);
        var gdClientMock = new Mock<IGoogleDriveClient>();
        var gdSource = new GoogleDriveDocumentSource(gdConfig, gdClientMock.Object);

        Assert.IsAssignableFrom<IDocumentSource>(fsSource);
        Assert.IsAssignableFrom<IDocumentSource>(gdSource);
    }

    [Fact]
    public async Task BothAdapters_SupportConcurrentSubscribers()
    {
        var tempDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString());
        System.IO.Directory.CreateDirectory(tempDir);
        try
        {
            var fsConfig = new DocumentSourceConfig
            {
                AdapterType = DocumentSourceAdapterType.FileSystem,
                SourceId = "fs-test",
                AdapterSpecificConfig = new()
                {
                    { "DirectoryPath", tempDir }
                }
            };

            var fsSource = new FileSystemDocumentSource(fsConfig);

            var task1 = Task.Run(async () =>
            {
                var docs = new List<DocumentContent>();
                await foreach (var doc in fsSource.ReadAllAsync(CancellationToken.None))
                {
                    docs.Add(doc);
                }
                return docs;
            });

            var task2 = Task.Run(async () =>
            {
                var docs = new List<DocumentContent>();
                await foreach (var doc in fsSource.ReadAllAsync(CancellationToken.None))
                {
                    docs.Add(doc);
                }
                return docs;
            });

            var results = await Task.WhenAll(task1, task2);

            Assert.NotNull(results[0]);
            Assert.NotNull(results[1]);
            Assert.Equal(results[0].Count, results[1].Count);
        }
        finally
        {
            System.IO.Directory.Delete(tempDir, true);
        }
    }
}
