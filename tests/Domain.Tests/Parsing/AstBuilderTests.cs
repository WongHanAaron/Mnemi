using FluentAssertions;
using Mnemi.Domain.Parsing;
using Xunit;

namespace Mnemi.Domain.Tests.Parsing;

public class AstBuilderTests
{
    [Fact]
    public void AstBuilder_builds_ast_from_syntax_nodes()
    {
        var builder = new AstBuilder();
        var syntax = new DslDocumentSyntax(new DslSyntaxNode[] { new NoteSyntaxNode(1, "note") });

        var ast = builder.Build(syntax);

        ast.Nodes.Should().HaveCount(1);
        ast.Nodes[0].Should().BeOfType<NoteSyntaxNode>();
    }
}
