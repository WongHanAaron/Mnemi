using System;
using System.Collections.Generic;
using System.Linq;
using Mnemi.Domain.Entities;

namespace Mnemi.Domain.Parsing;

public sealed class ClozeCardParser : CardTypeParser
{
    public ClozeCardParser(IMetadataParser metadataParser, ICardParserUtilities cardParserUtilities)
        : base(metadataParser, cardParserUtilities)
    {
    }

    public override bool CanParse(string trimmedLine)
    {
        return trimmedLine.StartsWith(CardParsingConstants.ClozeBlockStart, StringComparison.OrdinalIgnoreCase);
    }

    public override int Parse(Document document, string[] lines, int startIndex, List<Card> cards)
    {
        var endIndex = CardParserUtilities.FindClosingMarker(lines, startIndex);
        if (endIndex < 0)
        {
            return startIndex;
        }

        var blockLines = lines[(startIndex + 1)..endIndex];
        var separator = Array.FindIndex(blockLines, line => line.Trim() == CardParsingConstants.CardSeparator);
        if (separator < 0)
        {
            return endIndex;
        }

        var questionText = string.Join(Environment.NewLine, blockLines[..separator]).Trim();
        var answerSection = string.Join(Environment.NewLine, blockLines[(separator + 1)..]).Trim();
        var rawContent = string.Join(Environment.NewLine, blockLines).TrimEnd();
        var blanks = CardParserUtilities.ExtractClozeBlanks(questionText, answerSection);
        if (!blanks.Any())
        {
            return endIndex;
        }

        var metadataIndex = MetadataParser.FindMetadataLine(lines, endIndex + 1, out var metadata);
        var groups = metadata.TagOverride.Any() ? metadata.TagOverride : document.DocumentTags;

        cards.Add(new ClozeCard(
            questionText,
            blanks,
            CardParserUtilities.ComputeHash(rawContent),
            groups,
            metadata.LearningState,
            rawContent,
            document.File.RelativePath,
            startIndex + 1,
            endIndex + 1));

        return metadataIndex >= 0 ? metadataIndex : endIndex;
    }
}
