using System.Collections.Generic;

namespace Mnemi.Domain.Entities;

public sealed record QaCard(
    string Question,
    string Answer,
    string Id,
    IReadOnlyList<Group> Groups,
    LearningState? LearningState,
    string RawContent,
    string? Source,
    int? SourceLineNumberStart,
    int? SourceLineNumberEnd)
    : Card(Id, CardType.Qa, Groups, LearningState, RawContent, Source, SourceLineNumberStart, SourceLineNumberEnd);