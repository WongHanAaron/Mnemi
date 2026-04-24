using System.Collections.Generic;
using Mnemi.Domain.Entities;

namespace Mnemi.Domain.Parsing;

public interface ICardTypeParser
{
    bool CanParse(string trimmedLine);

    int Parse(Document document, string[] lines, int startIndex, List<Card> cards);
}