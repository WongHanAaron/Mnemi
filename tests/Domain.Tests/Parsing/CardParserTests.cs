using System.Linq;
using FluentAssertions;
using Moq;
using Mnemi.Domain.Entities;
using Mnemi.Domain.Parsing;
using DomainFile = Mnemi.Domain.Entities.File;
using Xunit;

namespace Mnemi.Domain.Tests.Parsing;

public class CardParserTests
{
    private static IDocumentParser CreateDocumentParser()
    {
        return new DocumentParser(new GroupParser());
    }

    private static ICardParser CreateCardParser()
    {
        var groupParser = new GroupParser();
        var metadataParser = new MetadataParser(groupParser);
        var cardParserUtilities = new CardParserUtilities();
        var parsers = new ICardTypeParser[]
        {
            new QaCardParser(metadataParser, cardParserUtilities),
            new ClozeCardParser(metadataParser, cardParserUtilities),
            new McqCardParser(metadataParser, cardParserUtilities),
            new InlineCardParser(metadataParser, cardParserUtilities)
        };

        return new CardParser(parsers, cardParserUtilities);
    }

    [Fact]
    public void CardParser_parses_single_line_qa_with_document_tags()
    {
        var file = new DomainFile(
            "deck.md",
            "notes/deck.md",
            null,
            null,
            """
#spanish
#spanish/verbs

What is "hablar"?::to speak
<!-- mnemi: 7f4a2e | status=review -->
""");

        var document = CreateDocumentParser().Parse(file);
        var cards = CreateCardParser().Parse(document);

        cards.Should().ContainSingle();
        var card = cards[0].Should().BeOfType<QaCard>().Subject;
        card.Question.Should().Be("What is \"hablar\"?");
        card.Answer.Should().Be("to speak");
        card.Source.Should().Be("notes/deck.md");
        card.LearningState?.Status.Should().Be("review");
        card.Groups.Should().ContainSingle();
        card.Groups[0].DisplayPath.Should().Be("Spanish::Verbs");
    }

    [Fact]
    public void CardParser_parses_inline_cloze_with_multiple_choice_options()
    {
        var file = new DomainFile(
            "deck.md",
            "notes/deck.md",
            null,
            null,
            """
The capital of France is {{Washington|**Paris**}}::
""");

        var document = CreateDocumentParser().Parse(file);
        var cards = CreateCardParser().Parse(document);

        cards.Should().ContainSingle();
        var card = cards[0].Should().BeOfType<ClozeCard>().Subject;
        card.QuestionText.Should().Contain("{{Washington|**Paris**}}");
        card.ClozeBlanks.Should().ContainSingle();
        var blank = card.ClozeBlanks[0];
        blank.Options.Should().HaveCount(2);
        blank.Options.Should().Contain(option => option.Text == "Paris" && option.IsAccepted);
        blank.Options.Should().Contain(option => option.Text == "Washington" && !option.IsAccepted);
    }

    [Fact]
    public void CardParser_applies_card_level_tag_override()
    {
        var file = new DomainFile(
            "deck.md",
            "notes/deck.md",
            null,
            null,
            """
#spanish
#spanish/verbs

What is "ser"?::to be
<!-- mnemi: abc123 | status=new | tag=Spanish::Verbs::Irregular -->
""");

        var document = CreateDocumentParser().Parse(file);
        var cards = CreateCardParser().Parse(document);

        cards.Should().ContainSingle();
        var card = cards[0].Should().BeOfType<QaCard>().Subject;
        card.Groups.Should().ContainSingle();
        card.Groups[0].DisplayPath.Should().Be("Spanish::Verbs::Irregular");
    }

    [Fact]
    public void CardParser_uses_first_matching_parser_and_skips_comment_lines()
    {
        var file = new DomainFile("deck.md", "notes/deck.md", null, null, "ignored");
        var document = new Document(file, file.FileContents, Array.Empty<Group>());

        var parserOneMock = new Mock<ICardTypeParser>();
        parserOneMock.Setup(parser => parser.CanParse(It.IsAny<string>())).Returns(false);

        var parserTwoMock = new Mock<ICardTypeParser>();
        parserTwoMock.Setup(parser => parser.CanParse("actual-line")).Returns(true);
        parserTwoMock
            .Setup(parser => parser.Parse(document, It.IsAny<string[]>(), 1, It.IsAny<List<Card>>()))
            .Returns(1);

        var parserUtilitiesMock = new Mock<ICardParserUtilities>();
        parserUtilitiesMock
            .Setup(utilities => utilities.SplitLines(document.Content))
            .Returns(new[] { "<!-- ignored -->", "actual-line" });

        var parser = new CardParser(new[] { parserOneMock.Object, parserTwoMock.Object }, parserUtilitiesMock.Object);

        var cards = parser.Parse(document);

        cards.Should().BeEmpty();
        parserUtilitiesMock.Verify(utilities => utilities.SplitLines(document.Content), Times.Once);
        parserOneMock.Verify(mock => mock.CanParse("actual-line"), Times.Once);
        parserTwoMock.Verify(mock => mock.CanParse("actual-line"), Times.Once);
        parserTwoMock.Verify(mock => mock.Parse(document, It.IsAny<string[]>(), 1, It.IsAny<List<Card>>()), Times.Once);
    }
}