using System;
using System.Collections.Generic;

using Mnemi.Domain.Enums;

namespace Mnemi.Domain.Entities;

public class StudySession
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid UserId { get; init; }
    public Guid DeckId { get; init; }
    public DateTime StartTime { get; init; } = DateTime.UtcNow;
    public DateTime? EndTime { get; set; }
    private readonly List<SessionResult> _results = new();
    public IReadOnlyCollection<SessionResult> Results => _results.AsReadOnly();

    public void AddResult(SessionResult result)
    {
        _results.Add(result);
    }
}
