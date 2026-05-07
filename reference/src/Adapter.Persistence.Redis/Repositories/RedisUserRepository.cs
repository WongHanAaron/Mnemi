using System.Text.Json;
using StackExchange.Redis;
using Mnemi.Domain.Entities;
using Mnemi.Application.Ports;

namespace Mnemi.Adapter.Persistence.Redis;

/// <summary>
/// Redis implementation of IUserRepository using JSON storage.
/// </summary>
public class RedisUserRepository : IUserRepository
{
    private readonly RedisConnectionProvider _redis;
    private static readonly string KeyPrefix = "user:";
    private static readonly string IndexKey = "users:index";
    private static readonly string EmailIndexKey = "users:email:";

    public RedisUserRepository(RedisConnectionProvider redis)
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var key = GetKey(id);
        var json = await _redis.Database.StringGetAsync(key);

        if (json.IsNullOrEmpty)
            return null;

        var entity = JsonSerializer.Deserialize<UserJsonEntity>(json!);
        return entity == null ? null : MapToDomain(entity);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var userId = await _redis.Database.StringGetAsync(GetEmailIndexKey(email));
        if (userId.IsNullOrEmpty)
            return null;

        if (Guid.TryParse(userId, out var id))
        {
            return await GetByIdAsync(id, ct);
        }

        return null;
    }

    public async Task<User?> GetByAuthConnectionAsync(OAuthProvider provider, string providerUserId, CancellationToken ct = default)
    {
        // This would require a lookup via auth connection index
        // For now, return null as this requires additional indexing
        return await Task.FromResult<User?>(null);
    }

    public async Task<User> CreateAsync(User user, CancellationToken ct = default)
    {
        var key = GetKey(user.Id);
        var entity = new UserJsonEntity
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            CreatedAt = user.CreatedAt
        };

        var json = JsonSerializer.Serialize(entity);
        await _redis.Database.StringSetAsync(key, json);
        await _redis.Database.SetAddAsync(IndexKey, user.Id.ToString());
        await _redis.Database.StringSetAsync(GetEmailIndexKey(user.Email), user.Id.ToString());

        return user;
    }

    public async Task UpdateAsync(User user, CancellationToken ct = default)
    {
        var key = GetKey(user.Id);

        var entity = new UserJsonEntity
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            CreatedAt = user.CreatedAt
        };

        var json = JsonSerializer.Serialize(entity);
        await _redis.Database.StringSetAsync(key, json);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var user = await GetByIdAsync(id, ct);
        if (user != null)
        {
            var key = GetKey(id);
            await _redis.Database.KeyDeleteAsync(key);
            await _redis.Database.SetRemoveAsync(IndexKey, id.ToString());
            await _redis.Database.KeyDeleteAsync(GetEmailIndexKey(user.Email));
        }
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
    {
        var userId = await _redis.Database.StringGetAsync(GetEmailIndexKey(email));
        return !userId.IsNullOrEmpty;
    }

    private static string GetKey(Guid id) => $"{KeyPrefix}{id}";
    private static string GetEmailIndexKey(string email) => $"{EmailIndexKey}{email.ToLowerInvariant()}";

    private static User MapToDomain(UserJsonEntity entity)
    {
        return User.Load(entity.Id, entity.Email, entity.DisplayName, entity.CreatedAt);
    }
}
