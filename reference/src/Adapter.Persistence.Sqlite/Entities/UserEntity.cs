namespace Mnemi.Adapter.Persistence.Sqlite;

/// <summary>
/// Database entity for User storage.
/// </summary>
public class UserEntity
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
