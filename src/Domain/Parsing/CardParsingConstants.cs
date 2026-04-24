namespace Mnemi.Domain.Parsing;

internal static class CardParsingConstants
{
    internal const string QaBlockStart = "#!qa";
    internal const string ClozeBlockStart = "#!cloze";
    internal const string McqBlockStart = "#!mcq";
    internal const string BlockClosingMarker = "#!";
    internal const string CardSeparator = "::";
    internal const string MetadataCommentPrefix = "<!--";
    internal const string ClozeOpen = "{{";
    internal const string ClozeClose = "}}";
    internal const string MultipleChoiceDelimiter = "|";
    internal const string CrLf = "\r\n";
    internal const string Lf = "\n";
    internal const string Space = " ";
    internal const string Tab = "\t";
}