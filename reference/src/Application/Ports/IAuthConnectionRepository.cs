using Mnemi.Domain.Entities;

namespace Mnemi.Application.Ports;

/// <summary>
/// Repository interface for AuthConnection operations.
/// </summary>
public interface IAuthConnectionRepository
{
    /// <summary>
    /// Gets an auth connection by its unique identifier.
    /// </summary>
    Task<AuthConnection?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets an auth connection by provider and provider user ID.
    /// </summary>
    Task<AuthConnection?> GetByProviderUserIdAsync(OAuthProvider provider, string providerUserId, CancellationToken ct = default);

    /// <summary>
    /// Gets all auth connections for a user.
    /// </summary>
    Task<IReadOnlyList<AuthConnection>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Creates a new auth connection.
    /// </summary>
    Task<AuthConnection> CreateAsync(AuthConnection connection, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing auth connection.
    /// </summary>
    Task UpdateAsync(AuthConnection connection, CancellationToken ct = default);

    /// <summary>
    /// Deletes an auth connection by its ID.
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
