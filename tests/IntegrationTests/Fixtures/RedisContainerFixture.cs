using Testcontainers.Redis;
using Xunit;

namespace Mnemi.IntegrationTests.Fixtures;

/// <summary>
/// Xunit fixture that manages a Redis Docker container for integration tests.
/// Uses Testcontainers to spin up a Redis instance in Docker (WSL2).
/// </summary>
public class RedisContainerFixture : IAsyncLifetime
{
    private readonly RedisContainer _redisContainer;

    public string ConnectionString { get; private set; } = string.Empty;

    public RedisContainerFixture()
    {
        _redisContainer = new RedisBuilder()
            .WithImage("redis:7-alpine")
            .WithName($"mnemi-redis-test-{Guid.NewGuid():N}")
            .WithPortBinding(6379, true) // Random host port
            .WithAutoRemove(true)
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _redisContainer.StartAsync();
        ConnectionString = _redisContainer.GetConnectionString();
    }

    public async Task DisposeAsync()
    {
        await _redisContainer.StopAsync();
        await _redisContainer.DisposeAsync();
    }
}

/// <summary>
/// Collection definition for tests that use the Redis container fixture.
/// </summary>
[CollectionDefinition("Redis Integration Tests")]
public class RedisCollection : ICollectionFixture<RedisContainerFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}
