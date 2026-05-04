using Mnemi.Adapter.Persistence.Redis;
using Mnemi.Domain.Entities;
using Testcontainers.Redis;
using Xunit;

namespace Mnemi.IntegrationTests;

/// <summary>
/// Integration tests for Redis persistence adapter using Testcontainers.
/// Spins up a Redis container in Docker and tests CRUD operations.
/// </summary>
public class RedisIntegrationTests : IAsyncLifetime
{
    private RedisContainer? _redisContainer;
    private RedisConnectionProvider? _connectionProvider;
    private RedisUserRepository? _userRepository;
    private RedisAuthConnectionRepository? _authConnectionRepository;
    private RedisDocumentSourceRepository? _documentSourceRepository;

    public async Task InitializeAsync()
    {
        // Spin up Redis container
        _redisContainer = new RedisBuilder()
            .WithImage("redis:7-alpine")
            .WithName($"mnemi-redis-test-{Guid.NewGuid():N}")
            .Build();

        await _redisContainer.StartAsync();

        // Create connection provider
        var connectionString = _redisContainer.GetConnectionString();
        _connectionProvider = new RedisConnectionProvider(connectionString);

        // Create repositories
        _userRepository = new RedisUserRepository(_connectionProvider);
        _authConnectionRepository = new RedisAuthConnectionRepository(_connectionProvider);
        _documentSourceRepository = new RedisDocumentSourceRepository(_connectionProvider);
    }

    public async Task DisposeAsync()
    {
        if (_connectionProvider != null)
        {
            _connectionProvider.Dispose();
        }

        if (_redisContainer != null)
        {
            await _redisContainer.StopAsync();
            await _redisContainer.DisposeAsync();
        }
    }

    [Fact]
    public async Task CanConnectToRedis()
    {
        Assert.NotNull(_connectionProvider);
        Assert.True(_connectionProvider.IsConnected);
    }

    [Fact]
    public async Task CanCreateAndRetrieveUser()
    {
        // Arrange
        var user = User.Create("test@example.com", "Test User");

        // Act
        var created = await _userRepository!.CreateAsync(user);
        var retrieved = await _userRepository.GetByIdAsync(user.Id);

        // Assert
        Assert.NotNull(created);
        Assert.NotNull(retrieved);
        Assert.Equal(user.Id, retrieved.Id);
        Assert.Equal(user.Email, retrieved.Email);
        Assert.Equal(user.DisplayName, retrieved.DisplayName);
    }

    [Fact]
    public async Task CanRetrieveUserByEmail()
    {
        // Arrange
        var user = User.Create("email-test@example.com", "Email Test User");
        await _userRepository!.CreateAsync(user);

        // Act
        var retrieved = await _userRepository.GetByEmailAsync("email-test@example.com");

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(user.Id, retrieved.Id);
    }

    [Fact]
    public async Task CanUpdateUser()
    {
        // Arrange
        var user = User.Create("update-test@example.com", "Original Name");
        await _userRepository!.CreateAsync(user);

        // Act
        user.UpdateDisplayName("Updated Name");
        await _userRepository.UpdateAsync(user);
        var retrieved = await _userRepository.GetByIdAsync(user.Id);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal("Updated Name", retrieved.DisplayName);
    }

    [Fact]
    public async Task CanDeleteUser()
    {
        // Arrange
        var user = User.Create("delete-test@example.com", "Delete Test User");
        await _userRepository!.CreateAsync(user);

        // Act
        await _userRepository.DeleteAsync(user.Id);
        var retrieved = await _userRepository.GetByIdAsync(user.Id);

        // Assert
        Assert.Null(retrieved);
    }

    [Fact]
    public async Task CanCreateAndRetrieveAuthConnection()
    {
        // Arrange
        var user = User.Create("auth-test@example.com", "Auth Test User");
        await _userRepository!.CreateAsync(user);

        var connection = AuthConnection.Create(
            user.Id,
            OAuthProvider.Google,
            "google-user-123",
            "encrypted-access-token",
            "encrypted-refresh-token",
            DateTime.UtcNow.AddHours(1),
            "openid email profile");

        // Act
        var created = await _authConnectionRepository!.CreateAsync(connection);
        var retrieved = await _authConnectionRepository.GetByIdAsync(connection.Id);

        // Assert
        Assert.NotNull(created);
        Assert.NotNull(retrieved);
        Assert.Equal(connection.Id, retrieved.Id);
        Assert.Equal(connection.Provider, retrieved.Provider);
        Assert.Equal(connection.ProviderUserId, retrieved.ProviderUserId);
    }

    [Fact]
    public async Task CanRetrieveAuthConnectionByProviderUserId()
    {
        // Arrange
        var user = User.Create("provider-test@example.com", "Provider Test User");
        await _userRepository!.CreateAsync(user);

        var connection = AuthConnection.Create(
            user.Id,
            OAuthProvider.GitHub,
            "github-user-456",
            "encrypted-token",
            null,
            null,
            "read:user repo");

        await _authConnectionRepository!.CreateAsync(connection);

        // Act
        var retrieved = await _authConnectionRepository.GetByProviderUserIdAsync(
            OAuthProvider.GitHub, "github-user-456");

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(connection.Id, retrieved.Id);
    }

