using System;
using System.Collections.Generic;
using Mnemi.Domain.Entities;

namespace Mnemi.Domain.Parsing;

public abstract record DslSyntaxNode(int LineNumber);

public abstract record QuestionSyntaxNode(int LineNumber) : DslSyntaxNode(LineNumber);

public sealed record EmptyLineSyntaxNode(int LineNumber) : DslSyntaxNode(LineNumber);

public sealed record MetadataCommentSyntaxNode(int LineNumber, string RawText) : DslSyntaxNode(LineNumber);

public sealed record DocumentTagSyntaxNode(int LineNumber, string TagText) : DslSyntaxNode(LineNumber);

public sealed record NoteSyntaxNode(int LineNumber, string Text) : DslSyntaxNode(LineNumber);

public sealed record InlineCardSyntaxNode(int LineNumber, string Text) : QuestionSyntaxNode(LineNumber);

public sealed record BlockCardSyntaxNode(int LineNumber, int EndLineNumber, CardType BlockType, string[] BodyLines) : QuestionSyntaxNode(LineNumber);

public sealed record DslDocumentSyntax(IReadOnlyList<DslSyntaxNode> Nodes);

public interface IMarkdownSyntaxParser
{
    DslDocumentSyntax Parse(IReadOnlyList<SyntaxToken> tokens);
}

public sealed class MarkdownSyntaxParser : IMarkdownSyntaxParser
{
    public DslDocumentSyntax Parse(IReadOnlyList<SyntaxToken> tokens)
    {
        if (tokens == null)
        {
            throw new ArgumentNullException(nameof(tokens));
        }

        var nodes = new List<DslSyntaxNode>();

        for (var index = 0; index < tokens.Count; index++)
        {
            var token = tokens[index];

            switch (token.Kind)
            {
                case TokenKind.Empty:
                    nodes.Add(new EmptyLineSyntaxNode(token.LineNumber));
                    break;
                case TokenKind.DocumentTag:
                    nodes.Add(new DocumentTagSyntaxNode(token.LineNumber, token.TrimmedText));
                    break;
                case TokenKind.MetadataComment:
                    nodes.Add(new MetadataCommentSyntaxNode(token.LineNumber, token.TrimmedText));
                    break;
                case TokenKind.InlineCandidate:
                    nodes.Add(new InlineCardSyntaxNode(token.LineNumber, token.RawText));
                    break;
                case TokenKind.QaBlockStart:
                case TokenKind.ClozeBlockStart:
                case TokenKind.McqBlockStart:
                    nodes.Add(ParseBlock(tokens, ref index));
                    break;
                default:
                    nodes.Add(new NoteSyntaxNode(token.LineNumber, token.RawText));
                    break;
            }
        }

        return new DslDocumentSyntax(nodes);
    }

    private static BlockCardSyntaxNode ParseBlock(IReadOnlyList<SyntaxToken> tokens, ref int startIndex)
    {
        var start = tokens[startIndex];
        var type = start.Kind switch
        {
            TokenKind.QaBlockStart => CardType.Qa,
            TokenKind.ClozeBlockStart => CardType.Cloze,
            TokenKind.McqBlockStart => CardType.MultipleChoice,
            _ => CardType.Qa
        };

        var body = new List<string>();
        var endLine = start.LineNumber;

        for (var index = startIndex + 1; index < tokens.Count; index++)
        {
            var token = tokens[index];
            if (token.Kind == TokenKind.BlockClosingMarker)
            {
                endLine = token.LineNumber;
                startIndex = index;
                return new BlockCardSyntaxNode(start.LineNumber, endLine, type, body.ToArray());
            }

            body.Add(token.RawText);
        }

        // Unterminated blocks are preserved as best-effort syntax with end set to start.
        return new BlockCardSyntaxNode(start.LineNumber, endLine, type, body.ToArray());
    }
}
