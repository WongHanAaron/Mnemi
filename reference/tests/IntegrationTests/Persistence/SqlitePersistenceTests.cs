using Microsoft.EntityFrameworkCore;
using Mnemi.Adapter.Persistence.Sqlite;
using Mnemi.Domain.Entities;
using Xunit;

namespace Mnemi.IntegrationTests.Persistence;

/// <summary>
/// Integration tests for SQLite persistence adapter.
/// Tests both file-based and in-memory SQLite databases.
/// </summary>
public class SqlitePersistenceTests : IDisposable
{
    private readonly string _testDbPath;

    public SqlitePersistenceTests()
    {
        // Create a unique test database path for each test run
        _testDbPath = Path.Combine(Path.GetTempPath(), $"mnemi-test-{Guid.NewGuid()}.db");
    }

    public void Dispose()
    {
        // Clean up test database file
        try
        {
            if (System.IO.File.Exists(_testDbPath))
            {
                System.IO.File.Delete(_testDbPath);
            }
        }
        catch
        {
            // Best effort cleanup
        }
    }

    private MnemiDbContext CreateFileContext()
    {
        var options = new DbContextOptionsBuilder<MnemiDbContext>()
            .UseSqlite($"Data Source={_testDbPath}")
            .Options;

        var context = new MnemiDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    private MnemiDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<MnemiDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        var context = new MnemiDbContext(options);
        context.Database.OpenConnection();
        context.Database.EnsureCreated();
        return context;
    }

    #region File-Based SQLite Tests

    [Fact]
    public async Task FileDatabase_Connection_ShouldBeEstablished()
    {
        // Arrange & Act
        using var context = CreateFileContext();

        // Assert
        Assert.True(context.Database.CanConnect());
    }

    [Fact]
    public async Task FileDatabase_UserRepository_CreateAndRead_ShouldWork()
    {
        // Arrange
        using var context = CreateFileContext();
        var repository = new UserRepository(context);
        var user = User.Create("file-test@example.com", "File Test User");

        // Act
        var created = await repository.CreateAsync(user);
        var retrieved = await repository.GetByIdAsync(created.Id);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(user.Email, retrieved.Email);
        Assert.Equal(user.DisplayName, retrieved.DisplayName);
    }

    [Fact]
    public async Task FileDatabase_UserRepository_GetByEmail_ShouldReturnUser()
    {
        // Arrange
        using var context = CreateFileContext();
        var repository = new UserRepository(context);
        var user = User.Create("file-email-test@example.com", "File Email Test User");
        await repository.CreateAsync(user);

        // Act
        var retrieved = await repository.GetByEmailAsync("file-email-test@example.com");

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(user.Email, retrieved.Email);
    }

