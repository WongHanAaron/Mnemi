using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Mnemi.Domain.Entities;

namespace Mnemi.Domain.Parsing;

public interface ICardParserUtilities
{
    int FindClosingMarker(string[] lines, int startIndex);

    string[] SplitLines(string content);

    IReadOnlyList<ClozeBlank> ExtractClozeBlanks(string questionText, string answerSection);

    IReadOnlyList<ClozeAnswerOption> ParseClozeOptions(string rawContent);

    IReadOnlyList<MultipleChoiceOption> ExtractMultipleChoiceOptions(IEnumerable<string> rawParts);

    IReadOnlyList<MultipleChoiceOption> ExtractMultipleChoiceOptions(string[] optionLines);

    string NormalizeOptionText(string text);

    string ComputeHash(string content);
}

public sealed class CardParserUtilities : ICardParserUtilities
{
    private const string ClozeStrongDelimiter = "**";
    private const string ClozeDelimiter = "*";
    private const string MarkerGroupName = "marker";
    private const string TextGroupName = "text";
    private const string CorrectMarker = "x";
    private const string HashByteSeparator = "-";
    private static readonly Regex ClozePlaceholderPattern = new(@"\{\{([^}]*)\}\}", RegexOptions.Compiled);
    private static readonly Regex McqOptionPattern = new(@"^-\s*\[(?<marker> |x|X)\]\s*(?<text>.*)$", RegexOptions.Compiled);

    public int FindClosingMarker(string[] lines, int startIndex)
    {
        for (var index = startIndex + 1; index < lines.Length; index++)
        {
            if (lines[index].Trim().Equals(CardParsingConstants.BlockClosingMarker, StringComparison.OrdinalIgnoreCase))
            {
                return index;
            }
        }

        return -1;
    }

    public string[] SplitLines(string content)
    {
        return content?.Split(new[] { CardParsingConstants.CrLf, CardParsingConstants.Lf }, StringSplitOptions.None)
            ?? Array.Empty<string>();
    }

    public IReadOnlyList<ClozeBlank> ExtractClozeBlanks(string questionText, string answerSection)
    {
        var placeholders = ClozePlaceholderPattern.Matches(questionText).Cast<Match>().ToArray();
        if (!placeholders.Any())
        {
            return Array.Empty<ClozeBlank>();
        }

        var answerMap = BuildAnswerMap(answerSection);
        var blanks = new List<ClozeBlank>();

        foreach (Match placeholder in placeholders)
        {
            var placeholderContent = placeholder.Groups[1].Value.Trim();
            var options = ParseClozeOptions(placeholderContent);
            if (answerMap.TryGetValue(placeholderContent, out var mappedAnswer))
            {
                options = ParseClozeOptions(mappedAnswer);
            }

            blanks.Add(new ClozeBlank(placeholder.Value, options));
        }

        return blanks;
    }

