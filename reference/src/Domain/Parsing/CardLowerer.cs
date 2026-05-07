using System;
using System.Collections.Generic;
using System.Linq;
using Mnemi.Domain.Entities;
using EntityGroup = Mnemi.Domain.Entities.Group;

namespace Mnemi.Domain.Parsing;

public sealed record NormalizedCard(
    CardType Type,
    string Id,
    string RawContent,
    string? Question,
    string? Answer,
    IReadOnlyList<ClozeBlank>? ClozeBlanks,
    IReadOnlyList<MultipleChoiceOption>? Options,
    IReadOnlyList<EntityGroup> Groups,
    LearningState? LearningState,
    int SourceLineStart,
    int SourceLineEnd);

public interface ICardLowerer
{
    IReadOnlyList<NormalizedCard> Lower(BoundDocument boundDocument);
}

public sealed class CardLowerer : ICardLowerer
{
    private readonly ICardParserUtilities _cardParserUtilities;

    public CardLowerer(ICardParserUtilities cardParserUtilities)
    {
        _cardParserUtilities = cardParserUtilities ?? throw new ArgumentNullException(nameof(cardParserUtilities));
    }

    public IReadOnlyList<NormalizedCard> Lower(BoundDocument boundDocument)
    {
        if (boundDocument == null)
        {
            throw new ArgumentNullException(nameof(boundDocument));
        }

        var cards = new List<NormalizedCard>();

        foreach (var question in boundDocument.Questions)
        {
            var groups = question.Metadata.TagOverride.Any() ? question.Metadata.TagOverride : boundDocument.DocumentTags;

            switch (question.Question)
            {
                case InlineCardSyntaxNode inline:
                    LowerInline(inline, question.Metadata.LearningState, groups, cards);
                    break;
                case BlockCardSyntaxNode block:
                    LowerBlock(block, question.Metadata.LearningState, groups, cards);
                    break;
            }
        }

        return cards;
    }

    private void LowerInline(InlineCardSyntaxNode inline, LearningState learningState, IReadOnlyList<EntityGroup> groups, List<NormalizedCard> cards)
    {
        var splitIndex = inline.Text.IndexOf(CardParsingConstants.CardSeparator, StringComparison.Ordinal);
        if (splitIndex < 0)
        {
            return;
        }

        var question = inline.Text[..splitIndex].Trim();
        var answer = inline.Text[(splitIndex + 2)..].Trim();
        var rawContent = inline.Text.TrimEnd();
        var id = _cardParserUtilities.ComputeHash(rawContent);

        if (question.Contains(CardParsingConstants.ClozeOpen, StringComparison.Ordinal) &&
            question.Contains(CardParsingConstants.ClozeClose, StringComparison.Ordinal))
        {
            var blanks = _cardParserUtilities.ExtractClozeBlanks(question, answer);
            if (!blanks.Any())
            {
                return;
            }

            cards.Add(new NormalizedCard(CardType.Cloze, id, rawContent, question, null, blanks, null, groups, learningState, inline.LineNumber, inline.LineNumber));
            return;
        }

        if (answer.Contains(CardParsingConstants.MultipleChoiceDelimiter, StringComparison.Ordinal))
        {
            var options = _cardParserUtilities.ExtractMultipleChoiceOptions(answer.Split(CardParsingConstants.MultipleChoiceDelimiter, StringSplitOptions.RemoveEmptyEntries));
            if (options.Any())
            {
                cards.Add(new NormalizedCard(CardType.MultipleChoice, id, rawContent, question, null, null, options, groups, learningState, inline.LineNumber, inline.LineNumber));
                return;
            }
        }

        cards.Add(new NormalizedCard(CardType.Qa, id, rawContent, question, answer, null, null, groups, learningState, inline.LineNumber, inline.LineNumber));
    }

    private void LowerBlock(BlockCardSyntaxNode block, LearningState learningState, IReadOnlyList<EntityGroup> groups, List<NormalizedCard> cards)
    {
        var separator = Array.FindIndex(block.BodyLines, line => line.Trim() == CardParsingConstants.CardSeparator);
        if (separator < 0)
        {
            return;
        }

        var question = string.Join(Environment.NewLine, block.BodyLines[..separator]).Trim();
        var answer = string.Join(Environment.NewLine, block.BodyLines[(separator + 1)..]).Trim();
        var rawContent = string.Join(Environment.NewLine, block.BodyLines).TrimEnd();
        var id = _cardParserUtilities.ComputeHash(rawContent);

        switch (block.BlockType)
        {
            case CardType.Qa:
                cards.Add(new NormalizedCard(CardType.Qa, id, rawContent, question, answer, null, null, groups, learningState, block.LineNumber, block.EndLineNumber));
                break;
            case CardType.Cloze:
                var blanks = _cardParserUtilities.ExtractClozeBlanks(question, answer);
                if (blanks.Any())
                {
                    cards.Add(new NormalizedCard(CardType.Cloze, id, rawContent, question, null, blanks, null, groups, learningState, block.LineNumber, block.EndLineNumber));
                }
                break;
            case CardType.MultipleChoice:
                var options = _cardParserUtilities.ExtractMultipleChoiceOptions(block.BodyLines[(separator + 1)..]);
                if (options.Any())
                {
                    cards.Add(new NormalizedCard(CardType.MultipleChoice, id, rawContent, question, null, null, options, groups, learningState, block.LineNumber, block.EndLineNumber));
                }
                break;
        }
    }
}
