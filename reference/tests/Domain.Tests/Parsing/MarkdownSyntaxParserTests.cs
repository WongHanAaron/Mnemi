using FluentAssertions;
using Mnemi.Domain.Entities;
using Mnemi.Domain.Parsing;
using Xunit;

namespace Mnemi.Domain.Tests.Parsing;

public class MarkdownSyntaxParserTests
{
    [Fact]
    public void MarkdownSyntaxParser_parses_block_and_inline_nodes()
    {
        var parser = new MarkdownSyntaxParser();
        var tokens = new SyntaxToken[]
        {
            new(TokenKind.QaBlockStart, 1, "#!qa", "#!qa"),
            new(TokenKind.Text, 2, "Q", "Q"),
            new(TokenKind.InlineCandidate, 3, "::", "::"),
            new(TokenKind.Text, 4, "A", "A"),
            new(TokenKind.BlockClosingMarker, 5, "#!", "#!"),
            new(TokenKind.MetadataComment, 6, "<!-- mnemi: h -->", "<!-- mnemi: h -->"),
            new(TokenKind.InlineCandidate, 7, "X::Y", "X::Y")
        };

        var syntax = parser.Parse(tokens);

        syntax.Nodes.Should().ContainSingle(node => node is BlockCardSyntaxNode);
        syntax.Nodes.Should().ContainSingle(node => node is MetadataCommentSyntaxNode);
        syntax.Nodes.Should().ContainSingle(node => node is InlineCardSyntaxNode);

        var block = syntax.Nodes[0].Should().BeOfType<BlockCardSyntaxNode>().Subject;
        block.BlockType.Should().Be(CardType.Qa);
        block.LineNumber.Should().Be(1);
        block.EndLineNumber.Should().Be(5);
    }
}