    [Fact]
    public async Task FileDatabase_UserRepository_ExistsByEmail_ShouldReturnTrue()
    {
        // Arrange
        using var context = CreateFileContext();
        var repository = new UserRepository(context);
        var user = User.Create("file-exists-test@example.com", "File Exists Test User");
        await repository.CreateAsync(user);

        // Act
        var exists = await repository.ExistsByEmailAsync("file-exists-test@example.com");

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task FileDatabase_UserRepository_Update_ShouldModifyUser()
    {
        // Arrange
        using var context = CreateFileContext();
        var repository = new UserRepository(context);
        var user = User.Create("file-update-test@example.com", "Original Name");
        var created = await repository.CreateAsync(user);

        // Act
        created.UpdateDisplayName("Updated File Name");
        await repository.UpdateAsync(created);
        var retrieved = await repository.GetByIdAsync(created.Id);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal("Updated File Name", retrieved.DisplayName);
    }

    [Fact]
    public async Task FileDatabase_UserRepository_Delete_ShouldRemoveUser()
    {
        // Arrange
        using var context = CreateFileContext();
        var repository = new UserRepository(context);
        var user = User.Create("file-delete-test@example.com", "File Delete Test User");
        var created = await repository.CreateAsync(user);

        // Act
        await repository.DeleteAsync(created.Id);
        var retrieved = await repository.GetByIdAsync(created.Id);

        // Assert
        Assert.Null(retrieved);
    }

    [Fact]
    public async Task FileDatabase_AuthConnectionRepository_CreateAndRead_ShouldWork()
    {
        // Arrange
        using var context = CreateFileContext();
        var userRepository = new UserRepository(context);
        var authRepository = new AuthConnectionRepository(context);

        var user = User.Create("file-auth-test@example.com", "File Auth Test User");
        var createdUser = await userRepository.CreateAsync(user);

        var connection = AuthConnection.Create(
            createdUser.Id,
            OAuthProvider.Google,
            "file-google-user-123",
            "encrypted-access-token",
            "encrypted-refresh-token",
            DateTime.UtcNow.AddHours(1),
            "openid email profile");

        // Act
        var created = await authRepository.CreateAsync(connection);
        var retrieved = await authRepository.GetByIdAsync(created.Id);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(connection.Provider, retrieved.Provider);
        Assert.Equal(connection.ProviderUserId, retrieved.ProviderUserId);
    }

    [Fact]
    public async Task FileDatabase_DocumentSourceRepository_CreateAndRead_ShouldWork()
    {
        // Arrange
        using var context = CreateFileContext();
        var userRepository = new UserRepository(context);
        var authRepository = new AuthConnectionRepository(context);
        var sourceRepository = new DocumentSourceRepository(context);

        var user = User.Create("file-source-test@example.com", "File Source Test User");
        var createdUser = await userRepository.CreateAsync(user);

        var connection = AuthConnection.Create(
            createdUser.Id,
            OAuthProvider.Google,
            "file-google-source-user",
            "encrypted-token",
            null,
            null,
            "drive.readonly");
        var createdConnection = await authRepository.CreateAsync(connection);

        var source = DocumentSourceConfig.CreateGoogleDrive(
            createdUser.Id,
            createdConnection.Id,
            "My File Flashcards",
            "1BxiMVs0XRA5nFMdKvBdBZjgmUUqptlbs74OgvE2upms");

        // Act
        var created = await sourceRepository.CreateAsync(source);
        var retrieved = await sourceRepository.GetByIdAsync(created.Id);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(source.DisplayName, retrieved.DisplayName);
        Assert.Equal(source.Provider, retrieved.Provider);
    }

    [Fact]
    public async Task FileDatabase_FullWorkflow_CreateUserWithConnectionAndSource_ShouldWork()
    {
        // Arrange
        using var context = CreateFileContext();
        var userRepository = new UserRepository(context);
        var authRepository = new AuthConnectionRepository(context);
        var sourceRepository = new DocumentSourceRepository(context);

        // Act - Create user
        var user = User.Create("file-full-workflow@example.com", "File Full Workflow User");
        var createdUser = await userRepository.CreateAsync(user);

        // Act - Create auth connection
        var connection = AuthConnection.Create(
            createdUser.Id,
            OAuthProvider.Google,
            "file-google-workflow-user",
            "encrypted-access-token",
            "encrypted-refresh-token",
            DateTime.UtcNow.AddHours(1),
            "openid email profile https://www.googleapis.com/auth/drive.readonly");
        var createdConnection = await authRepository.CreateAsync(connection);

        // Act - Create document source
        var source = DocumentSourceConfig.CreateGoogleDrive(
            createdUser.Id,
            createdConnection.Id,
            "My File Study Notes",
            "1BxiMVs0XRA5nFMdKvBdBZjgmUUqptlbs74OgvE2upms",
            "/flashcards");
        var createdSource = await sourceRepository.CreateAsync(source);

        // Assert - Retrieve everything
        var retrievedUser = await userRepository.GetByIdAsync(createdUser.Id);
        var retrievedConnection = await authRepository.GetByIdAsync(createdConnection.Id);
        var retrievedSource = await sourceRepository.GetByIdAsync(createdSource.Id);

        Assert.NotNull(retrievedUser);
        Assert.NotNull(retrievedConnection);
        Assert.NotNull(retrievedSource);

        Assert.Equal(createdUser.Id, retrievedConnection.UserId);
        Assert.Equal(createdConnection.Id, retrievedSource.AuthConnectionId);
    }

    #endregion

    #region In-Memory SQLite Tests

    [Fact]
    public async Task InMemoryDatabase_Connection_ShouldBeEstablished()
    {
        // Arrange & Act
        using var context = CreateInMemoryContext();

        // Assert
        Assert.True(context.Database.CanConnect());
    }

    [Fact]
    public async Task InMemoryDatabase_UserRepository_CreateAndRead_ShouldWork()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new UserRepository(context);
        var user = User.Create("memory-test@example.com", "Memory Test User");

        // Act
        var created = await repository.CreateAsync(user);
        var retrieved = await repository.GetByIdAsync(created.Id);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(user.Email, retrieved.Email);
        Assert.Equal(user.DisplayName, retrieved.DisplayName);
    }

    [Fact]
    public async Task InMemoryDatabase_UserRepository_GetByEmail_ShouldReturnUser()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new UserRepository(context);
        var user = User.Create("memory-email-test@example.com", "Memory Email Test User");
        await repository.CreateAsync(user);

