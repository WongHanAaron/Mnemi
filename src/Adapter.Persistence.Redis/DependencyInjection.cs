using Microsoft.Extensions.DependencyInjection;
using Mnemi.Application.Ports;

namespace Mnemi.Adapter.Persistence.Redis;

/// <summary>
/// Extension methods for registering Redis persistence services with DI.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers Redis persistence with a connection string.
    /// </summary>
    /// <param name="connectionString">Redis connection string (e.g., "localhost:6379").</param>
    public static IServiceCollection AddRedisPersistence(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddSingleton(new RedisConnectionProvider(connectionString));
        RegisterRepositories(services);
        return services;
    }

    private static void RegisterRepositories(IServiceCollection services)
    {
        services.AddScoped<IUserRepository>(sp =>
        {
            var redis = sp.GetRequiredService<RedisConnectionProvider>();
            return new RedisUserRepository(redis);
        });

        services.AddScoped<IAuthConnectionRepository>(sp =>
        {
            var redis = sp.GetRequiredService<RedisConnectionProvider>();
            return new RedisAuthConnectionRepository(redis);
        });

        services.AddScoped<IDocumentSourceRepository>(sp =>
        {
            var redis = sp.GetRequiredService<RedisConnectionProvider>();
            return new RedisDocumentSourceRepository(redis);
        });
    }
}
