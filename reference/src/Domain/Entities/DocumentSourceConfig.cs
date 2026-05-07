using System.Text.Json;
using Mnemi.Domain.ValueObjects;

namespace Mnemi.Domain.Entities;

/// <summary>
/// Entity representing a linked external location containing flashcard content.
/// </summary>
public class DocumentSourceConfig : Entity
{
    /// <summary>
    /// Foreign key to the User entity.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Navigation property to the owning user.
    /// </summary>
    public User User { get; private set; } = null!;

    /// <summary>
    /// Foreign key to the AuthConnection used to access this source.
    /// </summary>
    public Guid AuthConnectionId { get; private set; }

    /// <summary>
    /// Navigation property to the associated auth connection.
    /// </summary>
    public AuthConnection AuthConnection { get; private set; } = null!;

    /// <summary>
    /// The storage provider type.
    /// </summary>
    public DocumentSourceProvider Provider { get; private set; }

    /// <summary>
    /// User-defined display name for this source.
    /// Defaults to folder/repo name but can be customized.
    /// </summary>
    public string DisplayName { get; private set; }

    /// <summary>
    /// Provider-specific configuration as JSON.
    /// Google Drive: { "folderId": "..." }
    /// GitHub: { "repoOwner": "...", "repoName": "...", "rootPath": "..." }
    /// </summary>
    public string ProviderConfigJson { get; private set; }

    /// <summary>
    /// UTC timestamp when the source was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// UTC timestamp of last update.
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    /// <summary>
    /// Indicates if the source is currently accessible.
    /// Set to false if folder/repo is deleted or permissions revoked.
    /// </summary>
    public bool IsAccessible { get; private set; }

    /// <summary>
    /// Error message if source is not accessible.
    /// </summary>
    public string? LastErrorMessage { get; private set; }

    // EF Core requires a parameterless constructor
    private DocumentSourceConfig() { }

    private DocumentSourceConfig(
        Guid id,
        Guid userId,
        Guid authConnectionId,
        DocumentSourceProvider provider,
        string displayName,
        string providerConfigJson,
        DateTime createdAt,
        DateTime? updatedAt = null,
        bool? isAccessible = null,
        string? lastErrorMessage = null)
    {
        Id = id;
        UserId = userId;
        AuthConnectionId = authConnectionId;
        Provider = provider;
        DisplayName = displayName;
        ProviderConfigJson = providerConfigJson;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt ?? createdAt;
        IsAccessible = isAccessible ?? true;
        LastErrorMessage = lastErrorMessage;
    }

    /// <summary>
    /// Factory method to create a new Google Drive document source.
    /// </summary>
    public static DocumentSourceConfig CreateGoogleDrive(
        Guid userId,
        Guid authConnectionId,
        string displayName,
        string folderId,
        string? subPath = null)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID is required", nameof(userId));
        if (authConnectionId == Guid.Empty)
            throw new ArgumentException("Auth connection ID is required", nameof(authConnectionId));
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name is required", nameof(displayName));
        if (displayName.Length > 200)
            throw new ArgumentException("Display name must be 200 characters or less", nameof(displayName));
        if (string.IsNullOrWhiteSpace(folderId))
            throw new ArgumentException("Folder ID is required", nameof(folderId));

        var config = new GoogleDriveProviderConfig
        {
            FolderId = folderId.Trim(),
            SubPath = subPath?.Trim()
        };

        var configJson = JsonSerializer.Serialize(config);

