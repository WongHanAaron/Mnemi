using StackExchange.Redis;

namespace Mnemi.Adapter.Persistence.Redis;

/// <summary>
/// Provides Redis connection management and database access.
/// </summary>
public class RedisConnectionProvider : IDisposable
{
    private readonly IConnectionMultiplexer _connection;
    private readonly IDatabase _database;
    private bool _disposed;

    /// <summary>
    /// Creates a new Redis connection provider.
    /// </summary>
    public RedisConnectionProvider(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));

        _connection = ConnectionMultiplexer.Connect(connectionString);
        _database = _connection.GetDatabase();
    }

    /// <summary>
    /// Creates a new Redis connection provider with an existing connection.
    /// </summary>
    public RedisConnectionProvider(IConnectionMultiplexer connection)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _database = _connection.GetDatabase();
    }

    /// <summary>
    /// Gets the Redis database instance.
    /// </summary>
    public IDatabase Database => _database;

    /// <summary>
    /// Gets the underlying connection multiplexer.
    /// </summary>
    public IConnectionMultiplexer Connection => _connection;

    /// <summary>
    /// Checks if the Redis connection is available.
    /// </summary>
    public bool IsConnected => _connection.IsConnected;

    public void Dispose()
    {
        if (!_disposed)
        {
            _connection.Dispose();
            _disposed = true;
        }
    }
}
