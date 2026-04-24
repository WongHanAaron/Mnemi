using System;
using System.Collections.Generic;

namespace Mnemi.Domain.Cards;

internal abstract class CardTypeParser
{
    protected readonly MetadataParser MetadataParser;

    protected CardTypeParser(MetadataParser metadataParser)
    {
        MetadataParser = metadataParser ?? throw new ArgumentNullException(nameof(metadataParser));
    }

    public abstract bool CanParse(string trimmedLine);

    public abstract int Parse(Document document, string[] lines, int startIndex, List<Card> cards);
}
