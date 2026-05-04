namespace Mnemi.Domain.Entities;

/// <summary>
/// Entity representing an OAuth connection between a Mnemi user and an external provider.
/// </summary>
public class AuthConnection : Entity
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
    /// The OAuth provider (Google or GitHub).
    /// </summary>
    public OAuthProvider Provider { get; private set; }

    /// <summary>
    /// Provider-specific user identifier.
    /// Google: 'sub' claim
    /// GitHub: 'id' claim
    /// </summary>
    public string ProviderUserId { get; private set; }

    /// <summary>
    /// Encrypted OAuth access token.
    /// Encrypted at rest using AES-256.
    /// </summary>
    public string EncryptedAccessToken { get; private set; }

    /// <summary>
    /// Encrypted OAuth refresh token (if provided).
    /// Encrypted at rest using AES-256.
    /// </summary>
    public string? EncryptedRefreshToken { get; private set; }

    /// <summary>
    /// UTC timestamp when the access token expires.
    /// Null if token doesn't expire.
    /// </summary>
    public DateTime? ExpiresAt { get; private set; }

    /// <summary>
    /// Space-separated list of granted OAuth scopes.
    /// </summary>
    public string Scopes { get; private set; }

    /// <summary>
    /// UTC timestamp when the connection was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// UTC timestamp of last update.
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    /// <summary>
    /// Indicates if the connection is valid and usable.
    /// Set to false if token refresh fails or user revokes access.
    /// </summary>
    public bool IsValid { get; private set; }

    // EF Core requires a parameterless constructor
    private AuthConnection() { }

    private AuthConnection(
        Guid id,
        Guid userId,
        OAuthProvider provider,
        string providerUserId,
        string encryptedAccessToken,
        string? encryptedRefreshToken,
        DateTime? expiresAt,
        string scopes,
        DateTime createdAt,
        DateTime? updatedAt = null,
        bool? isValid = null)
    {
        Id = id;
        UserId = userId;
        Provider = provider;
        ProviderUserId = providerUserId;
        EncryptedAccessToken = encryptedAccessToken;
        EncryptedRefreshToken = encryptedRefreshToken;
        ExpiresAt = expiresAt;
        Scopes = scopes;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt ?? createdAt;
        IsValid = isValid ?? true;
    }

    /// <summary>
    /// Factory method to create a new auth connection.
    /// </summary>
    public static AuthConnection Create(
        Guid userId,
        OAuthProvider provider,
        string providerUserId,
        string encryptedAccessToken,
        string? encryptedRefreshToken,
        DateTime? expiresAt,
        string scopes)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID is required", nameof(userId));
        if (string.IsNullOrWhiteSpace(providerUserId))
            throw new ArgumentException("Provider user ID is required", nameof(providerUserId));
        if (providerUserId.Length > 255)
            throw new ArgumentException("Provider user ID must be 255 characters or less", nameof(providerUserId));
        if (string.IsNullOrWhiteSpace(encryptedAccessToken))
            throw new ArgumentException("Access token is required", nameof(encryptedAccessToken));
        if (string.IsNullOrWhiteSpace(scopes))
            throw new ArgumentException("Scopes are required", nameof(scopes));

        return new AuthConnection(
            Guid.NewGuid(),
            userId,
            provider,
            providerUserId.Trim(),
            encryptedAccessToken,
            encryptedRefreshToken,
            expiresAt,
            scopes.Trim(),
            DateTime.UtcNow);
    }

    /// <summary>
    /// Loads an existing auth connection from persistence. Used by repositories.
    /// </summary>
    public static AuthConnection Load(
        Guid id,
        Guid userId,
        OAuthProvider provider,
        string providerUserId,
        string encryptedAccessToken,
        string? encryptedRefreshToken,
        DateTime? expiresAt,
        string scopes,
        DateTime createdAt,
        DateTime updatedAt,
        bool isValid)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("ID is required", nameof(id));
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID is required", nameof(userId));
        if (string.IsNullOrWhiteSpace(providerUserId))
            throw new ArgumentException("Provider user ID is required", nameof(providerUserId));
        if (string.IsNullOrWhiteSpace(encryptedAccessToken))
            throw new ArgumentException("Access token is required", nameof(encryptedAccessToken));
        if (string.IsNullOrWhiteSpace(scopes))
            throw new ArgumentException("Scopes are required", nameof(scopes));

        return new AuthConnection(
            id,
            userId,
            provider,
            providerUserId.Trim(),
            encryptedAccessToken,
            encryptedRefreshToken,
            expiresAt,
            scopes.Trim(),
            createdAt,
            updatedAt,
            isValid);
    }

    /// <summary>
    /// Updates the stored tokens.
    /// </summary>
    public void UpdateTokens(string encryptedAccessToken, string? encryptedRefreshToken, DateTime? expiresAt)
    {
        if (string.IsNullOrWhiteSpace(encryptedAccessToken))
            throw new ArgumentException("Access token is required", nameof(encryptedAccessToken));

        EncryptedAccessToken = encryptedAccessToken;
        EncryptedRefreshToken = encryptedRefreshToken;
        ExpiresAt = expiresAt;
        UpdatedAt = DateTime.UtcNow;
        IsValid = true;
    }

    /// <summary>
    /// Marks the connection as invalid (e.g., when refresh fails or user revokes access).
    /// </summary>
    public void MarkInvalid()
    {
        IsValid = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if the access token is expired.
    /// </summary>
    public bool IsTokenExpired()
    {
        if (!ExpiresAt.HasValue)
            return false;

        // Consider token expired 5 minutes before actual expiration to account for clock skew
        return DateTime.UtcNow.AddMinutes(5) >= ExpiresAt.Value;
    }

    /// <summary>
    /// Checks if the connection has the specified scope.
    /// </summary>
    public bool HasScope(string scope)
    {
        if (string.IsNullOrWhiteSpace(scope))
            return false;

        return Scopes.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Any(s => s.Equals(scope, StringComparison.OrdinalIgnoreCase));
    }
}
