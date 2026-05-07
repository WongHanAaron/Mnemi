using System;
using System.Collections.Generic;
using Mnemi.Domain.Entities;

namespace Mnemi.Domain.Parsing;

public interface ICardEmitter
{
    IReadOnlyList<Card> Emit(Document document, IReadOnlyList<NormalizedCard> loweredCards);
}

public sealed class CardEmitter : ICardEmitter
{
    public IReadOnlyList<Card> Emit(Document document, IReadOnlyList<NormalizedCard> loweredCards)
    {
        if (document == null)
        {
            throw new ArgumentNullException(nameof(document));
        }

        if (loweredCards == null)
        {
            throw new ArgumentNullException(nameof(loweredCards));
        }

        var cards = new List<Card>(loweredCards.Count);

        foreach (var lowered in loweredCards)
        {
            switch (lowered.Type)
            {
                case CardType.Qa:
                    cards.Add(new QaCard(
                        lowered.Question ?? string.Empty,
                        lowered.Answer ?? string.Empty,
                        lowered.Id,
                        lowered.Groups,
                        lowered.LearningState,
                        lowered.RawContent,
                        document.File.RelativePath,
                        lowered.SourceLineStart,
                        lowered.SourceLineEnd));
                    break;
                case CardType.Cloze:
                    cards.Add(new ClozeCard(
                        lowered.Question ?? string.Empty,
                        lowered.ClozeBlanks ?? Array.Empty<ClozeBlank>(),
                        lowered.Id,
                        lowered.Groups,
                        lowered.LearningState,
                        lowered.RawContent,
                        document.File.RelativePath,
                        lowered.SourceLineStart,
                        lowered.SourceLineEnd));
                    break;
                case CardType.MultipleChoice:
                    cards.Add(new MultipleChoiceCard(
                        lowered.Question ?? string.Empty,
                        lowered.Options ?? Array.Empty<MultipleChoiceOption>(),
                        lowered.Id,
                        lowered.Groups,
                        lowered.LearningState,
                        lowered.RawContent,
                        document.File.RelativePath,
                        lowered.SourceLineStart,
                        lowered.SourceLineEnd));
                    break;
            }
        }

        return cards;
    }
}
