using FluentAssertions;
using Moq;
using Mnemi.Domain.Entities;
using Mnemi.Domain.Parsing;
using DomainFile = Mnemi.Domain.Entities.File;
using Xunit;

namespace Mnemi.Domain.Tests.Parsing;

public class SemanticBinderTests
{
    [Fact]
    public void SemanticBinder_attaches_closest_following_metadata_to_question()
    {
        var metadata = new MetadataResult(new LearningState("h", "review", null, null, null, null, null, null, null), Array.Empty<Group>());
        var metadataParserMock = new Mock<IMetadataParser>();
        metadataParserMock
            .Setup(parser => parser.TryParseComment("<!-- mnemi: h | status=review -->", out metadata))
            .Returns(true);

        var binder = new SemanticBinder(metadataParserMock.Object);
        var file = new DomainFile("deck.md", "notes/deck.md", null, null, string.Empty);
        var document = new Document(file, string.Empty, Array.Empty<Group>());
        var ast = new DslAstDocument(new DslSyntaxNode[]
        {
            new InlineCardSyntaxNode(1, "Q::A"),
            new EmptyLineSyntaxNode(2),
            new MetadataCommentSyntaxNode(3, "<!-- mnemi: h | status=review -->")
        });

        var bound = binder.Bind(document, ast);

        bound.Questions.Should().ContainSingle();
        bound.Questions[0].Metadata.LearningState.Status.Should().Be("review");
    }
}
