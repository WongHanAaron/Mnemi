using System;
using System.Collections.Generic;
using Mnemi.Domain.Entities;

namespace Mnemi.Domain.Parsing;

public abstract class CardTypeParser : ICardTypeParser
{
    protected readonly IMetadataParser MetadataParser;
    protected readonly ICardParserUtilities CardParserUtilities;

    protected CardTypeParser(IMetadataParser metadataParser, ICardParserUtilities cardParserUtilities)
    {
        MetadataParser = metadataParser ?? throw new ArgumentNullException(nameof(metadataParser));
        CardParserUtilities = cardParserUtilities ?? throw new ArgumentNullException(nameof(cardParserUtilities));
    }

    public abstract bool CanParse(string trimmedLine);

    public abstract int Parse(Document document, string[] lines, int startIndex, List<Card> cards);
}
