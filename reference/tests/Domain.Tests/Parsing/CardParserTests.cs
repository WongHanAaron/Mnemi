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
    private static IDocumentParser CreateDocumentParser() => new DocumentParser(new GroupParser());

    private static ICardParser CreateCardParser() => new CardParser();

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
    public void CardParser_runs_dsl_pipeline_stages_in_order()
    {
        var file = new DomainFile("deck.md", "notes/deck.md", null, null, "source");
        var document = new Document(file, file.FileContents, Array.Empty<Group>());

        var source = new SourceDocument(document.Content, new[] { new SourceLine(1, "source", "source") });
        var tokens = new[] { new SyntaxToken(TokenKind.Text, 1, "source", "source") };
        var syntax = new DslDocumentSyntax(new[] { new NoteSyntaxNode(1, "source") });
        var ast = new DslAstDocument(syntax.Nodes);
        var bound = new BoundDocument(Array.Empty<BoundQuestion>(), document.DocumentTags);
        var diagnostics = new DiagnosticBag(Array.Empty<Diagnostic>());
        var lowered = new[]
        {
            new NormalizedCard(CardType.Qa, "id", "raw", "q", "a", null, null, document.DocumentTags, LearningState.New, 1, 1)
        };
        var emitted = new Card[]
        {
            new QaCard("q", "a", "id", document.DocumentTags, LearningState.New, "raw", file.RelativePath, 1, 1)
        };

        var readerMock = new Mock<ISourceReader>();
        readerMock.Setup(reader => reader.Read(document.Content)).Returns(source);

        var scannerMock = new Mock<IMarkdownScanner>();
        scannerMock.Setup(scanner => scanner.Scan(source)).Returns(tokens);

        var syntaxParserMock = new Mock<IMarkdownSyntaxParser>();
        syntaxParserMock.Setup(parser => parser.Parse(tokens)).Returns(syntax);

        var astBuilderMock = new Mock<IAstBuilder>();
        astBuilderMock.Setup(builder => builder.Build(syntax)).Returns(ast);

        var binderMock = new Mock<ISemanticBinder>();
        binderMock.Setup(binder => binder.Bind(document, ast)).Returns(bound);

        var validatorMock = new Mock<ISemanticValidator>();
        validatorMock.Setup(validator => validator.Validate(bound)).Returns(diagnostics);

        var lowererMock = new Mock<ICardLowerer>();
        lowererMock.Setup(lowerer => lowerer.Lower(bound)).Returns(lowered);

        var emitterMock = new Mock<ICardEmitter>();
        emitterMock.Setup(emitter => emitter.Emit(document, lowered)).Returns(emitted);

        var parser = new CardParser(
            readerMock.Object,
            scannerMock.Object,
            syntaxParserMock.Object,
            astBuilderMock.Object,
            binderMock.Object,
            validatorMock.Object,
            lowererMock.Object,
            emitterMock.Object);

        var cards = parser.Parse(document);

        cards.Should().ContainSingle();
        cards[0].Should().BeOfType<QaCard>();
        readerMock.Verify(reader => reader.Read(document.Content), Times.Once);
        scannerMock.Verify(scanner => scanner.Scan(source), Times.Once);
        syntaxParserMock.Verify(parser => parser.Parse(tokens), Times.Once);
        astBuilderMock.Verify(builder => builder.Build(syntax), Times.Once);
        binderMock.Verify(binder => binder.Bind(document, ast), Times.Once);
        validatorMock.Verify(validator => validator.Validate(bound), Times.Once);
        lowererMock.Verify(lowerer => lowerer.Lower(bound), Times.Once);
        emitterMock.Verify(emitter => emitter.Emit(document, lowered), Times.Once);
    }
}