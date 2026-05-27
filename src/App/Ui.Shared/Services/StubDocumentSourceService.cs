using Mnemi.Application.Ports;

namespace Ui.Shared.Services;

/// <summary>
/// Shared stub implementation of IDocumentSourceService for development.
/// Stores source data in memory. Used by both the Web and MAUI hosts
/// until real DB-backed services are ready.
/// </summary>
public sealed class StubDocumentSourceService : IDocumentSourceService
{
    private readonly List<SourceInfo> _sources = new()
    {
        new SourceInfo("src-bio-1", "Biology Decks", "GoogleDrive", true, null,
            new DateTime(2026, 2, 1)),
        new SourceInfo("src-cs-1", "CS Notes", "GitHub", false,
            "Repository not found or access revoked",
            new DateTime(2026, 4, 5)),
    };

    public Task<IReadOnlyList<SourceInfo>> ListSourcesAsync(CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<SourceInfo>>(_sources.ToList());

    public Task<UserUpdateResult> UpdateSourceDisplayNameAsync(
        string sourceId, string displayName, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            return Task.FromResult(UserUpdateResult.Fail("Source name is required."));

        if (displayName.Length > ValidationConstants.MaxSourceNameLength)
            return Task.FromResult(UserUpdateResult.Fail($"Source name must be {ValidationConstants.MaxSourceNameLength} characters or less."));

        var source = _sources.FirstOrDefault(s => s.SourceId == sourceId);
        if (source is null)
            return Task.FromResult(UserUpdateResult.Fail("Source not found."));

        var index = _sources.IndexOf(source);
        _sources[index] = source with { DisplayName = displayName.Trim() };
        return Task.FromResult(UserUpdateResult.Ok());
    }

    public Task<UserUpdateResult> RemoveSourceAsync(string sourceId, CancellationToken ct = default)
    {
        var source = _sources.FirstOrDefault(s => s.SourceId == sourceId);
        if (source is null)
            return Task.FromResult(UserUpdateResult.Fail("Source not found."));

        _sources.Remove(source);
        return Task.FromResult(UserUpdateResult.Ok());
    }
}
