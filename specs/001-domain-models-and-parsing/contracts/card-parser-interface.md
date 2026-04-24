# Contract: Card Parser Interface

## Purpose

Describe the public interface and expected behavior for the Mnemi document parser and card parser.

## Public API

### `DocumentParser`

A shared parser class that converts raw file metadata into a parsed `Document` representation.

#### Methods

- `Document Parse(File file)`
  - `file`: the raw source file metadata and contents.
  - Returns a `Document` containing document content and document-level card properties.

- `Document ParseLines(IEnumerable<string> lines, File file)`
  - Optional helper for line-based parsing scenarios when input is already split into lines.
  - `file`: the source file metadata used for context and source tracking.

### `CardParser`

A shared parser class that converts a parsed `Document` into card models.

#### Methods

- `IReadOnlyList<Card> Parse(Document document)`
  - `document`: the parsed document containing raw content and document-level card properties.
  - Returns a list of parsed `Card` objects.

## Output Model Contract

### `Card`

A generic card contract containing:

- `string Id` — a shortened hash derived from `RawContent`
- `CardType CardType`
- `IReadOnlyList<Group> Groups`
- `LearningState? LearningState`
- `string RawContent`
- `string? Source`
- `int? SourceLineNumberStart`
- `int? SourceLineNumberEnd`

### `CardType`

- `Qa`
- `Cloze`
- `MultipleChoice`

### `QaCard`

- `string Question`
- `string Answer`

### `ClozeCard`

- `string QuestionText`
- `IReadOnlyList<ClozeBlank> ClozeBlanks`

### `MultipleChoiceCard`

- `string Question`
- `IReadOnlyList<MultipleChoiceOption> Options`

### `LearningState`

- `string? Hash`
- `string Status`
- `int? Days`
- `decimal? Ease`
- `DateTime? Due`
- `string? LastResponse`
- `int? Lapses`
- `int? Repetitions`
- `IReadOnlyList<Group>? TagOverride`

### `ClozeBlank`

- `string Placeholder`
- `IReadOnlyList<ClozeAnswerOption> Options`

### `ClozeAnswerOption`

- `string Text`
- `bool IsAccepted`

### `MultipleChoiceOption`

- `string Text`
- `bool IsCorrect`

### `Group`

- `IReadOnlyList<string> Segments`
- `string DisplayPath`

### `Document`

A parsed document containing source file metadata, document-level tags, and raw document content.

- `File File`
- `string Content`
- `IReadOnlyList<Group> DocumentTags`

### `File`

- `string Filename`
- `string RelativePath`
- `DateTime? DateCreated`
- `DateTime? DateLastModified`
- `string FileContents`

## Group Normalization Contract

- Document-level tags are extracted from the file header before the first card definition.
- Tag segments are normalized to UpperCamelCase for lowercase tags.
- Same-root tags resolve to the longest specific path.
- Independent roots are preserved as multiple group entries.
- Card-level `tag=` metadata in `mnemi:` comments overrides document-level groups.

## Error Handling Contract

- Invalid card definitions are ignored; parsing continues for the remainder of the document.
- Malformed `mnemi:` comments do not prevent card creation.
- Escaped reserved markers such as `\::`, `\#!`, `\{{`, and `\}}` are preserved in output text.

## Notes

This contract is intentionally focused on the shared domain interface, not on UI rendering or persistence details. Future adapters may consume `IReadOnlyList<Card>` and apply review scheduling or storage as required.
