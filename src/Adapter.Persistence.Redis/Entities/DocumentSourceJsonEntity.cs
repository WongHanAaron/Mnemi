using System.Text.Json.Serialization;

namespace Mnemi.Adapter.Persistence.Redis;

/// <summary>
/// JSON-serializable entity for DocumentSourceConfig storage in Redis.
/// </summary>
public class DocumentSourceJsonEntity
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("userId")]
    public Guid UserId { get; set; }

    [JsonPropertyName("authConnectionId")]
    public Guid AuthConnectionId { get; set; }

    [JsonPropertyName("provider")]
    public string Provider { get; set; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("providerConfigJson")]
    public string ProviderConfigJson { get; set; } = string.Empty;

    [JsonPropertyName("isAccessible")]
    public bool IsAccessible { get; set; }

    [JsonPropertyName("lastErrorMessage")]
    public string? LastErrorMessage { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; }
}
