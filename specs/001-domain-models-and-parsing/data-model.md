# Data Model: Markdown Flashcard Parsing

## Core Entities

### Card
Represents a parsed flashcard at the shared base level.

- `Id` (string) — a shortened hash derived from the original raw card content.
- `Source` (string?) — source filename or document id
- `CardType` (enum) — `Qa`, `Cloze`, `MultipleChoice`
- `Groups` (`IReadOnlyList<Group>`) — resolved group paths
- `LearningState` — optional review metadata from `mnemi:` comments
- `RawContent` (string) — the original card text block for traceability
- `SourceLineNumberStart` (int?) — approximate line where the card begins
- `SourceLineNumberEnd` (int?) — approximate line where the card ends

### CardType
Enumerates supported card formats.

- `Qa`
- `Cloze`
- `MultipleChoice`

### QaCard
Specialized card model for standard question/answer cards.

- `Question` (string)
- `Answer` (string)

### ClozeCard
Specialized card model for cloze content.

- `QuestionText` (string) — the text containing placeholders such as `{{Paris}}`
- `ClozeBlanks` (`IReadOnlyList<ClozeBlank>`)

### ClozeBlank
Represents a single blank inside a cloze card.

- `Placeholder` (string) — the visible placeholder text, e.g. `{{Paris}}`
- `Options` (`IReadOnlyList<ClozeAnswerOption>`) — a superset of candidate answers with accepted status flags

### ClozeAnswerOption
Represents a candidate answer for a cloze blank.

- `Text` (string)
- `IsAccepted` (bool)

### MultipleChoiceCard
Specialized card model for MCQ content.

- `Question` (string)
- `Options` (`IReadOnlyList<MultipleChoiceOption>`)

### MultipleChoiceOption
Represents a single choice in an MCQ.

- `Text` (string)
- `IsCorrect` (bool)

### LearningState
Represents hidden learning metadata parsed from `mnemi:` comments.

- `Hash` (string?)
- `Status` (string) — `new`, `learning`, `review`, or `lapsed`
- `Days` (int?)
- `Ease` (decimal?)
- `Due` (`DateTime?`)
- `LastResponse` (string?) — `again`, `hard`, `good`, or `easy`
- `Lapses` (int?)
- `Repetitions` (int?)
- `TagOverride` (`IReadOnlyList<Group>?`) — card-level group overrides

### Group
Represents a normalized Obsidian tag path.

- `Segments` (`IReadOnlyList<string>`)
- `DisplayPath` (string) — e.g. `Spanish::Verbs::Irregular`

## Supporting Concepts

### File
Represents raw source file metadata and content.

- `Filename` (string) — the file's base name
- `RelativePath` (string) — the path relative to the vault or repository root
- `DateCreated` (`DateTime?`) — optional file creation timestamp
- `DateLastModified` (`DateTime?`) — optional last modified timestamp
- `FileContents` (string) — raw Markdown content of the file

### Document
Represents a parsed Markdown document containing content and document-level card properties.

- `File` (`File`) — source file metadata
- `Content` (string) — raw document content after any document-level normalization
- `DocumentTags` (`IReadOnlyList<Group>`) — top-of-document groups that apply to all cards unless overridden

### Parser Result
The document parser accepts a `File` and returns a `Document` with the following expectations:

- The `Document` contains raw content and document-level card properties.
- The document parser is responsible for extracting top-of-document tags.
- The card parser then consumes the `Document` and produces parsed `Card` objects.
- Cards with card-level `tag=` overrides still use the override values.
- Cards preserve raw Markdown content in `RawContent` for traceability.
- Learning metadata is attached through `LearningState` when present.
- The returned cards carry source metadata such as `Source`, `SourceLineNumberStart`, and `SourceLineNumberEnd`.

## Relationships

- A `CardParser` produces many `Card` entities from a single `Document`.
- Each `Card` may include multiple `Groups`.
- `ClozeCard` and `MultipleChoiceCard` are specialized views of the base `Card` concept; the parser may implement them as separate record types or a single card model with optional typed details.
