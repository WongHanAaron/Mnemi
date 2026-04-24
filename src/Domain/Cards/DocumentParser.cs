using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Mnemi.Domain.Cards;

public class DocumentParser
{
    private static readonly Regex TagLinePattern = new("^#(?<tag>[A-Za-z0-9_/]+)$", RegexOptions.Compiled);
    private static readonly Regex CardBoundaryPattern = new(@"(#!qa|#!cloze|#!mcq|::)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public Document Parse(File file)
    {
        if (file == null)
        {
            throw new ArgumentNullException(nameof(file));
        }

        var lines = SplitLines(file.FileContents);
        var documentTags = new List<Group>();

        for (var index = 0; index < lines.Length; index++)
        {
            var trimmedLine = lines[index].Trim();
            if (string.IsNullOrWhiteSpace(trimmedLine))
            {
                continue;
            }

            if (TagLinePattern.IsMatch(trimmedLine))
            {
                var rawTag = TagLinePattern.Match(trimmedLine).Groups["tag"].Value;
                documentTags.Add(Group.Parse(rawTag));
                continue;
            }

            if (CardBoundaryPattern.IsMatch(trimmedLine))
            {
                break;
            }
        }

        var normalizedTags = Group.PruneAncestors(documentTags);
        return new Document(file, file.FileContents, normalizedTags);
    }

    private static string[] SplitLines(string content)
    {
        return content?.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
            ?? Array.Empty<string>();
    }
}
