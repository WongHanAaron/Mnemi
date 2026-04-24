using System;
using System.Collections.Generic;
using Mnemi.Domain.Entities;

namespace Mnemi.Domain.Parsing;

public interface ICardParser
{
    IReadOnlyList<Card> Parse(Document document);
}

public class CardParser : ICardParser
{
    private readonly IReadOnlyList<ICardTypeParser> _parsers;
    private readonly ICardParserUtilities _cardParserUtilities;

    public CardParser(IReadOnlyList<ICardTypeParser> parsers, ICardParserUtilities cardParserUtilities)
    {
        _parsers = parsers ?? throw new ArgumentNullException(nameof(parsers));
        _cardParserUtilities = cardParserUtilities ?? throw new ArgumentNullException(nameof(cardParserUtilities));
    }

    public IReadOnlyList<Card> Parse(Document document)
    {
        if (document == null)
        {
            throw new ArgumentNullException(nameof(document));
        }

        var cards = new List<Card>();
        var lines = _cardParserUtilities.SplitLines(document.Content);

        for (var index = 0; index < lines.Length; index++)
        {
            var trimmed = lines[index].Trim();
            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith(CardParsingConstants.MetadataCommentPrefix, StringComparison.Ordinal))
            {
                continue;
            }

            foreach (var parser in _parsers)
            {
                if (!parser.CanParse(trimmed))
                {
                    continue;
                }

                index = parser.Parse(document, lines, index, cards);
                break;
            }
        }

        return cards;
    }
}
