using Mnemi.Adapter.Persistence.Redis;
using Mnemi.Domain.Entities;
using Mnemi.IntegrationTests.Fixtures;
using Xunit;

namespace Mnemi.IntegrationTests.Persistence;

[Collection("Redis Integration Tests")]
public class RedisPersistenceTests : IDisposable
{
    private readonly RedisConnectionProvider _connectionProvider;
    private readonly RedisUserRepository _userRepository;
    private readonly RedisAuthConnectionRepository _authConnectionRepository;
    private readonly RedisDocumentSourceRepository _documentSourceRepository;

    public RedisPersistenceTests(RedisContainerFixture fixture)
    {
        _connectionProvider = new RedisConnectionProvider(fixture.ConnectionString);
        _userRepository = new RedisUserRepository(_connectionProvider);
        _authConnectionRepository = new RedisAuthConnectionRepository(_connectionProvider);
        _documentSourceRepository = new RedisDocumentSourceRepository(_connectionProvider);
    }

    public void Dispose()
    {
        _connectionProvider?.Dispose();
    }

    [Fact]
    public async Task Connection_ShouldBeEstablished()
    {
        // Act & Assert
        Assert.True(_connectionProvider.IsConnected);
    }

    [Fact]
    public async Task UserRepository_CreateAndRead_ShouldWork()
    {
        // Arrange
        var user = User.Create("test@example.com", "Test User");

        // Act
        var created = await _userRepository.CreateAsync(user);
        var retrieved = await _userRepository.GetByIdAsync(created.Id);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(user.Email, retrieved.Email);
        Assert.Equal(user.DisplayName, retrieved.DisplayName);
    }

    [Fact]
    public async Task UserRepository_GetByEmail_ShouldReturnUser()
    {
        // Arrange
        var user = User.Create("email-test@example.com", "Email Test User");
        await _userRepository.CreateAsync(user);

        // Act
        var retrieved = await _userRepository.GetByEmailAsync("email-test@example.com");

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(user.Email, retrieved.Email);
    }

