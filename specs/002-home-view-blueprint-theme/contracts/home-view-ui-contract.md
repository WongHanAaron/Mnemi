# Contract: Home View UI Contract

## Purpose

Define the shared UI contract for the Home View scaffold so web and MAUI hosts can consume the same behavior while keeping shared components host-agnostic.

## Shared Contract Surface

### Home Dashboard Component

Component name:
- `HomeDashboard`

Inputs:
- `HomeDashboardViewModel Model`
- `EventCallback<HomePrimaryAction> OnPrimaryStudyAction`
- `EventCallback<DeckPrimaryAction> OnDeckAction`

Behavioral requirements:
- Must render side navigation region in desktop-horizontal mode.
- Must render welcome, quick stats, recent decks, and pinned decks regions.
- Must render loading, empty, and missing-data fallback states per section.
- Must not call host-specific APIs directly.

### Deck Card Component

Component name:
- `DeckCard`

Inputs:
- `StudyDeckSummaryViewModel Model`
- `EventCallback<DeckPrimaryAction> OnAction`

Behavioral requirements:
- Must render visual cover/artwork placeholder, metadata, progress/status, and primary action.
- Must gracefully degrade when artwork/title metadata is missing.

### Side Navigation Component

Component name:
- `HomeSideNav`

Inputs:
- `IReadOnlyList<HomeNavItemViewModel> Items`
- `string ActiveItemId`

Behavioral requirements:
- Must render side-oriented primary navigation for desktop home experience.
- Must expose active item semantics and keyboard-focus-safe links/buttons.

## Selector Contract (Automation)

The following stable selector keys must exist in rendered markup:

- `data-testid="home-shell"`
- `data-testid="home-sidenav"`
- `data-testid="home-welcome"`
- `data-testid="home-quick-stats"`
- `data-testid="home-recent-decks"`
- `data-testid="home-pinned-decks"`
- `data-testid="home-primary-study-action"`
- `data-testid="deck-card-{deckId}"`
- `data-testid="deck-card-action-{deckId}"`

State selectors:
- `data-testid="home-state-loading-{section}"`
- `data-testid="home-state-empty-{section}"`
- `data-testid="home-state-missing-{section}"`

## Host Integration Contract

Web host (`src/Ui.Web`):
- Responsible for route registration and DI wiring of data provider abstractions.
- Must pass shared view model to `HomeDashboard` without introducing host-only types in shared component signatures.

MAUI host (`src/Ui.Maui`):
- Responsible for equivalent DI wiring and shell navigation integration.
- Must preserve the same shared component contract and selector behavior where applicable.

## Data Contract Snapshot

`HomeDashboardViewModel` minimum fields:
- `LearnerProfileSummaryViewModel Profile`
- `IReadOnlyList<QuickStatMetricViewModel> QuickStats`
- `IReadOnlyList<StudyDeckSummaryViewModel> RecentDecks`
- `IReadOnlyList<StudyDeckSummaryViewModel> PinnedDecks`
- `SectionDataState QuickStatsState`
- `SectionDataState RecentDecksState`
- `SectionDataState PinnedDecksState`

## Non-Goals

- No commitment in this contract to backend API shape.
- No requirement to implement MAUI-native host features inside shared components.
- No fixed requirement for a third-party UI package API surface; visual direction is token/style based.
