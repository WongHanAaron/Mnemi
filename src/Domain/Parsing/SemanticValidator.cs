using System;
using System.Collections.Generic;
using System.Linq;
using Mnemi.Domain.Entities;

namespace Mnemi.Domain.Parsing;

public sealed record Diagnostic(string Code, string Message, int LineNumber);

public sealed record DiagnosticBag(IReadOnlyList<Diagnostic> Diagnostics)
{
    public bool HasErrors => Diagnostics.Count > 0;
}

public interface ISemanticValidator
{
    DiagnosticBag Validate(BoundDocument boundDocument);
}

public sealed class SemanticValidator : ISemanticValidator
{
    private const string MissingSeparatorCode = "DSL001";
    private const string MissingSeparatorMessage = "Card block is missing a separator line (::).";
    private const string InvalidClozeCode = "DSL002";
    private const string InvalidClozeMessage = "Cloze card must contain at least one cloze blank.";
    private const string InvalidMcqCode = "DSL003";
    private const string InvalidMcqMessage = "Multiple choice card must contain at least one correct option.";
    private readonly ICardParserUtilities _cardParserUtilities;

    public SemanticValidator(ICardParserUtilities cardParserUtilities)
    {
        _cardParserUtilities = cardParserUtilities ?? throw new ArgumentNullException(nameof(cardParserUtilities));
    }

    public DiagnosticBag Validate(BoundDocument boundDocument)
    {
        if (boundDocument == null)
        {
            throw new ArgumentNullException(nameof(boundDocument));
        }

        var diagnostics = new List<Diagnostic>();

        foreach (var question in boundDocument.Questions)
        {
            switch (question.Question)
            {
                case InlineCardSyntaxNode inline:
                    ValidateInline(inline, diagnostics);
                    break;
                case BlockCardSyntaxNode block:
                    ValidateBlock(block, diagnostics);
                    break;
            }
        }

        return new DiagnosticBag(diagnostics);
    }

    private void ValidateInline(InlineCardSyntaxNode inline, List<Diagnostic> diagnostics)
    {
        var splitIndex = inline.Text.IndexOf(CardParsingConstants.CardSeparator, StringComparison.Ordinal);
        if (splitIndex < 0)
        {
            diagnostics.Add(new Diagnostic(MissingSeparatorCode, MissingSeparatorMessage, inline.LineNumber));
            return;
        }

        var question = inline.Text[..splitIndex];
        var answer = inline.Text[(splitIndex + 2)..];

        if (question.Contains(CardParsingConstants.ClozeOpen, StringComparison.Ordinal) &&
            question.Contains(CardParsingConstants.ClozeClose, StringComparison.Ordinal) &&
            !_cardParserUtilities.ExtractClozeBlanks(question, answer).Any())
        {
            diagnostics.Add(new Diagnostic(InvalidClozeCode, InvalidClozeMessage, inline.LineNumber));
        }

        if (answer.Contains(CardParsingConstants.MultipleChoiceDelimiter, StringComparison.Ordinal))
        {
            var options = _cardParserUtilities.ExtractMultipleChoiceOptions(answer.Split(CardParsingConstants.MultipleChoiceDelimiter, StringSplitOptions.RemoveEmptyEntries));
            if (!options.Any(option => option.IsCorrect))
            {
                diagnostics.Add(new Diagnostic(InvalidMcqCode, InvalidMcqMessage, inline.LineNumber));
            }
        }
    }

    private void ValidateBlock(BlockCardSyntaxNode block, List<Diagnostic> diagnostics)
    {
        var separator = Array.FindIndex(block.BodyLines, line => line.Trim() == CardParsingConstants.CardSeparator);
        if (separator < 0)
        {
            diagnostics.Add(new Diagnostic(MissingSeparatorCode, MissingSeparatorMessage, block.LineNumber));
            return;
        }

        var question = string.Join(Environment.NewLine, block.BodyLines[..separator]).Trim();
        var answer = string.Join(Environment.NewLine, block.BodyLines[(separator + 1)..]).Trim();

        if (block.BlockType == CardType.Cloze)
        {
            var blanks = _cardParserUtilities.ExtractClozeBlanks(question, answer);
            if (!blanks.Any())
            {
                diagnostics.Add(new Diagnostic(InvalidClozeCode, InvalidClozeMessage, block.LineNumber));
            }
        }

        if (block.BlockType == CardType.MultipleChoice)
        {
            var options = _cardParserUtilities.ExtractMultipleChoiceOptions(block.BodyLines[(separator + 1)..]);
            if (!options.Any(option => option.IsCorrect))
            {
                diagnostics.Add(new Diagnostic(InvalidMcqCode, InvalidMcqMessage, block.LineNumber));
            }
        }
    }
}
