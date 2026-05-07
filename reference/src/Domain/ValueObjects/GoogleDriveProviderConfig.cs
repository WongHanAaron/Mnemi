namespace Mnemi.Domain.ValueObjects;

/// <summary>
/// Configuration specific to Google Drive document sources.
/// </summary>
public class GoogleDriveProviderConfig
{
    /// <summary>
    /// Google Drive folder ID.
    /// Found in the URL: drive.google.com/drive/folders/{folderId}
    /// </summary>
    public string FolderId { get; set; } = string.Empty;

    /// <summary>
    /// Optional path within the folder for nested organization.
    /// </summary>
    public string? SubPath { get; set; }
}