    [Fact]
    public async Task CanCreateAndRetrieveDocumentSource()
    {
        // Arrange
        var user = User.Create("source-test@example.com", "Source Test User");
        await _userRepository!.CreateAsync(user);

        var connection = AuthConnection.Create(
            user.Id,
            OAuthProvider.Google,
            "google-source-123",
            "encrypted-token",
            null,
            null,
            "drive.readonly");
        await _authConnectionRepository!.CreateAsync(connection);

        var source = DocumentSourceConfig.CreateGoogleDrive(
            user.Id,
            connection.Id,
            "My Flashcards",
            "folder-id-123");

        // Act
        var created = await _documentSourceRepository!.CreateAsync(source);
        var retrieved = await _documentSourceRepository.GetByIdAsync(source.Id);

        // Assert
        Assert.NotNull(created);
        Assert.NotNull(retrieved);
        Assert.Equal(source.Id, retrieved.Id);
        Assert.Equal(source.DisplayName, retrieved.DisplayName);
        Assert.Equal(source.Provider, retrieved.Provider);
    }

    [Fact]
    public async Task CanRetrieveDocumentSourcesByUserId()
    {
        // Arrange
        var user = User.Create("sources-test@example.com", "Sources Test User");
        await _userRepository!.CreateAsync(user);

        var connection = AuthConnection.Create(
            user.Id,
            OAuthProvider.Google,
            "google-multi-123",
            "encrypted-token",
            null,
            null,
            "drive.readonly");
        await _authConnectionRepository!.CreateAsync(connection);

        var source1 = DocumentSourceConfig.CreateGoogleDrive(
            user.Id, connection.Id, "Source 1", "folder-1");
        var source2 = DocumentSourceConfig.CreateGoogleDrive(
            user.Id, connection.Id, "Source 2", "folder-2");

        await _documentSourceRepository!.CreateAsync(source1);
        await _documentSourceRepository.CreateAsync(source2);

        // Act
        var sources = await _documentSourceRepository.GetByUserIdAsync(user.Id);

        // Assert
        Assert.Equal(2, sources.Count);
    }

    [Fact]
    public async Task DuplicateProviderConfigDetectionWorks()
    {
        // Arrange
        var user = User.Create("duplicate-test@example.com", "Duplicate Test User");
        await _userRepository!.CreateAsync(user);

        var connection = AuthConnection.Create(
            user.Id,
            OAuthProvider.Google,
            "google-dup-123",
            "encrypted-token",
            null,
            null,
            "drive.readonly");
        await _authConnectionRepository!.CreateAsync(connection);

        var source = DocumentSourceConfig.CreateGoogleDrive(
            user.Id, connection.Id, "Original", "duplicate-folder-id");
        await _documentSourceRepository!.CreateAsync(source);

        // Act
        var exists = await _documentSourceRepository.ExistsAsync(
            user.Id, DocumentSourceProvider.GoogleDrive, "duplicate-folder-id");

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task FullUserWithConnectionsAndSources()
    {
        // Arrange - Create complete user scenario
        var user = User.Create("full-test@example.com", "Full Test User");

        var googleConnection = AuthConnection.Create(
            user.Id,
            OAuthProvider.Google,
            "google-full-123",
            "google-encrypted-token",
            "google-refresh-token",
            DateTime.UtcNow.AddHours(1),
            "openid email profile drive.readonly");

        var githubConnection = AuthConnection.Create(
            user.Id,
            OAuthProvider.GitHub,
            "github-full-456",
            "github-encrypted-token",
            null,
            null,
            "read:user repo");

        user.AddAuthConnection(googleConnection);
        user.AddAuthConnection(githubConnection);

        var googleSource = DocumentSourceConfig.CreateGoogleDrive(
            user.Id, googleConnection.Id, "Google Drive Flashcards", "drive-folder-123");

        var githubSource = DocumentSourceConfig.CreateGitHub(
            user.Id, githubConnection.Id, "GitHub Study Notes",
            "myusername", "study-repo", "/flashcards", "main");

        user.AddDocumentSource(googleSource);
        user.AddDocumentSource(githubSource);

        // Act
        await _userRepository!.CreateAsync(user);
        await _authConnectionRepository!.CreateAsync(googleConnection);
        await _authConnectionRepository.CreateAsync(githubConnection);
        await _documentSourceRepository!.CreateAsync(googleSource);
        await _documentSourceRepository.CreateAsync(githubSource);

        // Assert - Verify everything was stored
        var retrievedUser = await _userRepository.GetByIdAsync(user.Id);
        var connections = await _authConnectionRepository.GetByUserIdAsync(user.Id);
        var sources = await _documentSourceRepository.GetByUserIdAsync(user.Id);

        Assert.NotNull(retrievedUser);
        Assert.Equal(2, connections.Count);
        Assert.Equal(2, sources.Count);
    }
}
