using System.Text.Json.Serialization;

namespace Mnemi.Adapter.Persistence.Redis;

/// <summary>
/// JSON-serializable entity for AuthConnection storage in Redis.
/// </summary>
public class AuthConnectionJsonEntity
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("userId")]
    public Guid UserId { get; set; }

    [JsonPropertyName("provider")]
    public string Provider { get; set; } = string.Empty;

    [JsonPropertyName("providerUserId")]
    public string ProviderUserId { get; set; } = string.Empty;

    [JsonPropertyName("encryptedAccessToken")]
    public string EncryptedAccessToken { get; set; } = string.Empty;

    [JsonPropertyName("encryptedRefreshToken")]
    public string? EncryptedRefreshToken { get; set; }

    [JsonPropertyName("expiresAt")]
    public DateTime? ExpiresAt { get; set; }

    [JsonPropertyName("scopes")]
    public string Scopes { get; set; } = string.Empty;

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("isValid")]
    public bool IsValid { get; set; }
}
