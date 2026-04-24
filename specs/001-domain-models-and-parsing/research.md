# Research: Domain Models and Markdown Parsing

## Decision Summary

- Chosen architecture: keep core flashcard models and Markdown parsing logic in `src/Domain`.
- Rationale: The Domain project is the shared business layer in this repository and should own the core rules for card creation, tag resolution, and state metadata independent of UI or application hosting.
- Alternative considered: placing the parser in `src/Application` as an integration service. Rejected because parsing syntax is domain logic, not adapter-specific behavior.

## Parsing Scope and Behavior

- The parser will support:
  - single-line QA (`Question::Answer`)
  - single-line cloze (`Text with {{answer}}::`)
  - single-line multiple choice (`Question::Option1|Option2|*Correct*|Option4`)
  - multi-line QA blocks with `#!qa`, `::`, and `#!`
  - multi-line cloze blocks with `#!cloze`, `::`, and `#!`
  - multi-line MCQ blocks with `#!mcq`, a list of `- [ ]` / `- [x]` options, and `#!`
- The parser will ignore plain Markdown content outside recognized QA blocks.
- Reserved markers inside content can be escaped using a leading backslash, e.g. `\::`, `\#!`, `\{{`, `\}}`.

## Group Tag Resolution

- Document-level Obsidian tags will be parsed from the file header before the first card definition.
- Tag normalization rules:
  - `#spanish` → `Spanish`
  - `#spanish/verbs` → `Spanish::Verbs`
  - `#MachineLearning/BayesianInference` → `MachineLearning::BayesianInference`
- Same-root tags use the most specific path.
  - Example: `#spanish`, `#spanish/verbs` → `Spanish::Verbs`
- Independent roots are preserved together.
  - Example: `#mathematics/probability` and `#machinelearning/bayesianinference` → both groups on the card.
- Card-level override using `tag=` inside the `mnemi:` metadata comment has highest priority.

## Metadata Handling

- The parser will consume `mnemi:` comments immediately following a card definition.
- Recognized hidden metadata fields include:
  - `status`, `days`, `ease`, `due`, `last`, `lapses`, `reps`, and optional `tag=` overrides.
- If a hash or comment is malformed or absent, the parser will still create a card with default learning-state values.

## Contract Decision

- A lightweight public contract document will be created to describe the parser interface and output model.
- This contract will remain language-agnostic and describe the `DocumentParser` and `CardParser` APIs, card model shapes, and expected source metadata.
