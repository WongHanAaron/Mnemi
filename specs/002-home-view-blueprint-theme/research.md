# Research: Home View Blueprint Theme

## Decision 1: Shared UI placement for home feature scaffold

- Decision: Place reusable home view components, display models, and theme assets in `src/Ui.Shared`, with only route/host wiring in `src/Ui.Web`.
- Rationale: Repository architecture guidance requires host-agnostic shared UI that can run in both web and MAUI.
- Alternatives considered:
  - Build directly in `src/Ui.Web` only. Rejected because it would duplicate work for MAUI and violate shared-first direction.
  - Build host-specific split components in both hosts. Rejected due to maintenance overhead and divergence risk.

## Decision 2: Home page composition structure

- Decision: Use a desktop-first composition with side navigation + primary content column containing welcome, quick stats, recent decks, and pinned decks sections.
- Rationale: Matches FR-001/FR-002 and the requested horizontal blueprint direction while keeping sections independently testable.
- Alternatives considered:
  - Top navigation shell. Rejected because spec explicitly requests side-oriented navigation.
  - Single long list without section grouping. Rejected due to poor scanability and weaker motivational framing.

## Decision 3: Deck card visual contract

- Decision: Standardize deck cards with four parts: visual cover area, metadata hierarchy, progress/status row, and primary action button.
- Rationale: Satisfies FR-007 and enables consistent styling across recent/pinned sections with explicit test hooks.
- Alternatives considered:
  - Plain text rows without cover treatment. Rejected as inconsistent with requested audio-card-inspired direction.
  - Free-form per-section card styling. Rejected because it breaks theme cohesion and increases CSS complexity.

## Decision 4: State handling model for loading/empty/missing data

- Decision: Represent section state explicitly using a small state enum/value contract (`Loading`, `Populated`, `Empty`, `MissingData`).
- Rationale: Directly addresses FR-010 and enables deterministic render logic and test scenarios.
- Alternatives considered:
  - Infer state from null checks only. Rejected because it is brittle and obscures intent.
  - Global page-level state only. Rejected because sections can have independent state.

## Decision 5: Testing strategy for selectors and behavior

- Decision: Add stable selectors (`data-testid`) to all critical interaction regions and key card actions; validate with shared component tests and web E2E checks.
- Rationale: Meets FR-012 and aligns with repository UI/testing guidance for reliable automation.
- Alternatives considered:
  - Rely on visible text selectors only. Rejected due to brittleness.
  - End-to-end testing only. Rejected because component-level state rendering should be validated in isolation.

## Decision 6: Theme integration direction

- Decision: Implement an original tokenized theme inspired by the reference direction, using centralized CSS variables and reusable classes rather than hard-coding style values per component.
- Rationale: Meets FR-008 while avoiding vendor lock-in and preserving future MAUI/web portability.
- Alternatives considered:
  - Directly copy source-site styling. Rejected for maintainability and originality concerns.
  - Keep default project styling. Rejected because it does not satisfy the requested visual identity.
