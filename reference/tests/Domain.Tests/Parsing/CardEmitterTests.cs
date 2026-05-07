using FluentAssertions;
using Mnemi.Domain.Entities;
using Mnemi.Domain.Parsing;
using DomainFile = Mnemi.Domain.Entities.File;
using Xunit;

namespace Mnemi.Domain.Tests.Parsing;

public class CardEmitterTests
{
    [Fact]
    public void CardEmitter_emits_domain_qa_card_from_normalized_card()
    {
        var emitter = new CardEmitter();
        var file = new DomainFile("deck.md", "notes/deck.md", null, null, string.Empty);
        var document = new Document(file, string.Empty, Array.Empty<Group>());
        var lowered = new[]
        {
            new NormalizedCard(
                CardType.Qa,
                "abc123",
                "raw",
                "question",
                "answer",
                null,
                null,
                Array.Empty<Group>(),
                LearningState.New,
                1,
                1)
        };

        var cards = emitter.Emit(document, lowered);

        cards.Should().ContainSingle();
        var card = cards[0].Should().BeOfType<QaCard>().Subject;
        card.Question.Should().Be("question");
        card.Answer.Should().Be("answer");
    }
}
