using Mnemi.Application.Ports;

namespace Ui.Shared.Services;

/// <summary>
/// Shared stub implementation of IUserService for development.
/// Stores display name and provider data in memory. Used by both
/// the Web and MAUI hosts until real DB-backed services are ready.
/// </summary>
public sealed class StubUserService : IUserService
{
    private string _displayName = "Student";
    private readonly DateTime _createdAt = new(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc);

    private readonly List<ProviderInfo> _providers = new()
    {
        new ProviderInfo("conn-google-1", "Google", "student@gmail.com", true, new DateTime(2026, 1, 15)),
        new ProviderInfo("conn-github-1", "GitHub", "student-dev", true, new DateTime(2026, 3, 10)),
    };

    public Task<UserUpdateResult> UpdateDisplayNameAsync(string displayName, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            return Task.FromResult(UserUpdateResult.Fail("Display name is required."));

        if (displayName.Length > ValidationConstants.MaxDisplayNameLength)
            return Task.FromResult(UserUpdateResult.Fail($"Display name must be {ValidationConstants.MaxDisplayNameLength} characters or less."));

        _displayName = displayName.Trim();
        return Task.FromResult(UserUpdateResult.Ok());
    }

    public Task<IReadOnlyList<ProviderInfo>> ListProvidersAsync(CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<ProviderInfo>>(_providers.ToList());

    public Task<UserUpdateResult> UnlinkProviderAsync(string connectionId, CancellationToken ct = default)
    {
        if (_providers.Count <= 1)
            return Task.FromResult(UserUpdateResult.Fail("You must keep at least one linked provider."));

        var provider = _providers.FirstOrDefault(p => p.ConnectionId == connectionId);
        if (provider is null)
            return Task.FromResult(UserUpdateResult.Fail("Provider not found."));

        _providers.Remove(provider);
        return Task.FromResult(UserUpdateResult.Ok());
    }

    public Task<UserUpdateResult> DeleteAccountAsync(CancellationToken ct = default)
    {
        _providers.Clear();
        _displayName = "Student";
        return Task.FromResult(UserUpdateResult.Ok());
    }
}
