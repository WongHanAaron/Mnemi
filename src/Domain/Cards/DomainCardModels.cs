using System;
using System.Collections.Generic;
using System.Linq;

namespace Mnemi.Domain.Cards;

public sealed record File(
    string Filename,
    string RelativePath,
    DateTime? DateCreated,
    DateTime? DateLastModified,
    string FileContents);

public sealed record Document(
    File File,
    string Content,
    IReadOnlyList<Group> DocumentTags);

public sealed record Group(IReadOnlyList<string> Segments)
{
    public string DisplayPath => string.Join("::", Segments);

    public static Group Parse(string rawTag)
    {
        if (string.IsNullOrWhiteSpace(rawTag))
        {
            throw new ArgumentException("Tag cannot be null or whitespace.", nameof(rawTag));
        }

        var separators = new[] { "::", "/" };
        var rawSegments = rawTag.Split(separators, StringSplitOptions.RemoveEmptyEntries);
        var normalizedSegments = rawSegments
            .Select(NormalizeSegment)
            .Where(segment => !string.IsNullOrWhiteSpace(segment))
            .ToArray();

        if (normalizedSegments.Length == 0)
        {
            throw new ArgumentException("Tag must contain at least one segment.", nameof(rawTag));
        }

        return new Group(normalizedSegments);
    }

    public static IReadOnlyList<Group> PruneAncestors(IEnumerable<Group> groups)
    {
        if (groups == null)
        {
            return Array.Empty<Group>();
        }

        var distinctGroups = groups
            .Where(group => group != null)
            .Distinct()
            .ToList();

        return distinctGroups
            .Where(group => !distinctGroups.Any(other => !ReferenceEquals(other, group) && IsPrefixOf(group.Segments, other.Segments)))
            .ToList();
    }

    private static bool IsPrefixOf(IReadOnlyList<string> prefix, IReadOnlyList<string> candidate)
    {
        if (prefix.Count >= candidate.Count)
        {
            return false;
        }

        for (var index = 0; index < prefix.Count; index++)
        {
            if (!string.Equals(prefix[index], candidate[index], StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    private static string NormalizeSegment(string segment)
    {
        segment = segment.Trim();
        if (string.IsNullOrEmpty(segment))
        {
            return string.Empty;
        }

        if (segment.All(c => char.IsLower(c) || char.IsDigit(c) || c == '_'))
        {
            return char.ToUpperInvariant(segment[0]) + segment.Substring(1);
        }

        return char.ToUpperInvariant(segment[0]) + segment.Substring(1);
    }
}

public enum CardType
{
    Qa,
    Cloze,
    MultipleChoice
}

public abstract record Card(
    string Id,
    CardType CardType,
    IReadOnlyList<Group> Groups,
    LearningState? LearningState,
    string RawContent,
    string? Source,
    int? SourceLineNumberStart,
    int? SourceLineNumberEnd);

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

public sealed record ClozeBlank(string Placeholder, IReadOnlyList<ClozeAnswerOption> Options);

public sealed record ClozeAnswerOption(string Text, bool IsAccepted);

public sealed record MultipleChoiceOption(string Text, bool IsCorrect);

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
