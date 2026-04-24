using System.Collections.Generic;

namespace Mnemi.Domain.Entities;

public sealed record MultipleChoiceCard(
    string Question,
    IReadOnlyList<MultipleChoiceOption> Options,
    string Id,
    IReadOnlyList<Group> Groups,
    LearningState? LearningState,
    string RawContent,
    string? Source,
    int? SourceLineNumberStart,
    int? SourceLineNumberEnd)
    : Card(Id, CardType.MultipleChoice, Groups, LearningState, RawContent, Source, SourceLineNumberStart, SourceLineNumberEnd);