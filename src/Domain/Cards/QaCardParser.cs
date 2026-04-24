using System;
using System.Collections.Generic;
using System.Linq;

namespace Mnemi.Domain.Cards;

internal sealed class QaCardParser : CardTypeParser
{
    public QaCardParser(MetadataParser metadataParser)
        : base(metadataParser)
    {
    }

    public override bool CanParse(string trimmedLine)
    {
        return trimmedLine.StartsWith("#!qa", StringComparison.OrdinalIgnoreCase);
    }

    public override int Parse(Document document, string[] lines, int startIndex, List<Card> cards)
    {
        var endIndex = CardParserUtilities.FindClosingMarker(lines, startIndex);
        if (endIndex < 0)
        {
            return startIndex;
        }

        var blockLines = lines[(startIndex + 1)..endIndex];
        var separator = Array.FindIndex(blockLines, line => line.Trim() == "::");
        if (separator < 0)
        {
            return endIndex;
        }

        var question = string.Join(Environment.NewLine, blockLines[..separator]).Trim();
        var answer = string.Join(Environment.NewLine, blockLines[(separator + 1)..]).Trim();
        var rawContent = string.Join(Environment.NewLine, blockLines).TrimEnd();

        var metadataIndex = MetadataParser.FindMetadataLine(lines, endIndex + 1, out var metadata);
        var groups = metadata.TagOverride.Any() ? metadata.TagOverride : document.DocumentTags;

        cards.Add(new QaCard(
            question,
            answer,
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
