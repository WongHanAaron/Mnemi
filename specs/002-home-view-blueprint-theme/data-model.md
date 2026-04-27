# Data Model: Home View Blueprint Theme

## Entities

### HomeDashboard
Represents the complete home-screen state consumed by the shared home view.

Fields:
- `LearnerProfile` (`LearnerProfileSummary`)
- `QuickStats` (`IReadOnlyList<QuickStatMetric>`)
- `RecentDecks` (`IReadOnlyList<StudyDeckSummary>`)
- `PinnedDecks` (`IReadOnlyList<StudyDeckSummary>`)
- `PrimaryStudyAction` (`HomePrimaryAction`)
- `ViewState` (`HomeViewState`)
- `LastUpdatedUtc` (`DateTimeOffset`)

Validation rules:
- Must always provide non-null section containers (may be empty).
- `PrimaryStudyAction` must include a non-empty action label and action identifier.
- `ViewState` defaults to `DesktopHorizontal` for this feature scope.

State transitions:
- `Initializing` -> `ReadyPopulated`
- `Initializing` -> `ReadyEmpty`
- `Initializing` -> `ReadyMissingDataFallback`
- `Ready*` -> `Refreshing` -> `Ready*`

### LearnerProfileSummary
Represents minimal identity information for welcome messaging.

Fields:
- `DisplayName` (`string?`)
- `GreetingFallback` (`string`)
- `AvatarInitials` (`string?`)

Validation rules:
- If `DisplayName` is null/whitespace, UI must render `GreetingFallback`.
- `AvatarInitials` is optional and non-blocking.

### QuickStatMetric
Represents a single motivation/progress tile.

Fields:
- `MetricId` (`string`)
- `Label` (`string`)
- `ValueText` (`string`)
- `TrendText` (`string?`)
- `TrendDirection` (`MetricTrendDirection`)
- `State` (`SectionDataState`)

Validation rules:
- `MetricId`, `Label`, and `ValueText` are required when state is `Populated`.
- For `MissingData`, UI renders fallback text and suppresses trend.

### StudyDeckSummary
Represents a deck card in recent and pinned sections.

Fields:
- `DeckId` (`string`)
- `Title` (`string`)
- `Subtitle` (`string?`)
- `ProgressPercent` (`int?`)
- `StatusLabel` (`string?`)
- `ArtworkToken` (`string?`)
- `DueCount` (`int?`)
- `IsPinned` (`bool`)
- `PrimaryAction` (`DeckPrimaryAction`)
- `State` (`SectionDataState`)

Validation rules:
- `DeckId` and `Title` are required for `Populated` state.
- `ProgressPercent` must be within 0-100 when provided.
- Missing artwork must fall back to themed placeholder visual.

State transitions:
- `Loading` -> `Populated`
- `Loading` -> `Empty`
- `Loading` -> `MissingData`

### HomeViewState
Represents current layout mode for the home view.

Fields:
- `Mode` (`LayoutMode`) where feature scope uses `DesktopHorizontal`
- `IsAdaptiveFallbackActive` (`bool`)
- `ViewportWidth` (`double?`)

Validation rules:
- For this phase, `Mode` is expected to be desktop-horizontal first.
- Adaptive fallback can activate when viewport narrows near tablet threshold.

## Supporting Value Objects

### HomePrimaryAction
- `ActionId` (`string`)
- `Label` (`string`)
- `TargetRoute` (`string`)

### DeckPrimaryAction
- `ActionId` (`string`)
- `Label` (`string`)
- `PayloadDeckId` (`string`)

### SectionDataState (enum)
- `Loading`
- `Populated`
- `Empty`
- `MissingData`

### MetricTrendDirection (enum)
- `Up`
- `Flat`
- `Down`
- `Unknown`

### LayoutMode (enum)
- `DesktopHorizontal`
- `TabletAdaptive`
- `PhoneStacked`

## Relationships

- One `HomeDashboard` has one `LearnerProfileSummary`.
- One `HomeDashboard` has many `QuickStatMetric` items.
- One `HomeDashboard` has many `StudyDeckSummary` items in each section (`RecentDecks`, `PinnedDecks`).
- Each `StudyDeckSummary` has one `DeckPrimaryAction`.
- `HomeViewState` influences structural rendering decisions but does not own section data.
