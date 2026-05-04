using Microsoft.EntityFrameworkCore;
using Mnemi.Domain.Entities;
using Mnemi.Application.Ports;

namespace Mnemi.Adapter.Persistence.Sqlite;

/// <summary>
/// SQLite implementation of IAuthConnectionRepository using Entity Framework Core.
/// </summary>
public class AuthConnectionRepository : IAuthConnectionRepository
{
    private readonly MnemiDbContext _context;

    public AuthConnectionRepository(MnemiDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<AuthConnection?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.AuthConnections
            .AsNoTracking()
            .FirstOrDefaultAsync(ac => ac.Id == id, ct);

        return entity == null ? null : MapToDomain(entity);
    }

    public async Task<AuthConnection?> GetByProviderUserIdAsync(OAuthProvider provider, string providerUserId, CancellationToken ct = default)
    {
        var entity = await _context.AuthConnections
            .AsNoTracking()
            .FirstOrDefaultAsync(ac =>
                ac.Provider == provider.ToString() &&
                ac.ProviderUserId == providerUserId,
                ct);

        return entity == null ? null : MapToDomain(entity);
    }

    public async Task<IReadOnlyList<AuthConnection>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        var entities = await _context.AuthConnections
            .AsNoTracking()
            .Where(ac => ac.UserId == userId && ac.IsValid)
            .OrderBy(ac => ac.Provider)
            .ToListAsync(ct);

        return entities.Select(MapToDomain).ToList();
    }

    public async Task<AuthConnection> CreateAsync(AuthConnection connection, CancellationToken ct = default)
    {
        var entity = new AuthConnectionEntity
        {
            Id = connection.Id,
            UserId = connection.UserId,
            Provider = connection.Provider.ToString(),
            ProviderUserId = connection.ProviderUserId,
            EncryptedAccessToken = connection.EncryptedAccessToken,
            EncryptedRefreshToken = connection.EncryptedRefreshToken,
            ExpiresAt = connection.ExpiresAt,
            Scopes = connection.Scopes,
            CreatedAt = connection.CreatedAt,
            UpdatedAt = connection.UpdatedAt,
            IsValid = connection.IsValid
        };

        _context.AuthConnections.Add(entity);
        await _context.SaveChangesAsync(ct);
        return connection;
    }

    public async Task UpdateAsync(AuthConnection connection, CancellationToken ct = default)
    {
        var entity = await _context.AuthConnections.FindAsync(new object[] { connection.Id }, ct);

        if (entity == null)
        {
            throw new InvalidOperationException($"AuthConnection with ID {connection.Id} not found");
        }

        entity.EncryptedAccessToken = connection.EncryptedAccessToken;
        entity.EncryptedRefreshToken = connection.EncryptedRefreshToken;
        entity.ExpiresAt = connection.ExpiresAt;
        entity.Scopes = connection.Scopes;
        entity.UpdatedAt = connection.UpdatedAt;
        entity.IsValid = connection.IsValid;

        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.AuthConnections.FindAsync(new object[] { id }, ct);
        if (entity != null)
        {
            entity.IsValid = false;
            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
        }
    }

    private static AuthConnection MapToDomain(AuthConnectionEntity entity)
    {
        if (!Enum.TryParse<OAuthProvider>(entity.Provider, out var provider))
        {
            throw new InvalidOperationException($"Unknown provider: {entity.Provider}");
        }

        return AuthConnection.Load(
            entity.Id,
            entity.UserId,
            provider,
            entity.ProviderUserId,
            entity.EncryptedAccessToken,
            entity.EncryptedRefreshToken,
            entity.ExpiresAt,
            entity.Scopes,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.IsValid);
    }
}
