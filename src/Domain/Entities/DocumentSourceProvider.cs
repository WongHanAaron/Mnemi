namespace Mnemi.Domain.Entities;

/// <summary>
/// Identifies the storage provider for a document source.
/// </summary>
public enum DocumentSourceProvider
{
    /// <summary>Google Drive folder</summary>
    GoogleDrive = 1,

    /// <summary>GitHub repository</summary>
    GitHub = 2
}
