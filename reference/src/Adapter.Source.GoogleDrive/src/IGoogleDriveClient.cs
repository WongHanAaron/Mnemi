using System;
using Mnemi.Application.Ports;
using Mnemi.Application.Ports.Source.Exceptions;

namespace Mnemi.Adapter.Source.GoogleDrive;

public interface IGoogleDriveClient
{
    IAsyncEnumerable<Google.Apis.Drive.v3.Data.File> ListFilesInFolderAsync(string folderId, CancellationToken cancellationToken);
    Task<string> DownloadFileAsStringAsync(string fileId, CancellationToken cancellationToken);
}
