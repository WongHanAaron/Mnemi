using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Mnemi.Application.Ports;

namespace Mnemi.Adapter.Persistence.Sqlite;

/// <summary>
/// Extension methods for registering SQLite persistence services with DI.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers SQLite persistence with a file-based database.
    /// </summary>
    /// <param name="databasePath">Path to the SQLite database file.</param>
    public static IServiceCollection AddSqlitePersistence(
        this IServiceCollection services,
        string databasePath)
    {
        services.AddDbContext<MnemiDbContext>(options =>
            options.UseSqlite($"Data Source={databasePath}"));

        RegisterRepositories(services);
        return services;
    }

    /// <summary>
    /// Registers SQLite persistence with an in-memory database (for testing).
    /// </summary>
    public static IServiceCollection AddSqliteInMemoryPersistence(
        this IServiceCollection services)
    {
        services.AddDbContext<MnemiDbContext>(options =>
            options.UseSqlite("DataSource=:memory:"));

        RegisterRepositories(services);
        return services;
    }

    private static void RegisterRepositories(IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAuthConnectionRepository, AuthConnectionRepository>();
        services.AddScoped<IDocumentSourceRepository, DocumentSourceRepository>();
    }
}
