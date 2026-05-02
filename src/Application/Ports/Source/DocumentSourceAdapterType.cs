namespace Mnemi.Application.Ports.Source;

/// <summary>
/// Represents the type of adapter used for document sourcing.
/// </summary>
public enum DocumentSourceAdapterType
{
    /// <summary>
    /// File system based adapter for reading local directory content.
    /// </summary>
    FileSystem = 1,

    /// <summary>
    /// Google Drive based adapter for reading cloud-based folder content.
    /// </summary>
    GoogleDrive = 2
}
