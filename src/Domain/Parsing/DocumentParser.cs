using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Mnemi.Domain.Entities;
using EntityFile = Mnemi.Domain.Entities.File;
using EntityGroup = Mnemi.Domain.Entities.Group;

namespace Mnemi.Domain.Parsing;

public interface IDocumentParser
{
    Document Parse(EntityFile file);
}

public class DocumentParser : IDocumentParser
{
    private const string TagGroupName = "tag";
    private static readonly Regex TagLinePattern = new("^#(?<tag>[A-Za-z0-9_/]+)$", RegexOptions.Compiled);
    private static readonly Regex CardBoundaryPattern = new(@"(#!qa|#!cloze|#!mcq|::)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private readonly IGroupParser _groupParser;

    public DocumentParser(IGroupParser groupParser)
    {
        _groupParser = groupParser ?? throw new ArgumentNullException(nameof(groupParser));
    }

    public Document Parse(EntityFile file)
    {
        if (file == null)
        {
            throw new ArgumentNullException(nameof(file));
        }

        var lines = SplitLines(file.FileContents);
        var documentTags = new List<EntityGroup>();

        for (var index = 0; index < lines.Length; index++)
        {
            var trimmedLine = lines[index].Trim();
            if (string.IsNullOrWhiteSpace(trimmedLine))
            {
                continue;
            }

            if (TagLinePattern.IsMatch(trimmedLine))
            {
                var rawTag = TagLinePattern.Match(trimmedLine).Groups[TagGroupName].Value;
                documentTags.Add(_groupParser.Parse(rawTag));
                continue;
            }

            if (CardBoundaryPattern.IsMatch(trimmedLine))
            {
                break;
            }
        }

        var normalizedTags = _groupParser.PruneAncestors(documentTags);
        return new Document(file, file.FileContents, normalizedTags);
    }

    private static string[] SplitLines(string content)
    {
        return content?.Split(new[] { CardParsingConstants.CrLf, CardParsingConstants.Lf }, StringSplitOptions.None)
            ?? Array.Empty<string>();
    }
}
