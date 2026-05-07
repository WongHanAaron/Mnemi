using System.Text.Json;
using StackExchange.Redis;
using Mnemi.Domain.Entities;
using Mnemi.Application.Ports;

namespace Mnemi.Adapter.Persistence.Redis;

/// <summary>
/// Redis implementation of IDocumentSourceRepository using JSON storage.
/// </summary>
public class RedisDocumentSourceRepository : IDocumentSourceRepository
{
    private readonly RedisConnectionProvider _redis;
    private static readonly string KeyPrefix = "docsource:";
    private static readonly string UserIndexPrefix = "docsource:user:";
    private static readonly string AuthConnectionIndexPrefix = "docsource:auth:";

    public RedisDocumentSourceRepository(RedisConnectionProvider redis)
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
    }

    public async Task<DocumentSourceConfig?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var key = GetKey(id);
        var json = await _redis.Database.StringGetAsync(key);

        if (json.IsNullOrEmpty)
            return null;

        var entity = JsonSerializer.Deserialize<DocumentSourceJsonEntity>(json!);
        return entity == null ? null : MapToDomain(entity);
    }

    public async Task<IReadOnlyList<DocumentSourceConfig>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        var indexKey = GetUserIndexKey(userId);
        var configIds = await _redis.Database.SetMembersAsync(indexKey);
        var configs = new List<DocumentSourceConfig>();

        foreach (var id in configIds)
        {
            if (Guid.TryParse(id, out var configId))
            {
                var config = await GetByIdAsync(configId, ct);
                if (config != null)
                {
                    configs.Add(config);
                }
            }
        }

        return configs.OrderBy(c => c.DisplayName).ToList();
    }

    public async Task<IReadOnlyList<DocumentSourceConfig>> GetByAuthConnectionIdAsync(Guid authConnectionId, CancellationToken ct = default)
    {
        var indexKey = GetAuthConnectionIndexKey(authConnectionId);
        var configIds = await _redis.Database.SetMembersAsync(indexKey);
        var configs = new List<DocumentSourceConfig>();

        foreach (var id in configIds)
        {
            if (Guid.TryParse(id, out var configId))
            {
                var config = await GetByIdAsync(configId, ct);
                if (config != null)
                {
                    configs.Add(config);
                }
            }
        }

        return configs.OrderBy(c => c.DisplayName).ToList();
    }

    public async Task<bool> ExistsAsync(Guid userId, DocumentSourceProvider provider, string providerConfigKey, CancellationToken ct = default)
    {
        // Check all document sources for this user
        var configs = await GetByUserIdAsync(userId, ct);
        return configs.Any(c => c.Provider == provider);
    }

    public async Task<DocumentSourceConfig> CreateAsync(DocumentSourceConfig source, CancellationToken ct = default)
    {
        var key = GetKey(source.Id);
        var entity = new DocumentSourceJsonEntity
        {
            Id = source.Id,
            UserId = source.UserId,
            AuthConnectionId = source.AuthConnectionId,
            Provider = source.Provider.ToString(),
            DisplayName = source.DisplayName,
            ProviderConfigJson = source.ProviderConfigJson,
            IsAccessible = source.IsAccessible,
            LastErrorMessage = source.LastErrorMessage,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt
        };

        var json = JsonSerializer.Serialize(entity);
        await _redis.Database.StringSetAsync(key, json);
        await _redis.Database.SetAddAsync(GetUserIndexKey(source.UserId), source.Id.ToString());
        await _redis.Database.SetAddAsync(GetAuthConnectionIndexKey(source.AuthConnectionId), source.Id.ToString());

        return source;
    }

    public async Task UpdateAsync(DocumentSourceConfig source, CancellationToken ct = default)
    {
        var key = GetKey(source.Id);
        var entity = new DocumentSourceJsonEntity
        {
            Id = source.Id,
            UserId = source.UserId,
            AuthConnectionId = source.AuthConnectionId,
            Provider = source.Provider.ToString(),
            DisplayName = source.DisplayName,
            ProviderConfigJson = source.ProviderConfigJson,
            IsAccessible = source.IsAccessible,
            LastErrorMessage = source.LastErrorMessage,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt
        };

        var json = JsonSerializer.Serialize(entity);
        await _redis.Database.StringSetAsync(key, json);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var config = await GetByIdAsync(id, ct);
        if (config != null)
        {
            var key = GetKey(id);
            await _redis.Database.KeyDeleteAsync(key);
            await _redis.Database.SetRemoveAsync(GetUserIndexKey(config.UserId), id.ToString());
            await _redis.Database.SetRemoveAsync(GetAuthConnectionIndexKey(config.AuthConnectionId), id.ToString());
        }
    }

    private static string GetKey(Guid id) => $"{KeyPrefix}{id}";
    private static string GetUserIndexKey(Guid userId) => $"{UserIndexPrefix}{userId}";
    private static string GetAuthConnectionIndexKey(Guid authConnectionId) => $"{AuthConnectionIndexPrefix}{authConnectionId}";

    private static DocumentSourceConfig MapToDomain(DocumentSourceJsonEntity entity)
    {
        if (!Enum.TryParse<DocumentSourceProvider>(entity.Provider, out var provider))
        {
            throw new InvalidOperationException($"Unknown provider: {entity.Provider}");
        }

        return DocumentSourceConfig.Load(
            entity.Id,
            entity.UserId,
            entity.AuthConnectionId,
            provider,
            entity.DisplayName,
            entity.ProviderConfigJson,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.IsAccessible,
            entity.LastErrorMessage);
    }
}
