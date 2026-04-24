using FluentAssertions;
using Mnemi.Domain.Entities;
using Mnemi.Domain.Parsing;
using Xunit;

namespace Mnemi.Domain.Tests.Parsing;

public class CardLowererTests
{
    [Fact]
    public void CardLowerer_normalizes_inline_qa_to_canonical_card_model()
    {
        var lowerer = new CardLowerer(new CardParserUtilities());
        var bound = new BoundDocument(
            new[]
            {
                new BoundQuestion(
                    new InlineCardSyntaxNode(1, "What is hablar?::to speak"),
                    new MetadataResult(LearningState.New, Array.Empty<Group>()))
            },
            new[] { new Group(new[] { "Spanish" }) });

        var lowered = lowerer.Lower(bound);

        lowered.Should().ContainSingle();
        lowered[0].Type.Should().Be(CardType.Qa);
        lowered[0].Question.Should().Be("What is hablar?");
        lowered[0].Answer.Should().Be("to speak");
    }
}
