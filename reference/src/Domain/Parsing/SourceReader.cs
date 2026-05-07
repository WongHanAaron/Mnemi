using System;
using System.Collections.Generic;

namespace Mnemi.Domain.Parsing;

public sealed record SourceLine(int LineNumber, string RawText, string TrimmedText);

public sealed record SourceDocument(string OriginalContent, IReadOnlyList<SourceLine> Lines);

public interface ISourceReader
{
    SourceDocument Read(string content);
}

public sealed class SourceReader : ISourceReader
{
    public SourceDocument Read(string content)
    {
        var normalized = Normalize(content);
        var split = normalized.Split(new[] { CardParsingConstants.CrLf, CardParsingConstants.Lf }, StringSplitOptions.None);
        var lines = new List<SourceLine>(split.Length);

        for (var index = 0; index < split.Length; index++)
        {
            lines.Add(new SourceLine(index + 1, split[index], split[index].Trim()));
        }

        return new SourceDocument(normalized, lines);
    }

    private static string Normalize(string? content)
    {
        if (string.IsNullOrEmpty(content))
        {
            return string.Empty;
        }

        // Remove UTF-8 BOM when present.
        if (content.Length > 0 && content[0] == '\uFEFF')
        {
            content = content[1..];
        }

        return content;
    }
}
