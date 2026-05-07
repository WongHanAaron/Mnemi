using System.Text.Json;
using StackExchange.Redis;
using Mnemi.Domain.Entities;
using Mnemi.Application.Ports;

namespace Mnemi.Adapter.Persistence.Redis;

/// <summary>
/// Redis implementation of IAuthConnectionRepository using JSON storage.
/// </summary>
public class RedisAuthConnectionRepository : IAuthConnectionRepository
{
    private readonly RedisConnectionProvider _redis;
    private static readonly string KeyPrefix = "auth:";
    private static readonly string UserIndexPrefix = "auth:user:";
    private static readonly string ProviderIndexPrefix = "auth:provider:";

    public RedisAuthConnectionRepository(RedisConnectionProvider redis)
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
    }

    public async Task<AuthConnection?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var key = GetKey(id);
        var json = await _redis.Database.StringGetAsync(key);

        if (json.IsNullOrEmpty)
            return null;

        var entity = JsonSerializer.Deserialize<AuthConnectionJsonEntity>(json!);
        return entity == null ? null : MapToDomain(entity);
    }

    public async Task<AuthConnection?> GetByProviderUserIdAsync(OAuthProvider provider, string providerUserId, CancellationToken ct = default)
    {
        var indexKey = GetProviderIndexKey(provider.ToString(), providerUserId);
        var connectionId = await _redis.Database.StringGetAsync(indexKey);

        if (connectionId.IsNullOrEmpty)
            return null;

        if (Guid.TryParse(connectionId, out var id))
        {
            return await GetByIdAsync(id, ct);
        }

        return null;
    }

    public async Task<IReadOnlyList<AuthConnection>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        var indexKey = GetUserIndexKey(userId);
        var connectionIds = await _redis.Database.SetMembersAsync(indexKey);
        var connections = new List<AuthConnection>();

        foreach (var id in connectionIds)
        {
            if (Guid.TryParse(id, out var connectionId))
            {
                var connection = await GetByIdAsync(connectionId, ct);
                if (connection != null && connection.IsValid)
                {
                    connections.Add(connection);
                }
            }
        }

        return connections.OrderBy(c => c.Provider.ToString()).ToList();
    }

    public async Task<AuthConnection> CreateAsync(AuthConnection connection, CancellationToken ct = default)
    {
        var key = GetKey(connection.Id);
        var entity = new AuthConnectionJsonEntity
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

        var json = JsonSerializer.Serialize(entity);
        await _redis.Database.StringSetAsync(key, json);

        // Update indexes
        await _redis.Database.SetAddAsync(GetUserIndexKey(connection.UserId), connection.Id.ToString());
        await _redis.Database.StringSetAsync(
            GetProviderIndexKey(connection.Provider.ToString(), connection.ProviderUserId),
            connection.Id.ToString());

        return connection;
    }

    public async Task UpdateAsync(AuthConnection connection, CancellationToken ct = default)
    {
        var key = GetKey(connection.Id);
        var entity = new AuthConnectionJsonEntity
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

        var json = JsonSerializer.Serialize(entity);
        await _redis.Database.StringSetAsync(key, json);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var connection = await GetByIdAsync(id, ct);
        if (connection != null)
        {
            var updatedConnection = AuthConnection.Load(
                connection.Id,
                connection.UserId,
                connection.Provider,
                connection.ProviderUserId,
                connection.EncryptedAccessToken,
                connection.EncryptedRefreshToken,
                connection.ExpiresAt,
                connection.Scopes,
                connection.CreatedAt,
                DateTime.UtcNow,
                false);

            await UpdateAsync(updatedConnection, ct);
        }
    }

    private static string GetKey(Guid id) => $"{KeyPrefix}{id}";
    private static string GetUserIndexKey(Guid userId) => $"{UserIndexPrefix}{userId}";
    private static string GetProviderIndexKey(string provider, string providerUserId) =>
        $"{ProviderIndexPrefix}{provider}:{providerUserId}";

    private static AuthConnection MapToDomain(AuthConnectionJsonEntity entity)
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
