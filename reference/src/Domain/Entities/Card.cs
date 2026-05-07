using System.Collections.Generic;

namespace Mnemi.Domain.Entities;

public abstract record Card(
    string Id,
    CardType CardType,
    IReadOnlyList<Group> Groups,
    LearningState? LearningState,
    string RawContent,
    string? Source,
    int? SourceLineNumberStart,
    int? SourceLineNumberEnd);