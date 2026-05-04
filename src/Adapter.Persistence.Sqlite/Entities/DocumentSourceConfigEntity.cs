namespace Mnemi.Adapter.Persistence.Sqlite;

/// <summary>
/// Database entity for DocumentSourceConfig storage.
/// </summary>
public class DocumentSourceConfigEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid AuthConnectionId { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string ProviderConfigJson { get; set; } = string.Empty;
    public bool IsAccessible { get; set; }
    public string? LastErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
