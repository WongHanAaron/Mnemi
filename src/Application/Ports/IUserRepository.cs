using Mnemi.Domain.Entities;

namespace Mnemi.Application.Ports;

/// <summary>
/// Repository interface for User aggregate operations.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Gets a user by their unique identifier.
    /// </summary>
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets a user by their email address.
    /// </summary>
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);

    /// <summary>
    /// Gets a user by their OAuth provider and provider user ID.
    /// </summary>
    Task<User?> GetByAuthConnectionAsync(OAuthProvider provider, string providerUserId, CancellationToken ct = default);

    /// <summary>
    /// Creates a new user.
    /// </summary>
    Task<User> CreateAsync(User user, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing user.
    /// </summary>
    Task UpdateAsync(User user, CancellationToken ct = default);

    /// <summary>
    /// Deletes a user by their ID.
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Checks if a user with the specified email exists.
    /// </summary>
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
}
