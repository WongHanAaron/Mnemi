namespace Mnemi.Application.Ports;

/// <summary>
/// Service for user account management operations used by the UI layer.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Updates the display name for the current user.
    /// Returns a result indicating success or failure with an error message.
    /// </summary>
    Task<UserUpdateResult> UpdateDisplayNameAsync(string displayName, CancellationToken ct = default);

    /// <summary>
    /// Lists all linked OAuth providers for the current user.
    /// </summary>
    Task<IReadOnlyList<ProviderInfo>> ListProvidersAsync(CancellationToken ct = default);

    /// <summary>
    /// Unlinks an OAuth provider connection. The user must have at least one
    /// remaining provider after this operation.
    /// </summary>
    Task<UserUpdateResult> UnlinkProviderAsync(string connectionId, CancellationToken ct = default);

    /// <summary>
    /// Permanently deletes the user's account and all associated data.
    /// Revokes OAuth tokens, removes providers, sources, and the user record.
    /// </summary>
    Task<UserUpdateResult> DeleteAccountAsync(CancellationToken ct = default);
}

/// <summary>
/// Result of a user profile update operation.
/// </summary>
public sealed class UserUpdateResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }

    public static UserUpdateResult Ok() => new() { Success = true };
    public static UserUpdateResult Fail(string error) => new() { Success = false, ErrorMessage = error };
}

/// <summary>
/// Information about a linked OAuth provider connection.
/// </summary>
public sealed record ProviderInfo(
    string ConnectionId,
    string ProviderName,    // "Google" or "GitHub"
    string ConnectedAs,     // email or username at the provider
    bool IsValid,
    DateTime ConnectedAt
);
