namespace Mnemi.Domain.Entities;

public class User
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string UserName { get; set; } = "Guest";
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
