# Implementation Tasks: Domain Models and Markdown Parsing

## Phase 1 - Domain Model

- [ ] Add shared domain entities for file metadata, parsed documents, card groups, and card variants.
- [ ] Define strong group objects and learning state metadata to support parsed tags and review state.

## Phase 2 - Parser Implementation

- [ ] Implement `DocumentParser` to extract top-of-document tags and preserve raw content.
- [ ] Implement `CardParser` to turn a parsed `Document` into typed `Card` objects.
- [ ] Support single-line QA, cloze, and multiple-choice cards.
- [ ] Support multi-line `#!qa`, `#!cloze`, and `#!mcq` blocks, including escaped marker semantics.
- [ ] Parse `mnemi:` comments for learning metadata and card-level group overrides.

## Phase 3 - Tests

- [ ] Add unit tests for document parsing and card parsing behavior.
- [ ] Verify document-level tags, override tags, cloze option parsing, and QA parsing.
