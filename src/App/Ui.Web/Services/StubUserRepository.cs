using Mnemi.Application.Ports;
using Mnemi.Domain.Entities;

namespace Ui.Web.Services;

/// <summary>
/// Stub IUserRepository for Phase 5 auth infrastructure verification.
/// </summary>
public class StubUserRepository : IUserRepository
{
    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult<User?>(null);

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => Task.FromResult<User?>(null);

    public Task<User?> GetByAuthConnectionAsync(OAuthProvider provider, string providerUserId, CancellationToken ct = default)
        => Task.FromResult<User?>(null);

    public Task<User> CreateAsync(User user, CancellationToken ct = default)
        => Task.FromResult(user);

    public Task UpdateAsync(User user, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
        => Task.FromResult(false);
}