        return new DocumentSourceConfig(
            Guid.NewGuid(),
            userId,
            authConnectionId,
            DocumentSourceProvider.GoogleDrive,
            displayName.Trim(),
            configJson,
            DateTime.UtcNow);
    }

    /// <summary>
    /// Factory method to create a new GitHub document source.
    /// </summary>
    public static DocumentSourceConfig CreateGitHub(
        Guid userId,
        Guid authConnectionId,
        string displayName,
        string repoOwner,
        string repoName,
        string? rootPath = null,
        string? branch = null)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID is required", nameof(userId));
        if (authConnectionId == Guid.Empty)
            throw new ArgumentException("Auth connection ID is required", nameof(authConnectionId));
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name is required", nameof(displayName));
        if (displayName.Length > 200)
            throw new ArgumentException("Display name must be 200 characters or less", nameof(displayName));
        if (string.IsNullOrWhiteSpace(repoOwner))
            throw new ArgumentException("Repository owner is required", nameof(repoOwner));
        if (string.IsNullOrWhiteSpace(repoName))
            throw new ArgumentException("Repository name is required", nameof(repoName));

        var config = new GitHubProviderConfig
        {
            RepoOwner = repoOwner.Trim(),
            RepoName = repoName.Trim(),
            RootPath = string.IsNullOrWhiteSpace(rootPath) ? "/" : rootPath.Trim(),
            Branch = branch?.Trim()
        };

        var configJson = JsonSerializer.Serialize(config);

        return new DocumentSourceConfig(
            Guid.NewGuid(),
            userId,
            authConnectionId,
            DocumentSourceProvider.GitHub,
            displayName.Trim(),
            configJson,
            DateTime.UtcNow);
    }

    /// <summary>
    /// Loads an existing document source from persistence. Used by repositories.
    /// </summary>
    public static DocumentSourceConfig Load(
        Guid id,
        Guid userId,
        Guid authConnectionId,
        DocumentSourceProvider provider,
        string displayName,
        string providerConfigJson,
        DateTime createdAt,
        DateTime updatedAt,
        bool isAccessible,
        string? lastErrorMessage = null)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("ID is required", nameof(id));
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID is required", nameof(userId));
        if (authConnectionId == Guid.Empty)
            throw new ArgumentException("Auth connection ID is required", nameof(authConnectionId));
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name is required", nameof(displayName));
        if (string.IsNullOrWhiteSpace(providerConfigJson))
            throw new ArgumentException("Provider config JSON is required", nameof(providerConfigJson));

        return new DocumentSourceConfig(
            id,
            userId,
            authConnectionId,
            provider,
            displayName.Trim(),
            providerConfigJson,
            createdAt,
            updatedAt,
            isAccessible,
            lastErrorMessage);
    }

    /// <summary>
    /// Updates the display name.
    /// </summary>
    public void UpdateDisplayName(string newDisplayName)
    {
        if (string.IsNullOrWhiteSpace(newDisplayName))
            throw new ArgumentException("Display name is required", nameof(newDisplayName));
        if (newDisplayName.Length > 200)
            throw new ArgumentException("Display name must be 200 characters or less", nameof(newDisplayName));

        DisplayName = newDisplayName.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the source as inaccessible with an error message.
    /// </summary>
    public void MarkInaccessible(string? errorMessage)
    {
        IsAccessible = false;
        LastErrorMessage = errorMessage;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the source as accessible.
    /// </summary>
    public void MarkAccessible()
    {
        IsAccessible = true;
        LastErrorMessage = null;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the provider configuration deserialized to the specified type.
    /// </summary>
    public T GetProviderConfig<T>() where T : class
    {
        return JsonSerializer.Deserialize<T>(ProviderConfigJson)
            ?? throw new InvalidOperationException("Failed to deserialize provider configuration");
    }

    /// <summary>
    /// Gets the unique key for this source based on provider configuration.
    /// Used for duplicate detection.
    /// </summary>
    public string GetProviderConfigKey()
    {
        return Provider switch
        {
            DocumentSourceProvider.GoogleDrive => GetProviderConfig<GoogleDriveProviderConfig>().FolderId,
            DocumentSourceProvider.GitHub =>
                $"{GetProviderConfig<GitHubProviderConfig>().RepoOwner}/{GetProviderConfig<GitHubProviderConfig>().RepoName}",
            _ => throw new NotSupportedException($"Provider {Provider} is not supported")
        };
    }
}
