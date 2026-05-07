using System;
using System.Collections.Generic;
using System.Linq;
using Mnemi.Domain.Entities;

namespace Mnemi.Domain.Parsing;

public interface IGroupParser
{
    Group Parse(string rawTag);

    IReadOnlyList<Group> PruneAncestors(IEnumerable<Group> groups);
}

public sealed class GroupParser : IGroupParser
{
    private const string EmptyTagErrorMessage = "Tag cannot be null or whitespace.";
    private const string MissingSegmentErrorMessage = "Tag must contain at least one segment.";
    private const string GroupPathSeparator = "/";
    private static readonly string[] GroupSeparators = { CardParsingConstants.CardSeparator, GroupPathSeparator };

    public Group Parse(string rawTag)
    {
        if (string.IsNullOrWhiteSpace(rawTag))
        {
            throw new ArgumentException(EmptyTagErrorMessage, nameof(rawTag));
        }

        var rawSegments = rawTag.Split(GroupSeparators, StringSplitOptions.RemoveEmptyEntries);
        var normalizedSegments = rawSegments
            .Select(NormalizeSegment)
            .Where(segment => !string.IsNullOrWhiteSpace(segment))
            .ToArray();

        if (normalizedSegments.Length == 0)
        {
            throw new ArgumentException(MissingSegmentErrorMessage, nameof(rawTag));
        }

        return new Group(normalizedSegments);
    }

    public IReadOnlyList<Group> PruneAncestors(IEnumerable<Group> groups)
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