    [Fact]
    public async Task UserRepository_ExistsByEmail_ShouldReturnTrue()
    {
        // Arrange
        var user = User.Create("exists-test@example.com", "Exists Test User");
        await _userRepository.CreateAsync(user);

        // Act
        var exists = await _userRepository.ExistsByEmailAsync("exists-test@example.com");

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task UserRepository_Update_ShouldModifyUser()
    {
        // Arrange
        var user = User.Create("update-test@example.com", "Original Name");
        var created = await _userRepository.CreateAsync(user);

        // Act
        created.UpdateDisplayName("Updated Name");
        await _userRepository.UpdateAsync(created);
        var retrieved = await _userRepository.GetByIdAsync(created.Id);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal("Updated Name", retrieved.DisplayName);
    }

    [Fact]
    public async Task UserRepository_Delete_ShouldRemoveUser()
    {
        // Arrange
        var user = User.Create("delete-test@example.com", "Delete Test User");
        var created = await _userRepository.CreateAsync(user);

        // Act
        await _userRepository.DeleteAsync(created.Id);
        var retrieved = await _userRepository.GetByIdAsync(created.Id);

        // Assert
        Assert.Null(retrieved);
    }

    [Fact]
    public async Task AuthConnectionRepository_CreateAndRead_ShouldWork()
    {
        // Arrange
        var user = User.Create("auth-test@example.com", "Auth Test User");
        var createdUser = await _userRepository.CreateAsync(user);

        var connection = AuthConnection.Create(
            createdUser.Id,
            OAuthProvider.Google,
            "google-user-123",
            "encrypted-access-token",
            "encrypted-refresh-token",
            DateTime.UtcNow.AddHours(1),
            "openid email profile");

        // Act
        var created = await _authConnectionRepository.CreateAsync(connection);
        var retrieved = await _authConnectionRepository.GetByIdAsync(created.Id);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(connection.Provider, retrieved.Provider);
        Assert.Equal(connection.ProviderUserId, retrieved.ProviderUserId);
        Assert.Equal(connection.Scopes, retrieved.Scopes);
    }

    [Fact]
    public async Task AuthConnectionRepository_GetByProviderUserId_ShouldReturnConnection()
    {
        // Arrange
        var user = User.Create("provider-test@example.com", "Provider Test User");
        var createdUser = await _userRepository.CreateAsync(user);

        var connection = AuthConnection.Create(
            createdUser.Id,
            OAuthProvider.GitHub,
            "github-user-456",
            "encrypted-token",
            null,
            null,
            "read:user repo");

        await _authConnectionRepository.CreateAsync(connection);

        // Act
        var retrieved = await _authConnectionRepository.GetByProviderUserIdAsync(
            OAuthProvider.GitHub, "github-user-456");

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(OAuthProvider.GitHub, retrieved.Provider);
        Assert.Equal("github-user-456", retrieved.ProviderUserId);
    }

    [Fact]
    public async Task AuthConnectionRepository_GetByUserId_ShouldReturnAllConnections()
    {
        // Arrange
        var user = User.Create("multi-auth-test@example.com", "Multi Auth Test User");
        var createdUser = await _userRepository.CreateAsync(user);

        var googleConnection = AuthConnection.Create(
            createdUser.Id,
            OAuthProvider.Google,
            "google-user-789",
            "encrypted-google-token",
            null,
            null,
            "openid email");

        var githubConnection = AuthConnection.Create(
            createdUser.Id,
            OAuthProvider.GitHub,
            "github-user-789",
            "encrypted-github-token",
            null,
            null,
            "read:user");

        await _authConnectionRepository.CreateAsync(googleConnection);
        await _authConnectionRepository.CreateAsync(githubConnection);

        // Act
        var connections = await _authConnectionRepository.GetByUserIdAsync(createdUser.Id);

        // Assert
        Assert.Equal(2, connections.Count);
        Assert.Contains(connections, c => c.Provider == OAuthProvider.Google);
        Assert.Contains(connections, c => c.Provider == OAuthProvider.GitHub);
    }

    [Fact]
    public async Task DocumentSourceRepository_CreateAndRead_ShouldWork()
    {
        // Arrange
        var user = User.Create("source-test@example.com", "Source Test User");
        var createdUser = await _userRepository.CreateAsync(user);

        var connection = AuthConnection.Create(
            createdUser.Id,
            OAuthProvider.Google,
            "google-source-user",
            "encrypted-token",
            null,
            null,
            "drive.readonly");
        var createdConnection = await _authConnectionRepository.CreateAsync(connection);

        var source = DocumentSourceConfig.CreateGoogleDrive(
            createdUser.Id,
            createdConnection.Id,
            "My Flashcards",
            "1BxiMVs0XRA5nFMdKvBdBZjgmUUqptlbs74OgvE2upms");

        // Act
        var created = await _documentSourceRepository.CreateAsync(source);
        var retrieved = await _documentSourceRepository.GetByIdAsync(created.Id);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(source.DisplayName, retrieved.DisplayName);
        Assert.Equal(source.Provider, retrieved.Provider);
    }

    [Fact]
    public async Task DocumentSourceRepository_GetByUserId_ShouldReturnAllSources()
    {
        // Arrange
        var user = User.Create("multi-source-test@example.com", "Multi Source Test User");
        var createdUser = await _userRepository.CreateAsync(user);

        var connection = AuthConnection.Create(
            createdUser.Id,
            OAuthProvider.Google,
            "google-multi-source-user",
            "encrypted-token",
            null,
            null,
            "drive.readonly");
        var createdConnection = await _authConnectionRepository.CreateAsync(connection);

        var source1 = DocumentSourceConfig.CreateGoogleDrive(
            createdUser.Id,
            createdConnection.Id,
            "Flashcards 1",
            "folder-id-1");

        var source2 = DocumentSourceConfig.CreateGoogleDrive(
            createdUser.Id,
            createdConnection.Id,
            "Flashcards 2",
            "folder-id-2");

        await _documentSourceRepository.CreateAsync(source1);
        await _documentSourceRepository.CreateAsync(source2);

        // Act
        var sources = await _documentSourceRepository.GetByUserIdAsync(createdUser.Id);

        // Assert
        Assert.Equal(2, sources.Count);
    }

    [Fact]
    public async Task DocumentSourceRepository_Exists_ShouldDetectDuplicate()
    {
        // Arrange
        var user = User.Create("duplicate-test@example.com", "Duplicate Test User");
        var createdUser = await _userRepository.CreateAsync(user);

        var connection = AuthConnection.Create(
            createdUser.Id,
            OAuthProvider.Google,
            "google-dup-user",
            "encrypted-token",
            null,
            null,
            "drive.readonly");
        var createdConnection = await _authConnectionRepository.CreateAsync(connection);

        var source = DocumentSourceConfig.CreateGoogleDrive(
            createdUser.Id,
            createdConnection.Id,
            "My Flashcards",
            "duplicate-folder-id");

        await _documentSourceRepository.CreateAsync(source);

        // Act
        var exists = await _documentSourceRepository.ExistsAsync(
            createdUser.Id,
            DocumentSourceProvider.GoogleDrive,
            "duplicate-folder-id");

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task DocumentSourceRepository_Update_ShouldModifySource()
    {
        // Arrange
        var user = User.Create("update-source-test@example.com", "Update Source Test User");
        var createdUser = await _userRepository.CreateAsync(user);

        var connection = AuthConnection.Create(
            createdUser.Id,
            OAuthProvider.Google,
            "google-update-user",
            "encrypted-token",
            null,
            null,
            "drive.readonly");
        var createdConnection = await _authConnectionRepository.CreateAsync(connection);

        var source = DocumentSourceConfig.CreateGoogleDrive(
            createdUser.Id,
            createdConnection.Id,
            "Original Name",
            "update-folder-id");

        var created = await _documentSourceRepository.CreateAsync(source);

        // Act
        created.UpdateDisplayName("Updated Source Name");
        await _documentSourceRepository.UpdateAsync(created);
        var retrieved = await _documentSourceRepository.GetByIdAsync(created.Id);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal("Updated Source Name", retrieved.DisplayName);
    }

    [Fact]
    public async Task DocumentSourceRepository_Delete_ShouldRemoveSource()
    {
        // Arrange
        var user = User.Create("delete-source-test@example.com", "Delete Source Test User");
        var createdUser = await _userRepository.CreateAsync(user);

        var connection = AuthConnection.Create(
            createdUser.Id,
            OAuthProvider.Google,
            "google-delete-user",
            "encrypted-token",
            null,
            null,
            "drive.readonly");
        var createdConnection = await _authConnectionRepository.CreateAsync(connection);

        var source = DocumentSourceConfig.CreateGoogleDrive(
            createdUser.Id,
            createdConnection.Id,
            "To Be Deleted",
            "delete-folder-id");

        var created = await _documentSourceRepository.CreateAsync(source);

        // Act
        await _documentSourceRepository.DeleteAsync(created.Id);
        var retrieved = await _documentSourceRepository.GetByIdAsync(created.Id);

        // Assert
        Assert.Null(retrieved);
    }

    [Fact]
    public async Task FullWorkflow_CreateUserWithConnectionAndSource_ShouldWork()
    {
        // Arrange - Create user
        var user = User.Create("full-workflow@example.com", "Full Workflow User");
        var createdUser = await _userRepository.CreateAsync(user);

        // Act - Create auth connection
        var connection = AuthConnection.Create(
            createdUser.Id,
            OAuthProvider.Google,
            "google-workflow-user",
            "encrypted-access-token",
            "encrypted-refresh-token",
            DateTime.UtcNow.AddHours(1),
            "openid email profile https://www.googleapis.com/auth/drive.readonly");
        var createdConnection = await _authConnectionRepository.CreateAsync(connection);

        // Act - Create document source
        var source = DocumentSourceConfig.CreateGoogleDrive(
            createdUser.Id,
            createdConnection.Id,
            "My Study Notes",
            "1BxiMVs0XRA5nFMdKvBdBZjgmUUqptlbs74OgvE2upms",
            "/flashcards");
        var createdSource = await _documentSourceRepository.CreateAsync(source);

        // Assert - Retrieve everything
        var retrievedUser = await _userRepository.GetByIdAsync(createdUser.Id);
        var retrievedConnection = await _authConnectionRepository.GetByIdAsync(createdConnection.Id);
        var retrievedSource = await _documentSourceRepository.GetByIdAsync(createdSource.Id);

        Assert.NotNull(retrievedUser);
        Assert.NotNull(retrievedConnection);
        Assert.NotNull(retrievedSource);

        Assert.Equal(createdUser.Id, retrievedConnection.UserId);
        Assert.Equal(createdConnection.Id, retrievedSource.AuthConnectionId);
    }
}
