using Mnemi.Application.Ports;
using Mnemi.Domain.Entities;

namespace Ui.Web.Services;

/// <summary>
/// Stub IAuthConnectionRepository for Phase 5 auth infrastructure verification.
/// </summary>
public class StubAuthConnectionRepository : IAuthConnectionRepository
{
    public Task<AuthConnection?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult<AuthConnection?>(null);

    public Task<AuthConnection?> GetByProviderUserIdAsync(OAuthProvider provider, string providerUserId, CancellationToken ct = default)
        => Task.FromResult<AuthConnection?>(null);

    public Task<IReadOnlyList<AuthConnection>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<AuthConnection>>(Array.Empty<AuthConnection>());

    public Task<AuthConnection> CreateAsync(AuthConnection connection, CancellationToken ct = default)
        => Task.FromResult(connection);

    public Task UpdateAsync(AuthConnection connection, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
        => Task.CompletedTask;
}
