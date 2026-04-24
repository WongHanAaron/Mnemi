using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Mnemi.Domain.Parsing;

public enum TokenKind
{
    Empty,
    DocumentTag,
    MetadataComment,
    QaBlockStart,
    ClozeBlockStart,
    McqBlockStart,
    BlockClosingMarker,
    InlineCandidate,
    Text
}

public sealed record SyntaxToken(TokenKind Kind, int LineNumber, string RawText, string TrimmedText);

public interface IMarkdownScanner
{
    IReadOnlyList<SyntaxToken> Scan(SourceDocument source);
}

public sealed class MarkdownScanner : IMarkdownScanner
{
    private static readonly Regex DocumentTagPattern = new("^#(?<tag>[A-Za-z0-9_/]+)$", RegexOptions.Compiled);

    public IReadOnlyList<SyntaxToken> Scan(SourceDocument source)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        var tokens = new List<SyntaxToken>(source.Lines.Count);
        foreach (var line in source.Lines)
        {
            tokens.Add(new SyntaxToken(Classify(line.TrimmedText), line.LineNumber, line.RawText, line.TrimmedText));
        }

        return tokens;
    }

    private static TokenKind Classify(string trimmed)
    {
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return TokenKind.Empty;
        }

        if (trimmed.StartsWith(CardParsingConstants.MetadataCommentPrefix, StringComparison.Ordinal) &&
            trimmed.Contains("mnemi:", StringComparison.OrdinalIgnoreCase))
        {
            return TokenKind.MetadataComment;
        }

        if (trimmed.StartsWith(CardParsingConstants.QaBlockStart, StringComparison.OrdinalIgnoreCase))
        {
            return TokenKind.QaBlockStart;
        }

        if (trimmed.StartsWith(CardParsingConstants.ClozeBlockStart, StringComparison.OrdinalIgnoreCase))
        {
            return TokenKind.ClozeBlockStart;
        }

        if (trimmed.StartsWith(CardParsingConstants.McqBlockStart, StringComparison.OrdinalIgnoreCase))
        {
            return TokenKind.McqBlockStart;
        }

        if (trimmed.Equals(CardParsingConstants.BlockClosingMarker, StringComparison.OrdinalIgnoreCase))
        {
            return TokenKind.BlockClosingMarker;
        }

        if (DocumentTagPattern.IsMatch(trimmed))
        {
            return TokenKind.DocumentTag;
        }

        if (trimmed.Contains(CardParsingConstants.CardSeparator, StringComparison.Ordinal))
        {
            return TokenKind.InlineCandidate;
        }

        return TokenKind.Text;
    }
}
