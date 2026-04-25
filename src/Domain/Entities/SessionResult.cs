using System;
using Mnemi.Domain.Enums;

namespace Mnemi.Domain.Entities;

public class SessionResult
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid StudySessionId { get; init; }
    public Guid CardId { get; init; }
    public ReviewRating Rating { get; set; }
    public TimeSpan TimeSpent { get; set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
