using System;
using System.Collections.Generic;

namespace Mnemi.Domain.Parsing;

public sealed record DslAstDocument(IReadOnlyList<DslSyntaxNode> Nodes);

public interface IAstBuilder
{
    DslAstDocument Build(DslDocumentSyntax syntax);
}

public sealed class AstBuilder : IAstBuilder
{
    public DslAstDocument Build(DslDocumentSyntax syntax)
    {
        if (syntax == null)
        {
            throw new ArgumentNullException(nameof(syntax));
        }

        return new DslAstDocument(syntax.Nodes);
    }
}
