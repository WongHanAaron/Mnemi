# Tasks: Home View Blueprint Theme

**Input**: Design documents from `/specs/002-home-view-blueprint-theme/`
**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/home-view-ui-contract.md`

**Tests**: Included because the specification requires acceptance-test coverage and stable automation selectors.

**Organization**: Tasks are grouped by user story so each story can be implemented and tested independently.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Establish feature scaffolding and shared asset locations.

- [x] T001 Create home feature folders and placeholder files in src/Ui.Shared/Components/Home/.gitkeep, src/Ui.Shared/Models/Home/.gitkeep, and src/Ui.Shared/Styles/home-blueprint-theme.css
- [x] T002 Create web host home route shell file in src/Ui.Web/Pages/Home.razor
- [x] T003 [P] Create web stub service folder and file in src/Ui.Web/Services/HomeDashboardStubDataProvider.cs

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Build shared contracts and baseline wiring required by all stories.

**⚠️ CRITICAL**: No user story work starts before this phase is complete.

- [x] T004 Create shared state enums in src/Ui.Shared/Models/Home/SectionDataState.cs and src/Ui.Shared/Models/Home/LayoutMode.cs
- [x] T005 [P] Create shared action models in src/Ui.Shared/Models/Home/HomePrimaryAction.cs and src/Ui.Shared/Models/Home/DeckPrimaryAction.cs
- [x] T006 [P] Create shared profile and metric models in src/Ui.Shared/Models/Home/LearnerProfileSummaryViewModel.cs and src/Ui.Shared/Models/Home/QuickStatMetricViewModel.cs
- [x] T007 [P] Create shared deck and dashboard models in src/Ui.Shared/Models/Home/StudyDeckSummaryViewModel.cs and src/Ui.Shared/Models/Home/HomeDashboardViewModel.cs
- [x] T008 Create host-agnostic dashboard data contract in src/Application/Home/IHomeDashboardService.cs
- [x] T009 Implement web stub dashboard service in src/Ui.Web/Services/HomeDashboardService.cs
- [x] T010 Register dashboard service dependency in src/Ui.Web/Program.cs

**Checkpoint**: Foundation complete and user stories can proceed.

---

## Phase 3: User Story 1 - Open Home Dashboard In Horizontal Desktop Layout (Priority: P1) 🎯 MVP

**Goal**: Deliver the desktop horizontal home dashboard with side navigation and core sections.

**Independent Test**: Launch web app in desktop viewport and verify side nav, welcome, quick stats, recent decks, and pinned decks are all visible and structured.

### Tests for User Story 1

- [x] T011 [P] [US1] Add bUnit layout rendering test in tests/Ui.Shared.Tests/Home/HomeDashboardLayoutTests.cs
- [x] T012 [P] [US1] Add Playwright desktop layout test in tests/Ui.E2E.Playwright/HomeView.layout.spec.ts

### Implementation for User Story 1

- [x] T013 [P] [US1] Implement side navigation component in src/Ui.Shared/Components/Home/HomeSideNav.razor
- [x] T014 [P] [US1] Implement quick stat tile component in src/Ui.Shared/Components/Home/QuickStatTile.razor
- [x] T015 [P] [US1] Implement deck card component shell in src/Ui.Shared/Components/Home/DeckCard.razor
- [x] T016 [US1] Implement desktop dashboard composition in src/Ui.Shared/Components/Home/HomeDashboard.razor
- [x] T017 [US1] Wire home page to shared dashboard component in src/Ui.Web/Pages/Home.razor
- [x] T018 [US1] Add side navigation route link in src/Ui.Web/Components/Layout/NavMenu.razor
- [x] T019 [US1] Add required core selectors in src/Ui.Shared/Components/Home/HomeDashboard.razor

**Checkpoint**: User Story 1 is functional and independently testable.

---

## Phase 4: User Story 2 - Recognize Visual Theme And Card Style (Priority: P2)

**Goal**: Apply cohesive blueprint-inspired theme and audio-card-style deck surfaces.

**Independent Test**: Verify themed colors, typography, spacing, and audio-card-style deck surfaces are consistently applied across home sections.

### Tests for User Story 2

- [x] T020 [P] [US2] Add deck-card theme structure test in tests/Ui.Shared.Tests/Home/DeckCardThemeTests.cs
- [x] T021 [P] [US2] Add Playwright theme consistency test in tests/Ui.E2E.Playwright/HomeView.theme.spec.ts

### Implementation for User Story 2

- [x] T022 [P] [US2] Define theme tokens and layout classes in src/Ui.Shared/Styles/home-blueprint-theme.css
- [x] T023 [US2] Apply themed shell and section classes in src/Ui.Shared/Components/Home/HomeDashboard.razor
- [x] T024 [US2] Apply audio-card visual hierarchy and action affordances in src/Ui.Shared/Components/Home/DeckCard.razor
- [x] T025 [US2] Apply quick-stat visual styling classes in src/Ui.Shared/Components/Home/QuickStatTile.razor
- [x] T026 [US2] Expose and consume shared stylesheet assets in src/Ui.Shared/Ui.Shared.csproj and src/Ui.Web/Pages/Home.razor

**Checkpoint**: User Stories 1 and 2 are independently functional.

---

## Phase 5: User Story 3 - Use Home View Across Shared Hosts (Priority: P3)

**Goal**: Ensure host-agnostic behavior with explicit state handling and compatibility wiring.

**Independent Test**: Validate shared UI has no host-specific API calls, renders loading/empty/missing states, and preserves selector contract.

### Tests for User Story 3

- [x] T027 [P] [US3] Add section-state rendering tests in tests/Ui.Shared.Tests/Home/HomeDashboardStateTests.cs
- [x] T028 [P] [US3] Add selector-contract Playwright test in tests/Ui.E2E.Playwright/HomeView.selectors.spec.ts

### Implementation for User Story 3

- [x] T029 [US3] Implement loading and empty section rendering in src/Ui.Shared/Components/Home/HomeDashboard.razor
- [x] T030 [US3] Implement missing-data fallbacks for profile and deck metadata in src/Ui.Shared/Components/Home/HomeDashboard.razor and src/Ui.Shared/Components/Home/DeckCard.razor
- [x] T031 [US3] Add adaptive fallback layout rules in src/Ui.Shared/Components/Home/HomeDashboard.razor and src/Ui.Shared/Styles/home-blueprint-theme.css
- [x] T032 [US3] Register equivalent home dashboard service wiring in src/Ui.Maui/Program.cs
- [x] T033 [US3] Enforce shared contract callback usage in src/Ui.Shared/Components/Home/HomeDashboard.razor, src/Ui.Shared/Components/Home/DeckCard.razor, and src/Ui.Shared/Components/Home/HomeSideNav.razor

**Checkpoint**: All user stories are independently functional and testable.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final quality, documentation, and validation across stories.

- [x] T034 [P] Update developer documentation in docs/dev/ui-home-view-blueprint-architecture.md
- [x] T035 [P] Update user documentation in docs/users/home-dashboard-overview.md
- [x] T036 Run quickstart validation checklist and record results in specs/002-home-view-blueprint-theme/quickstart.md
- [x] T037 [P] Run web and MAUI build checks and note outcomes in specs/002-home-view-blueprint-theme/plan.md

---

## Dependencies & Execution Order

### Phase Dependencies

- Setup (Phase 1) has no dependencies.
- Foundational (Phase 2) depends on Setup and blocks all user stories.
- User Stories (Phases 3-5) depend on Foundational completion.
- Polish (Phase 6) depends on completion of all selected user stories.

### User Story Dependencies

- User Story 1 (P1): Starts after Foundational and is the MVP.
- User Story 2 (P2): Starts after Foundational; depends on US1 components for themed enhancement only.
- User Story 3 (P3): Starts after Foundational; integrates with US1/US2 components while remaining independently testable.

### Within Each User Story

- Write tests first and confirm failure before implementation.
- Implement shared models/contracts before component composition updates.
- Add selectors and fallback states before final story sign-off.

---

## Parallel Execution Examples

### User Story 1

- Run T011 and T012 in parallel.
- Run T013, T014, and T015 in parallel.

### User Story 2

- Run T020 and T021 in parallel.
- Run T022 and T025 in parallel.

### User Story 3

- Run T027 and T028 in parallel.
- Run T031 and T032 in parallel.

---

## Implementation Strategy

### MVP First (US1 Only)

1. Complete Phase 1 and Phase 2.
2. Complete Phase 3 (US1).
3. Validate US1 independently with T011 and T012 before moving on.

### Incremental Delivery

1. Deliver US1 (layout scaffold) as MVP.
2. Deliver US2 (theme and card visual language).
3. Deliver US3 (cross-host and state robustness).
4. Finish with Phase 6 polish and validation.

### Parallel Team Strategy

1. Pair on Phase 1 and Phase 2 to unblock all story work.
2. After foundations: one engineer drives US1/US3 state architecture, another drives US2 theme and visual tests.
3. Rejoin for polish, docs, and build verification.
