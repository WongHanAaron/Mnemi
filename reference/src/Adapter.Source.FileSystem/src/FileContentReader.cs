using System.Text;
using Mnemi.Application.Ports.Source;
using Mnemi.Application.Ports.Source.Exceptions;

namespace Mnemi.Adapter.Source.FileSystem;

/// <summary>
/// Reads file content from the file system with encoding detection.
/// </summary>
public class FileContentReader
{
    /// <summary>
    /// Reads the content of a file and detects its encoding.
    /// </summary>
    /// <param name="filePath">The path to the file to read.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A DocumentContent record with the file content and detected encoding.</returns>
    /// <exception cref="DocumentSourceAccessException">Thrown if the file cannot be read due to access restrictions.</exception>
    /// <exception cref="DocumentSourceException">Thrown if the file cannot be read for other reasons.</exception>
    public async Task<DocumentContent> ReadFileAsync(string filePath, CancellationToken cancellationToken)
    {
        try
        {
            // Open file with BOM detection
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            // Detect encoding from BOM or default to UTF-8
            var encoding = DetectEncoding(fileStream);
            fileStream.Seek(0, SeekOrigin.Begin);

            // Read file content
            using var reader = new StreamReader(fileStream, encoding);
            var content = await reader.ReadToEndAsync(cancellationToken);

            var fileInfo = new FileInfo(filePath);

            return new DocumentContent(
                Id: Path.GetRelativePath(AppDomain.CurrentDomain.BaseDirectory, filePath),
                Content: content,
                LastModified: fileInfo.LastWriteTimeUtc,
                Encoding: encoding.WebName
            );
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new DocumentSourceAccessException(
                $"Access denied when reading file: '{filePath}'.",
                "file-system",
                DocumentSourceAdapterType.FileSystem,
                ex);
        }
        catch (FileNotFoundException ex)
        {
            throw new DocumentSourceAccessException(
                $"File not found: '{filePath}'.",
                "file-system",
                DocumentSourceAdapterType.FileSystem,
                ex);
        }
        catch (IOException ex) when (ex.Message.Contains("denied"))
        {
            throw new DocumentSourceAccessException(
                $"Cannot read file (access denied): '{filePath}'.",
                "file-system",
                DocumentSourceAdapterType.FileSystem,
                ex);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new DocumentSourceException(
                $"Error reading file '{filePath}': {ex.Message}",
                "file-system",
                DocumentSourceAdapterType.FileSystem,
                ex);
        }
    }

    /// <summary>
    /// Detects the encoding of a file by checking for BOM markers.
    /// </summary>
    private static Encoding DetectEncoding(FileStream fileStream)
    {
        var buffer = new byte[4];
        fileStream.Read(buffer, 0, 4);

        // Check for BOM signatures
        if (buffer[0] == 0xef && buffer[1] == 0xbb && buffer[2] == 0xbf)
            return Encoding.UTF8;

        if (buffer[0] == 0xff && buffer[1] == 0xfe && buffer[2] == 0 && buffer[3] == 0)
            return Encoding.UTF32;

        if (buffer[0] == 0xff && buffer[1] == 0xfe)
            return Encoding.Unicode;

        if (buffer[0] == 0xfe && buffer[1] == 0xff)
            return Encoding.BigEndianUnicode;

        // Default to UTF-8
        return Encoding.UTF8;
    }
}
