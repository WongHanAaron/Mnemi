using System;
using System.Collections.Generic;

namespace Mnemi.Domain.Cards;

public class CardParser
{
    private readonly IReadOnlyList<CardTypeParser> _parsers;

    public CardParser()
    {
        var metadataParser = new MetadataParser();
        _parsers = new CardTypeParser[]
        {
            new QaCardParser(metadataParser),
            new ClozeCardParser(metadataParser),
            new McqCardParser(metadataParser),
            new InlineCardParser(metadataParser)
        };
    }

    public IReadOnlyList<Card> Parse(Document document)
    {
        if (document == null)
        {
            throw new ArgumentNullException(nameof(document));
        }

        var cards = new List<Card>();
        var lines = CardParserUtilities.SplitLines(document.Content);

        for (var index = 0; index < lines.Length; index++)
        {
            var trimmed = lines[index].Trim();
            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("<!--", StringComparison.Ordinal))
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
