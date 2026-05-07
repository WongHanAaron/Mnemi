using System.Text.Json.Serialization;

namespace Mnemi.Adapter.Persistence.Redis;

/// <summary>
/// JSON-serializable entity for User storage in Redis.
/// </summary>
public class UserJsonEntity
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
}
