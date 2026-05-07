using System;
using System.Collections.Generic;
using Mnemi.Domain.Entities;
using EntityGroup = Mnemi.Domain.Entities.Group;

namespace Mnemi.Domain.Parsing;

public sealed record BoundQuestion(QuestionSyntaxNode Question, MetadataResult Metadata);

public sealed record BoundDocument(IReadOnlyList<BoundQuestion> Questions, IReadOnlyList<EntityGroup> DocumentTags);

public interface ISemanticBinder
{
    BoundDocument Bind(Document document, DslAstDocument ast);
}

public sealed class SemanticBinder : ISemanticBinder
{
    private readonly IMetadataParser _metadataParser;

    public SemanticBinder(IMetadataParser metadataParser)
    {
        _metadataParser = metadataParser ?? throw new ArgumentNullException(nameof(metadataParser));
    }

    public BoundDocument Bind(Document document, DslAstDocument ast)
    {
        if (document == null)
        {
            throw new ArgumentNullException(nameof(document));
        }

        if (ast == null)
        {
            throw new ArgumentNullException(nameof(ast));
        }

        var boundQuestions = new List<BoundQuestion>();

        for (var index = 0; index < ast.Nodes.Count; index++)
        {
            if (ast.Nodes[index] is not QuestionSyntaxNode question)
            {
                continue;
            }

            var metadata = ResolveFollowingMetadata(ast.Nodes, index);
            boundQuestions.Add(new BoundQuestion(question, metadata));
        }

        return new BoundDocument(boundQuestions, document.DocumentTags);
    }

    private MetadataResult ResolveFollowingMetadata(IReadOnlyList<DslSyntaxNode> nodes, int questionIndex)
    {
        for (var index = questionIndex + 1; index < nodes.Count; index++)
        {
            var node = nodes[index];
            if (node is EmptyLineSyntaxNode)
            {
                continue;
            }

            if (node is MetadataCommentSyntaxNode metadataNode &&
                _metadataParser.TryParseComment(metadataNode.RawText, out var metadata))
            {
                return metadata;
            }

            break;
        }

        return new MetadataResult(LearningState.New, Array.Empty<EntityGroup>());
    }
}