    private Dictionary<string, string> BuildAnswerMap(string answerSection)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(answerSection))
        {
            return map;
        }

        var lines = SplitLines(answerSection);
        string? currentKey = null;
        var currentText = new List<string>();

        foreach (var rawLine in lines)
        {
            var line = rawLine.TrimEnd();
            var separatorIndex = line.IndexOf(':');
            if (separatorIndex > 0)
            {
                if (currentKey != null)
                {
                    map[currentKey] = string.Join(Environment.NewLine, currentText).Trim();
                }

                currentKey = line[..separatorIndex].Trim();
                currentText.Clear();
                var remainder = line[(separatorIndex + 1)..].Trim();
                if (!string.IsNullOrEmpty(remainder))
                {
                    currentText.Add(remainder);
                }
                continue;
            }

            if (!string.IsNullOrWhiteSpace(line) && currentKey != null)
            {
                currentText.Add(line);
            }
        }

        if (currentKey != null)
        {
            map[currentKey] = string.Join(Environment.NewLine, currentText).Trim();
        }

        return map;
    }

    public IReadOnlyList<ClozeAnswerOption> ParseClozeOptions(string rawContent)
    {
        if (string.IsNullOrWhiteSpace(rawContent))
        {
            return Array.Empty<ClozeAnswerOption>();
        }

        var parts = rawContent.Split('|', StringSplitOptions.RemoveEmptyEntries);
        var options = new List<ClozeAnswerOption>();

        foreach (var part in parts)
        {
            var option = part.Trim();
            var isAccepted = false;

            if (option.StartsWith(ClozeStrongDelimiter, StringComparison.Ordinal) && option.EndsWith(ClozeStrongDelimiter, StringComparison.Ordinal))
            {
                isAccepted = true;
                option = option[2..^2].Trim();
            }
            else if (option.StartsWith(ClozeDelimiter, StringComparison.Ordinal) && option.EndsWith(ClozeDelimiter, StringComparison.Ordinal))
            {
                isAccepted = true;
                option = option[1..^1].Trim();
            }

            options.Add(new ClozeAnswerOption(option, isAccepted));
        }

        if (options.Count == 1 && !options[0].IsAccepted)
        {
            options[0] = new ClozeAnswerOption(options[0].Text, true);
        }

        return options;
    }

    public IReadOnlyList<MultipleChoiceOption> ExtractMultipleChoiceOptions(IEnumerable<string> rawParts)
    {
        return rawParts
            .Select(raw => raw.Trim())
            .Where(raw => !string.IsNullOrWhiteSpace(raw))
            .Select(raw =>
            {
                var normalized = raw.Trim();
                var isCorrect = false;

                if (normalized.StartsWith(ClozeDelimiter, StringComparison.Ordinal) && normalized.EndsWith(ClozeDelimiter, StringComparison.Ordinal))
                {
                    isCorrect = true;
                    normalized = normalized[1..^1].Trim();
                }

                return new MultipleChoiceOption(normalized, isCorrect);
            })
            .ToList();
    }

    public IReadOnlyList<MultipleChoiceOption> ExtractMultipleChoiceOptions(string[] optionLines)
    {
        var options = new List<MultipleChoiceOption>();
        MultipleChoiceOption? current = null;

        for (var index = 0; index < optionLines.Length; index++)
        {
            var line = optionLines[index];
            var trimmed = line.Trim();
            var match = McqOptionPattern.Match(trimmed);
            if (match.Success)
            {
                if (current != null)
                {
                    options.Add(current);
                }

                var marker = match.Groups[MarkerGroupName].Value;
                var text = match.Groups[TextGroupName].Value.Trim();
                current = new MultipleChoiceOption(NormalizeOptionText(text), string.Equals(marker, CorrectMarker, StringComparison.OrdinalIgnoreCase));
                continue;
            }

            if (current != null && (line.StartsWith(CardParsingConstants.Space, StringComparison.Ordinal) || line.StartsWith(CardParsingConstants.Tab, StringComparison.Ordinal)))
            {
                current = new MultipleChoiceOption(current.Text + CardParsingConstants.Space + line.Trim(), current.IsCorrect);
            }
        }

        if (current != null)
        {
            options.Add(current);
        }

        return options;
    }

    public string NormalizeOptionText(string text)
    {
        var trimmed = text.Trim();
        if (trimmed.StartsWith(ClozeDelimiter, StringComparison.Ordinal) && trimmed.EndsWith(ClozeDelimiter, StringComparison.Ordinal) && trimmed.Length > 1)
        {
            trimmed = trimmed[1..^1].Trim();
        }

        return trimmed;
    }

    public string ComputeHash(string content)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(content ?? string.Empty);
        var hashBytes = sha256.ComputeHash(bytes);
        var hex = BitConverter.ToString(hashBytes).Replace(HashByteSeparator, string.Empty, StringComparison.Ordinal).ToLowerInvariant();
        return hex[..8];
    }
}
