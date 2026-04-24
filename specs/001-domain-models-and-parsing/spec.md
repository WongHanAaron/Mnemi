# Feature Specification: Domain Models and Markdown Parsing

**Feature Branch**: `001-domain-models-and-parsing`  
**Created**: 2026-04-23  
**Status**: Draft  
**Input**: User description: "Can you setup the domain models based on the #file:markdown-question-format.md and #file:card-grouping-and-tags.md. Addiitonally, I would like to create the parsing logic for creating the cards from a document file content."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Import markdown flashcards (Priority: P1)

A content author wants to convert a Markdown document containing Mnemi-formatted QA definitions into structured flashcards, so the application can use those cards for study and review.

**Why this priority**: This is the core value of the feature and the primary entry point for all content ingestion.

**Independent Test**: Provide a sample Markdown document with single-line QA, cloze, and MCQ entries, then verify the parser returns a list of card objects with correct types, text, and metadata.

**Acceptance Scenarios**:

1. **Given** a Markdown file with a single-line QA definition, **When** the parser processes the file, **Then** it returns one card with `CardType.QA` and the correct question and answer.
2. **Given** a Markdown file with multiple card formats and mixed normal text, **When** the parser processes the file, **Then** it returns only the card definitions and ignores plain note text.

---

### User Story 2 - Resolve grouping tags from document header (Priority: P2)

A user wants document-level Obsidian tags at the top of a file to define default groups for all cards in that file.

**Why this priority**: Grouping is essential for organizing cards into decks and allows users to manage study content by topic.

**Independent Test**: Provide a Markdown document with header tags like `#spanish` and `#spanish/verbs`, then verify that parsed cards include the resolved group path `Spanish::Verbs`.

**Acceptance Scenarios**:

1. **Given** a file with `#spanish` and `#spanish/verbs` tags, **When** cards are parsed, **Then** the parser assigns the most specific group in the same hierarchy (`Spanish::Verbs`).
2. **Given** a file with `#mathematics/probability` and `#machinelearning/bayesianinference`, **When** cards are parsed, **Then** the parser assigns both groups to each card.

---

### User Story 3 - Apply card-level grouping overrides (Priority: P3)

A user wants to override document-level grouping for a specific card using `tag=` metadata in the `mnemi:` comment.

**Why this priority**: This allows per-card grouping flexibility and supports edge cases where a single card belongs to a different or additional group.

**Independent Test**: Provide a card comment with `tag=Spanish::Verbs::Irregular` and verify the resulting card uses that group instead of the document default.

**Acceptance Scenarios**:

1. **Given** a card with a `mnemi:` comment containing `tag=Spanish::Verbs::Irregular`, **When** the parser processes the card, **Then** the card is grouped as `Spanish::Verbs::Irregular`.
2. **Given** a card with a `tag=` value containing multiple comma-separated groups, **When** the parser processes the card, **Then** the card includes all specified groups.

---

### Edge Cases

- Document tags appear before any QA content and still include non-card Markdown such as headings and paragraphs.
- Multiple document-level tags share the same root; the parser chooses the longest specific path for those tags.
- Independent document tags with different roots apply together to the same card.
- A card contains escaped reserved syntax such as `\::` or `\#!` and should still parse correctly.
- A card has no `mnemi:` metadata comment and should still produce a valid card with default learning state.
- A card has malformed metadata fields; the parser should ignore invalid fields and preserve valid ones.
- A `#!mcq` block contains continued option text on indented lines.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST parse single-line QA definitions using `Question::Answer` into card objects.
- **FR-002**: The system MUST parse single-line cloze definitions using `{{answer}}::` and produce cloze card models with blank mappings.
- **FR-003**: The system MUST parse single-line multiple-choice definitions using `Question::Option1|Option2|*CorrectOption*|Option4` and identify the correct choice.
- **FR-004**: The system MUST parse multi-line QA, cloze, and MCQ blocks using `#!qa`, `#!cloze`, and `#!mcq` delimiters with `::` separating question and answer sections.
- **FR-005**: The system MUST ignore normal Markdown note text and only create cards from recognized QA definitions.
- **FR-006**: The system MUST parse document-level Obsidian tags from the file header before QA content and normalize them into group paths using the documented hierarchy rules.
- **FR-007**: The system MUST resolve same-root document tags to the most specific group path and include independent root tags as multiple groups.
- **FR-008**: The system MUST parse card-level group overrides from `tag=` in the `mnemi:` metadata comment and apply them with higher priority than document tags.
- **FR-009**: The system MUST parse `mnemi:` comments and preserve learning-state metadata fields such as status, days, ease, due, last, lapses, reps, and optional tag overrides.
- **FR-010**: The system MUST normalize group names so lowercase tags become UpperCamelCase group segments and mixed-case segments are preserved as documented.
- **FR-011**: The system MUST provide a parsing entrypoint that accepts raw Markdown file content and returns a collection of domain card models with source metadata.
- **FR-012**: The system MUST support escaped marker syntax for literal `#!`, `::`, `{{`, and `}}` text inside card content.

### Key Entities *(include if feature involves data)*

- **Card**: Represents a parsed flashcard with a unique identity, question text, answer text or structured answer data, type, group assignments, metadata, and source location.
- **CardType**: Represents the flashcard format, including QA, Cloze, and MultipleChoice.
- **ClozeBlank**: Represents a single blank inside a cloze card, including the visible placeholder and expected answer variants.
- **MultipleChoiceOption**: Represents each option in a multiple-choice card, including whether it is marked correct.
- **LearningState**: Represents parsed hidden state metadata from `mnemi:` comments, including review status, interval, ease factor, due date, last response, lapses, and repetitions.
- **Group**: Represents a normalized group path such as `Spanish::Verbs::Irregular`, including normalization behavior from Obsidian tag syntax.
- **DocumentContext**: Represents parsing context for a source file, including document-level tags, header normalization, and card-level overrides.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: The parser correctly converts at least 90% of valid Markdown QA samples from the provided syntax guide into domain card models during initial validation.
- **SC-002**: Document-level tags and card-level tag overrides are resolved consistently, with same-root document tags yielding the most specific path and independent roots resulting in multiple groups.
- **SC-003**: The parser correctly identifies and preserves the correct answer for cloze and MCQ formats in at least three representative test documents.
- **SC-004**: At least one end-to-end sample document is parsed into cards with no parsing errors for plain note text, multi-line block syntax, and escaped reserved markers.
- **SC-005**: The output domain model includes card metadata and source location for all parsed cards, enabling later review-state updates and traceability.

## Assumptions

- The feature covers domain model definitions and parsing from a single Markdown document file; cross-file vault indexing or folder scanning is out of scope.
- The parser may preserve raw Markdown for complex question or answer content and does not need to render HTML or UI presentation.
- Learning-state metadata is optional; missing `mnemi:` comments result in default/new state values.
- Group path normalization follows the documented Obsidian tag rules and is not responsible for enforcing user naming conventions beyond those rules.
- Parser performance is expected to handle typical study documents, not extremely large files or real-time editing workloads.
- This feature does not include review scheduling algorithms or UI behavior; it only creates typed card objects and group metadata.
