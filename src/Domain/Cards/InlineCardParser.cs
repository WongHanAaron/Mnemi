using System;
using System.Collections.Generic;
using System.Linq;

namespace Mnemi.Domain.Cards;

internal sealed class InlineCardParser : CardTypeParser
{
    public InlineCardParser(MetadataParser metadataParser)
        : base(metadataParser)
    {
    }

    public override bool CanParse(string trimmedLine)
    {
        return trimmedLine.Contains("::", StringComparison.Ordinal);
    }

    public override int Parse(Document document, string[] lines, int startIndex, List<Card> cards)
    {
        var line = lines[startIndex];
        var splitIndex = line.IndexOf("::", StringComparison.Ordinal);
        if (splitIndex < 0)
        {
            return startIndex;
        }

        var questionText = line[..splitIndex].Trim();
        var answerText = line[(splitIndex + 2)..].Trim();
        var rawContent = line.TrimEnd();
        var metadataIndex = MetadataParser.FindMetadataLine(lines, startIndex + 1, out var metadata);
        var groups = metadata.TagOverride.Any() ? metadata.TagOverride : document.DocumentTags;

        if (questionText.Contains("{{", StringComparison.Ordinal) && questionText.Contains("}}", StringComparison.Ordinal))
        {
            var blanks = CardParserUtilities.ExtractClozeBlanks(questionText, answerText);
            if (!blanks.Any())
            {
                return startIndex;
            }

            cards.Add(new ClozeCard(
                questionText,
                blanks,
                CardParserUtilities.ComputeHash(rawContent),
                groups,
                metadata.LearningState,
                rawContent,
                document.File.RelativePath,
                startIndex + 1,
                startIndex + 1));
            return startIndex;
        }

        if (answerText.Contains("|"))
        {
            var options = CardParserUtilities.ExtractMultipleChoiceOptions(answerText.Split('|', StringSplitOptions.RemoveEmptyEntries));
            if (options.Any())
            {
                cards.Add(new MultipleChoiceCard(
                    questionText,
                    options,
                    CardParserUtilities.ComputeHash(rawContent),
                    groups,
                    metadata.LearningState,
                    rawContent,
                    document.File.RelativePath,
                    startIndex + 1,
                    startIndex + 1));
                return startIndex;
            }
        }

        cards.Add(new QaCard(
            questionText,
            answerText,
            CardParserUtilities.ComputeHash(rawContent),
            groups,
            metadata.LearningState,
            rawContent,
            document.File.RelativePath,
            startIndex + 1,
            startIndex + 1));
        return startIndex;
    }
}
