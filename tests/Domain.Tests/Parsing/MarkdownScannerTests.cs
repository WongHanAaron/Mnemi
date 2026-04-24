using FluentAssertions;
using Mnemi.Domain.Parsing;
using Xunit;

namespace Mnemi.Domain.Tests.Parsing;

public class MarkdownScannerTests
{
    [Fact]
    public void MarkdownScanner_classifies_markdown_lines_into_expected_token_kinds()
    {
        var reader = new SourceReader();
        var scanner = new MarkdownScanner();

        var source = reader.Read("""
#spanish
#!qa
question
::
answer
#!
<!-- mnemi: abc | status=review -->
What::ever
""");

        var tokens = scanner.Scan(source);

        tokens.Should().Contain(token => token.Kind == TokenKind.DocumentTag);
        tokens.Should().Contain(token => token.Kind == TokenKind.QaBlockStart);
        tokens.Should().Contain(token => token.Kind == TokenKind.BlockClosingMarker);
        tokens.Should().Contain(token => token.Kind == TokenKind.MetadataComment);
        tokens.Should().Contain(token => token.Kind == TokenKind.InlineCandidate);
    }
}
