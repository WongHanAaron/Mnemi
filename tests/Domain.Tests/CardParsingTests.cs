using System;
using System.Linq;
using DomainFile = Mnemi.Domain.Cards.File;
using Mnemi.Domain.Cards;
using Xunit;

namespace Mnemi.Domain.Tests;

public class CardParsingTests
{
    [Fact]
    public void DocumentParser_ParsesTopLevelDocumentTags()
    {
        var file = new DomainFile(
            "deck.md",
            "notes/deck.md",
            null,
            null,
            "#spanish\r\n#spanish/verbs\r\n\r\nWhat is \"hablar\"?::to speak\r\n");

        var documentParser = new DocumentParser();
        var document = documentParser.Parse(file);

        Assert.Single(document.DocumentTags);
        Assert.Equal("Spanish::Verbs", document.DocumentTags[0].DisplayPath);
    }

    [Fact]
    public void CardParser_ParsesSingleLineQaWithDocumentTags()
    {
        var file = new DomainFile(
            "deck.md",
            "notes/deck.md",
            null,
            null,
            "#spanish\r\n#spanish/verbs\r\n\r\nWhat is \"hablar\"?::to speak\r\n<!-- mnemi: 7f4a2e | status=review -->\r\n");

        var document = new DocumentParser().Parse(file);
        var cards = new CardParser().Parse(document);

        Assert.Single(cards);
        var card = Assert.IsType<QaCard>(cards[0]);
        Assert.Equal("What is \"hablar\"?", card.Question);
        Assert.Equal("to speak", card.Answer);
        Assert.Equal("notes/deck.md", card.Source);
        Assert.Equal("review", card.LearningState?.Status);
        Assert.Single(card.Groups);
        Assert.Equal("Spanish::Verbs", card.Groups[0].DisplayPath);
    }

    [Fact]
    public void CardParser_ParsesInlineClozeWithMultipleChoiceOptions()
    {
        var file = new DomainFile(
            "deck.md",
            "notes/deck.md",
            null,
            null,
            "The capital of France is {{Washington|**Paris**}}::\r\n");

        var document = new DocumentParser().Parse(file);
        var cards = new CardParser().Parse(document);

        Assert.Single(cards);
        var card = Assert.IsType<ClozeCard>(cards[0]);
        Assert.Contains("{{Washington|**Paris**}}", card.QuestionText);
        Assert.Single(card.ClozeBlanks);
        var blank = card.ClozeBlanks[0];
        Assert.Equal(2, blank.Options.Count);
        Assert.Contains(blank.Options, option => option.Text == "Paris" && option.IsAccepted);
        Assert.Contains(blank.Options, option => option.Text == "Washington" && !option.IsAccepted);
    }

    [Fact]
    public void CardParser_AppliesCardLevelTagOverride()
    {
        var file = new DomainFile(
            "deck.md",
            "notes/deck.md",
            null,
            null,
            "#spanish\r\n#spanish/verbs\r\n\r\nWhat is \"ser\"?::to be\r\n<!-- mnemi: abc123 | status=new | tag=Spanish::Verbs::Irregular -->\r\n");

        var document = new DocumentParser().Parse(file);
        var cards = new CardParser().Parse(document);

        Assert.Single(cards);
        var card = Assert.IsType<QaCard>(cards[0]);
        Assert.Single(card.Groups);
        Assert.Equal("Spanish::Verbs::Irregular", card.Groups[0].DisplayPath);
    }
}
