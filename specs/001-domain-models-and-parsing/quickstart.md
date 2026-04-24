# Quickstart: Markdown Card Parsing

## Purpose

This document shows how to use the shared Markdown parser to convert raw file metadata into a parsed `Document` containing document-level tags and card models.

## Setup

1. Add a project reference to `src/Domain/Domain.csproj` from any consuming project.
2. Use the shared parser API in the Domain library.

## Example Usage

```csharp
using Mnemi.Domain.Cards;

var file = new File
{
    Filename = Path.GetFileName("deck.md"),
    RelativePath = "notes/deck.md",
    DateCreated = File.GetCreationTime("deck.md"),
    DateLastModified = File.GetLastWriteTime("deck.md"),
    FileContents = File.ReadAllText("deck.md")
};

var documentParser = new DocumentParser();
var cardParser = new CardParser();

var document = documentParser.Parse(file);
var cards = cardParser.Parse(document);

Console.WriteLine($"Document tags: {string.Join(", ", document.DocumentTags.Select(t => t.DisplayPath))}");
foreach (var card in cards)
{
    Console.WriteLine($"Card Type: {card.CardType}");
    Console.WriteLine($"Source lines: {card.SourceLineNumberStart}-{card.SourceLineNumberEnd}");
    Console.WriteLine($"Groups: {string.Join(", ", card.Groups.Select(g => g.DisplayPath))}");
}
```

## What to Expect

- Single-line QA definitions become `CardType.Qa` cards.
- Cloze definitions with `{{...}}::` become `CardType.Cloze` cards.
- Multiple-choice lines with `|` and a starred correct option become `CardType.MultipleChoice` cards.
- Multi-line block forms using `#!qa`, `#!cloze`, and `#!mcq` are parsed correctly.
- Document-level tag headers like `#spanish/verbs` are normalized into `Spanish::Verbs` groups.
- Card-level `tag=` overrides in `mnemi:` comments replace or augment default groups.

## Sample Input

```markdown
#spanish
#spanish/verbs

What is "hablar"?::to speak
<!-- mnemi: 7f4a2e | status=review | tag=Spanish::Verbs -->

#!qa
What is the process when water turns
from liquid to vapor?
::
Evaporation
#!
```

## Expected Output

- Card 1: QA card, question `What is "hablar"?`, answer `to speak`, groups `[Spanish::Verbs]`
- Card 2: QA block card, groups `[Spanish::Verbs]`

## Notes

- The parser is designed for content ingestion and may preserve raw Markdown for rendering later.
- Use the `sourcePath` argument to track the original file location in the resulting card metadata.
