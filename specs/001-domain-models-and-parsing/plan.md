# Implementation Plan: Domain Models and Markdown Parsing

**Branch**: `001-domain-models-and-parsing` | **Date**: 2026-04-23 | **Spec**: specs/001-domain-models-and-parsing/spec.md
**Input**: Feature specification from `specs/001-domain-models-and-parsing/spec.md`

**Note**: This plan is based on the existing Mnemi repository structure and the markdown flashcard syntax defined in `docs/users/markdown-question-format.md` and `docs/users/card-grouping-and-tags.md`.

## Summary

Implement a Domain-first card model and Markdown parser for Mnemi that converts supported QA syntax into strongly typed flashcard objects, preserves hidden learning metadata, and resolves Obsidian document/group tags into normalized group paths. The design uses a shared `Card` base model with specialized `QaCard`, `ClozeCard`, and `MultipleChoiceCard` variants so each card type has its own required properties without redundancy. The parser accepts a raw `File` and returns a parsed `Document` containing top-of-document tags and the list of parsed cards. The parser will belong in the shared `src/Domain` library so core business rules remain platform-agnostic and available to the web and MAUI UI layers.

## Technical Context

**Language/Version**: C# / .NET 8.0  
**Primary Dependencies**: None external for the Domain model; built-in .NET libraries only (`System.Text`, `System.Text.RegularExpressions`, `System.Collections.Generic`)  
**Storage**: N/A for parser and model creation; card metadata remains in-memory domain objects for later persistence or review scheduling  
**Testing**: xUnit via `tests/Domain.Tests`  
**Target Platform**: Shared library consumed by `src/Application`, `src/Ui.Web`, and `src/Ui.Maui`  
**Project Type**: Domain library / shared business logic  
**Performance Goals**: Support typical markdown content sizes for study documents; parse multi-hundred-line documents in under 100ms on modern hardware  
**Constraints**: No external parsing dependencies; the parser must support escaped syntax for reserved markers and preserve plain Markdown content within questions/answers  
**Scale/Scope**: Single-file Markdown ingestion and card model creation. Cross-file vault scanning and UI rendering are out of scope.

## Constitution Check

No explicit constitution gates were identified from the project constitution placeholder file. The planned design remains aligned with the repository's existing architecture by keeping shared logic in `src/Domain` and avoiding unnecessary external dependencies.

## Project Structure

### Documentation (this feature)

```text
specs/001-domain-models-and-parsing/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── card-parser-interface.md
└── tasks.md
```

### Source Code (repository root)

```text
src/
├── Domain/
│   ├── Cards/
│   ├── Domain.csproj
│   └── Entity.cs
├── Application/
│   ├── Cards/
│   ├── Application.csproj
│   └── IApplicationService.cs
├── Ui.Web/
├── Ui.Maui/
└── Ui.Shared/

tests/
└── Domain.Tests/
```

**Structure Decision**: Use the existing `src/Domain` project for core models and parser logic, with `src/Application` serving as the integration layer for future adapters and services.

## Complexity Tracking

No additional complexity violations are required for this plan. The selected design is intentionally simple: a shared Domain library plus tests in `tests/Domain.Tests`.
