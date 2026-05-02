using System;
using System.Text;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Mnemi.Application.Ports.Source;
using Mnemi.Application.Ports.Source.Exceptions;

namespace Mnemi.Adapter.Source.GoogleDrive;

/// <summary>
/// Wraps the Google Drive API with error handling and simplified access patterns.
/// </summary>
public class GoogleDriveClient : IGoogleDriveClient
{
    private readonly DriveService _driveService;
    private readonly string _sourceId;
    private const int MaxPageSize = 100;

    /// <summary>
    /// Creates a new instance of GoogleDriveClient.
    /// </summary>
    /// <param name="driveService">The configured Google Drive service.</param>
    /// <param name="sourceId">The source ID for error context.</param>
    public GoogleDriveClient(DriveService driveService, string sourceId)
    {
        _driveService = driveService ?? throw new ArgumentNullException(nameof(driveService));
        _sourceId = sourceId ?? throw new ArgumentNullException(nameof(sourceId));
    }

    /// <summary>
    /// Lists all files in the specified folder with pagination support.
    /// </summary>
    /// <param name="folderId">The Google Drive folder ID.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>An async enumerable of File objects representing all files in the folder.</returns>
    /// <exception cref="DocumentSourceAuthenticationException">Thrown on authentication failures.</exception>
    /// <exception cref="DocumentSourceAccessException">Thrown when access is denied.</exception>
    /// <exception cref="DocumentSourceTransientException">Thrown on transient API errors.</exception>
    public IAsyncEnumerable<Google.Apis.Drive.v3.Data.File> ListFilesInFolderAsync(
        string folderId,
        CancellationToken cancellationToken)
    {
        return ListFilesCoreAsync(folderId, cancellationToken);
    }

    private async IAsyncEnumerable<Google.Apis.Drive.v3.Data.File> ListFilesCoreAsync(
        string folderId,
        CancellationToken cancellationToken)
    {
        var pageToken = "";
        var request = _driveService.Files.List();
        request.Q = $"'{folderId}' in parents and trashed=false";
        request.Fields = "files(id, name, mimeType, modifiedTime, size)";
        request.PageSize = MaxPageSize;

        while (!string.IsNullOrEmpty(pageToken) || pageToken == "")
        {
            cancellationToken.ThrowIfCancellationRequested();

            request.PageToken = pageToken == "" ? null : pageToken;

            Google.Apis.Drive.v3.Data.FileList result;
            try
            {
                result = await request.ExecuteAsync(cancellationToken);
            }
            catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new DocumentSourceAuthenticationException(
                    "Authentication failed when accessing Google Drive API.",
                    _sourceId,
                    DocumentSourceAdapterType.GoogleDrive,
                    ex);
            }
            catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                throw new DocumentSourceAccessException(
                    $"Access denied to folder '{folderId}' or Google Drive API.",
                    _sourceId,
                    DocumentSourceAdapterType.GoogleDrive,
                    ex);
            }
            catch (Google.GoogleApiException ex) when ((int)ex.HttpStatusCode >= 500)
            {
                throw new DocumentSourceTransientException(
                    $"Transient error from Google Drive API: {ex.Message}",
                    _sourceId,
                    DocumentSourceAdapterType.GoogleDrive,
                    5,
                    ex);
            }
            catch (HttpRequestException ex)
            {
                throw new DocumentSourceTransientException(
                    $"Network error accessing Google Drive API: {ex.Message}",
                    _sourceId,
                    DocumentSourceAdapterType.GoogleDrive,
                    5,
                    ex);
            }

            if (result?.Files != null)
            {
                foreach (var file in result.Files)
                {
                    // Skip folders, only yield files
                    if (file.MimeType != "application/vnd.google-apps.folder")
                        yield return file;
                }
            }

            pageToken = result?.NextPageToken ?? "";

            if (string.IsNullOrEmpty(pageToken))
                break;
        }
    }

    /// <summary>
    /// Downloads a file from Google Drive as a string.
    /// </summary>
    /// <param name="fileId">The Google Drive file ID.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The file content as a string.</returns>
    /// <exception cref="DocumentSourceAuthenticationException">Thrown on authentication failures.</exception>
    /// <exception cref="DocumentSourceAccessException">Thrown when access is denied.</exception>
    /// <exception cref="DocumentSourceTransientException">Thrown on transient API errors.</exception>
    public async Task<string> DownloadFileAsStringAsync(string fileId, CancellationToken cancellationToken)
    {
        using var stream = new MemoryStream();
        var request = _driveService.Files.Get(fileId);
        request.MediaDownloader.ChunkSize = 256 * 1024; // 256KB chunks

        try
        {
            var progress = await request.DownloadAsync(stream, cancellationToken);

            if (progress.Status == Google.Apis.Download.DownloadStatus.Failed)
            {
                throw progress.Exception ?? new Exception("Download failed for unknown reason");
            }

            stream.Position = 0;
            using var reader = new StreamReader(stream, Encoding.UTF8);
            return await reader.ReadToEndAsync(cancellationToken);
        }
        catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            throw new DocumentSourceAuthenticationException(
                "Authentication failed when downloading file from Google Drive.",
                _sourceId,
                DocumentSourceAdapterType.GoogleDrive,
                ex);
        }
        catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.Forbidden || ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new DocumentSourceAccessException(
                $"Cannot access file '{fileId}' on Google Drive (not found or permission denied).",
                _sourceId,
                DocumentSourceAdapterType.GoogleDrive,
                ex);
        }
        catch (Google.GoogleApiException ex) when ((int)ex.HttpStatusCode >= 500)
        {
            throw new DocumentSourceTransientException(
                $"Transient error downloading file from Google Drive: {ex.Message}",
                _sourceId,
                DocumentSourceAdapterType.GoogleDrive,
                5,
                ex);
        }
        catch (HttpRequestException ex)
        {
            throw new DocumentSourceTransientException(
                $"Network error downloading file from Google Drive: {ex.Message}",
                _sourceId,
                DocumentSourceAdapterType.GoogleDrive,
                5,
                ex);
        }
    }
}