        // Act
        var retrieved = await repository.GetByEmailAsync("memory-email-test@example.com");

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(user.Email, retrieved.Email);
    }

    [Fact]
    public async Task InMemoryDatabase_AuthConnectionRepository_CreateAndRead_ShouldWork()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var userRepository = new UserRepository(context);
        var authRepository = new AuthConnectionRepository(context);

        var user = User.Create("memory-auth-test@example.com", "Memory Auth Test User");
        var createdUser = await userRepository.CreateAsync(user);

        var connection = AuthConnection.Create(
            createdUser.Id,
            OAuthProvider.GitHub,
            "memory-github-user-123",
            "encrypted-github-token",
            null,
            null,
            "read:user repo");

        // Act
        var created = await authRepository.CreateAsync(connection);
        var retrieved = await authRepository.GetByIdAsync(created.Id);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(connection.Provider, retrieved.Provider);
        Assert.Equal(connection.ProviderUserId, retrieved.ProviderUserId);
    }

    [Fact]
    public async Task InMemoryDatabase_DocumentSourceRepository_CreateAndRead_ShouldWork()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var userRepository = new UserRepository(context);
        var authRepository = new AuthConnectionRepository(context);
        var sourceRepository = new DocumentSourceRepository(context);

        var user = User.Create("memory-source-test@example.com", "Memory Source Test User");
        var createdUser = await userRepository.CreateAsync(user);

        var connection = AuthConnection.Create(
            createdUser.Id,
            OAuthProvider.GitHub,
            "memory-github-source-user",
            "encrypted-token",
            null,
            null,
            "repo");
        var createdConnection = await authRepository.CreateAsync(connection);

        var source = DocumentSourceConfig.CreateGitHub(
            createdUser.Id,
            createdConnection.Id,
            "My Memory Repo",
            "johndoe",
            "study-notes",
            "/flashcards",
            "main");

        // Act
        var created = await sourceRepository.CreateAsync(source);
        var retrieved = await sourceRepository.GetByIdAsync(created.Id);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(source.DisplayName, retrieved.DisplayName);
        Assert.Equal(source.Provider, retrieved.Provider);
    }

    [Fact]
    public async Task InMemoryDatabase_FullWorkflow_CreateUserWithConnectionAndSource_ShouldWork()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var userRepository = new UserRepository(context);
        var authRepository = new AuthConnectionRepository(context);
        var sourceRepository = new DocumentSourceRepository(context);

        // Act - Create user
        var user = User.Create("memory-full-workflow@example.com", "Memory Full Workflow User");
        var createdUser = await userRepository.CreateAsync(user);

        // Act - Create auth connection
        var connection = AuthConnection.Create(
            createdUser.Id,
            OAuthProvider.GitHub,
            "memory-github-workflow-user",
            "encrypted-access-token",
            "encrypted-refresh-token",
            DateTime.UtcNow.AddHours(1),
            "read:user repo");
        var createdConnection = await authRepository.CreateAsync(connection);

        // Act - Create document source
        var source = DocumentSourceConfig.CreateGitHub(
            createdUser.Id,
            createdConnection.Id,
            "My Memory Study Notes",
            "johndoe",
            "study-notes",
            "/flashcards",
            "main");
        var createdSource = await sourceRepository.CreateAsync(source);

        // Assert - Retrieve everything
        var retrievedUser = await userRepository.GetByIdAsync(createdUser.Id);
        var retrievedConnection = await authRepository.GetByIdAsync(createdConnection.Id);
        var retrievedSource = await sourceRepository.GetByIdAsync(createdSource.Id);

        Assert.NotNull(retrievedUser);
        Assert.NotNull(retrievedConnection);
        Assert.NotNull(retrievedSource);

        Assert.Equal(createdUser.Id, retrievedConnection.UserId);
        Assert.Equal(createdConnection.Id, retrievedSource.AuthConnectionId);
    }

    [Fact]
    public async Task InMemoryDatabase_DataIsolation_DifferentContextsShouldBeIsolated()
    {
        // Arrange - Create two separate in-memory contexts
        using var context1 = CreateInMemoryContext();
        using var context2 = CreateInMemoryContext();
        var repository1 = new UserRepository(context1);
        var repository2 = new UserRepository(context2);

        var user = User.Create("isolation-test@example.com", "Isolation Test User");

        // Act - Create user in context1
        var created = await repository1.CreateAsync(user);

        // Assert - User exists in context1
        var retrievedFromContext1 = await repository1.GetByIdAsync(created.Id);
        Assert.NotNull(retrievedFromContext1);

        // Assert - User does NOT exist in context2 (different in-memory database)
        var retrievedFromContext2 = await repository2.GetByIdAsync(created.Id);
        Assert.Null(retrievedFromContext2);
    }

    #endregion

    #region Comparison Tests

    [Fact]
    public async Task BothDatabases_ShouldHaveSameBehavior_ForBasicOperations()
    {
        // Arrange
        using var fileContext = CreateFileContext();
        using var memoryContext = CreateInMemoryContext();
        var fileRepo = new UserRepository(fileContext);
        var memoryRepo = new UserRepository(memoryContext);

        var fileUser = User.Create("compare-file@example.com", "Compare File User");
        var memoryUser = User.Create("compare-memory@example.com", "Compare Memory User");

        // Act
        var fileCreated = await fileRepo.CreateAsync(fileUser);
        var memoryCreated = await memoryRepo.CreateAsync(memoryUser);

        var fileRetrieved = await fileRepo.GetByIdAsync(fileCreated.Id);
        var memoryRetrieved = await memoryRepo.GetByIdAsync(memoryCreated.Id);

        // Assert - Both should behave the same way
        Assert.NotNull(fileRetrieved);
        Assert.NotNull(memoryRetrieved);
        Assert.Equal(fileUser.Email, fileRetrieved.Email);
        Assert.Equal(memoryUser.Email, memoryRetrieved.Email);
    }

    #endregion
}
