using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Mnemi.Domain.Cards;

public sealed class MetadataParser
{
    private static readonly Regex MnemiCommentPattern = new(@"^<!--\s*mnemi:\s*(.*?)\s*-->$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public int FindMetadataLine(string[] lines, int startIndex, out MetadataResult result)
    {
        for (var index = startIndex; index < lines.Length; index++)
        {
            if (string.IsNullOrWhiteSpace(lines[index]))
            {
                continue;
            }

            var trimmed = lines[index].Trim();
            var match = MnemiCommentPattern.Match(trimmed);
            if (!match.Success)
            {
                break;
            }

            result = ParseMnemiMetadata(match.Groups[1].Value);
            return index;
        }

        result = new MetadataResult(LearningState.New, Array.Empty<Group>());
        return -1;
    }

    public bool TryParseComment(string text, out MetadataResult result)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            result = new MetadataResult(LearningState.New, Array.Empty<Group>());
            return false;
        }

        var match = MnemiCommentPattern.Match(text.Trim());
        if (!match.Success)
        {
            result = new MetadataResult(LearningState.New, Array.Empty<Group>());
            return false;
        }

        result = ParseMnemiMetadata(match.Groups[1].Value);
        return true;
    }

    private static MetadataResult ParseMnemiMetadata(string metadataText)
    {
        var tokens = metadataText.Split('|', StringSplitOptions.RemoveEmptyEntries)
            .Select(token => token.Trim())
            .ToArray();

        string? hash = null;
        var status = "new";
        int? days = null;
        decimal? ease = null;
        DateTime? due = null;
        string? lastResponse = null;
        int? lapses = null;
        int? repetitions = null;
        var tagOverride = new List<Group>();

        foreach (var token in tokens)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                continue;
            }

            if (!token.Contains('=') && hash == null)
            {
                hash = token;
                continue;
            }

            var parts = token.Split('=', 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
            {
                continue;
            }

            var key = parts[0].Trim().ToLowerInvariant();
            var value = parts[1].Trim();

            switch (key)
            {
                case "status":
                    status = value;
                    break;
                case "days":
                    if (int.TryParse(value, out var parsedDays))
                    {
                        days = parsedDays;
                    }
                    break;
                case "ease":
                    if (decimal.TryParse(value, out var parsedEase))
                    {
                        ease = parsedEase;
                    }
                    break;
                case "due":
                    if (DateTime.TryParse(value, out var parsedDue))
                    {
                        due = parsedDue;
                    }
                    break;
                case "last":
                    lastResponse = value;
                    break;
                case "lapses":
                    if (int.TryParse(value, out var parsedLapses))
                    {
                        lapses = parsedLapses;
                    }
                    break;
                case "reps":
                    if (int.TryParse(value, out var parsedReps))
                    {
                        repetitions = parsedReps;
                    }
                    break;
                case "tag":
                    tagOverride.AddRange(ParseTagOverride(value));
                    break;
            }
        }

        var state = new LearningState(hash, status, days, ease, due, lastResponse, lapses, repetitions, tagOverride.Any() ? Group.PruneAncestors(tagOverride) : null);
        return new MetadataResult(state, state.TagOverride ?? Array.Empty<Group>());
    }

    private static IReadOnlyList<Group> ParseTagOverride(string tagValue)
    {
        return tagValue.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(part => Group.Parse(part.Trim()))
            .ToList();
    }
}

public sealed record MetadataResult(LearningState LearningState, IReadOnlyList<Group> TagOverride);
