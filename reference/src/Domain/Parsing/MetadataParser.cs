using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Mnemi.Domain.Entities;
using EntityGroup = Mnemi.Domain.Entities.Group;

namespace Mnemi.Domain.Parsing;

public interface IMetadataParser
{
    int FindMetadataLine(string[] lines, int startIndex, out MetadataResult result);

    bool TryParseComment(string text, out MetadataResult result);
}

public sealed class MetadataParser : IMetadataParser
{
    private const string DefaultStatus = "new";
    private const string StatusKey = "status";
    private const string DaysKey = "days";
    private const string EaseKey = "ease";
    private const string DueKey = "due";
    private const string LastKey = "last";
    private const string LapsesKey = "lapses";
    private const string RepsKey = "reps";
    private const string TagKey = "tag";
    private static readonly Regex MnemiCommentPattern = new(@"^<!--\s*mnemi:\s*(.*?)\s*-->$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private readonly IGroupParser _groupParser;

    public MetadataParser(IGroupParser groupParser)
    {
        _groupParser = groupParser ?? throw new ArgumentNullException(nameof(groupParser));
    }

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

            result = ParseMnemiMetadata(match.Groups[1].Value, _groupParser);
            return index;
        }

        result = new MetadataResult(LearningState.New, Array.Empty<EntityGroup>());
        return -1;
    }

    public bool TryParseComment(string text, out MetadataResult result)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            result = new MetadataResult(LearningState.New, Array.Empty<EntityGroup>());
            return false;
        }

        var match = MnemiCommentPattern.Match(text.Trim());
        if (!match.Success)
        {
            result = new MetadataResult(LearningState.New, Array.Empty<EntityGroup>());
            return false;
        }

        result = ParseMnemiMetadata(match.Groups[1].Value, _groupParser);
        return true;
    }

    private static MetadataResult ParseMnemiMetadata(string metadataText, IGroupParser groupParser)
    {
        var tokens = metadataText.Split('|', StringSplitOptions.RemoveEmptyEntries)
            .Select(token => token.Trim())
            .ToArray();

        string? hash = null;
        var status = DefaultStatus;
        int? days = null;
        decimal? ease = null;
        DateTime? due = null;
        string? lastResponse = null;
        int? lapses = null;
        int? repetitions = null;
        var tagOverride = new List<EntityGroup>();

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
                case StatusKey:
                    status = value;
                    break;
                case DaysKey:
                    if (int.TryParse(value, out var parsedDays))
                    {
                        days = parsedDays;
                    }
                    break;
                case EaseKey:
                    if (decimal.TryParse(value, out var parsedEase))
                    {
                        ease = parsedEase;
                    }
                    break;
                case DueKey:
                    if (DateTime.TryParse(value, out var parsedDue))
                    {
                        due = parsedDue;
                    }
                    break;
                case LastKey:
                    lastResponse = value;
                    break;
                case LapsesKey:
                    if (int.TryParse(value, out var parsedLapses))
                    {
                        lapses = parsedLapses;
                    }
                    break;
                case RepsKey:
                    if (int.TryParse(value, out var parsedReps))
                    {
                        repetitions = parsedReps;
                    }
                    break;
                case TagKey:
                    tagOverride.AddRange(ParseTagOverride(value, groupParser));
                    break;
            }
        }

        var state = new LearningState(hash, status, days, ease, due, lastResponse, lapses, repetitions, tagOverride.Any() ? groupParser.PruneAncestors(tagOverride) : null);
        return new MetadataResult(state, state.TagOverride ?? Array.Empty<EntityGroup>());
    }

    private static IReadOnlyList<EntityGroup> ParseTagOverride(string tagValue, IGroupParser groupParser)
    {
        return tagValue.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(part => groupParser.Parse(part.Trim()))
            .ToList();
    }
}

public sealed record MetadataResult(LearningState LearningState, IReadOnlyList<EntityGroup> TagOverride);
