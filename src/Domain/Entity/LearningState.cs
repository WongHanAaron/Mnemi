using System;
using System.Collections.Generic;

namespace Mnemi.Domain.Entities;

public sealed record LearningState(
    string? Hash,
    string Status,
    int? Days,
    decimal? Ease,
    DateTime? Due,
    string? LastResponse,
    int? Lapses,
    int? Repetitions,
    IReadOnlyList<Group>? TagOverride)
{
    public static LearningState New { get; } = new(null, "new", null, null, null, null, null, null, null);
}