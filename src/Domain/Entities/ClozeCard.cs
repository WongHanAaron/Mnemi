using System.Collections.Generic;

namespace Mnemi.Domain.Entities;

public sealed record ClozeCard(
    string QuestionText,
    IReadOnlyList<ClozeBlank> ClozeBlanks,
    string Id,
    IReadOnlyList<Group> Groups,
    LearningState? LearningState,
    string RawContent,
    string? Source,
    int? SourceLineNumberStart,
    int? SourceLineNumberEnd)
    : Card(Id, CardType.Cloze, Groups, LearningState, RawContent, Source, SourceLineNumberStart, SourceLineNumberEnd);