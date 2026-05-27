namespace Mnemi.Application.Ports;

/// <summary>
/// Service for managing a user's linked document sources.
/// </summary>
public interface IDocumentSourceService
{
    /// <summary>
    /// Lists all document sources for the current user.
    /// </summary>
    Task<IReadOnlyList<SourceInfo>> ListSourcesAsync(CancellationToken ct = default);

    /// <summary>
    /// Updates the display name of a document source.
    /// </summary>
    Task<UserUpdateResult> UpdateSourceDisplayNameAsync(string sourceId, string displayName, CancellationToken ct = default);

    /// <summary>
    /// Removes (unlinks) a document source.
    /// </summary>
    Task<UserUpdateResult> RemoveSourceAsync(string sourceId, CancellationToken ct = default);
}

/// <summary>
/// Information about a linked document source.
/// </summary>
public sealed record SourceInfo(
    string SourceId,
    string DisplayName,
    string ProviderName,    // "GoogleDrive" or "GitHub"
    bool IsAccessible,
    string? LastError,
    DateTime CreatedAt
);
