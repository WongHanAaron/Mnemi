# Implementation Plan: Home View Blueprint Theme

**Branch**: `003-prepare-spec-branch` | **Date**: 2026-04-27 | **Spec**: specs/002-home-view-blueprint-theme/spec.md
**Input**: Feature specification from `specs/002-home-view-blueprint-theme/spec.md`

## Summary

Deliver a desktop-first home dashboard scaffold that mirrors the requested horizontal composition and side navigation direction, with an original music-app-inspired theme and audio-card-style deck surfaces. Implementation stays host-agnostic by placing reusable home view components and UI contracts in shared UI (`src/Ui.Shared`) and keeping host wiring minimal in `src/Ui.Web` (and later `src/Ui.Maui`). The feature includes explicit loading, empty, and missing-data states plus stable automation selectors for UI validation.

## Technical Context

**Language/Version**: C# / .NET 8 (Blazor components)  
**Primary Dependencies**: Existing Blazor stack in `Ui.Shared` and `Ui.Web`; project theme scaffolding aligned with Blueprint-style layout guidance from spec reference  
**Storage**: N/A for this scaffold (view-model/stubbed in-memory data only)  
**Testing**: xUnit for shared component tests in `tests/Ui.Shared.Tests`; Playwright for web E2E validation hooks (existing test project)  
**Target Platform**: Web first (`src/Ui.Web`) with shared compatibility for MAUI host (`src/Ui.Maui`)  
**Project Type**: Shared UI feature for Blazor web + MAUI hosts  
**Performance Goals**: Home view shell and first visible section render within acceptable interactive desktop UX budget (target < 2s in local dev run)  
**Constraints**: Must keep shared UI host-agnostic; no direct host API calls in shared components; provide stable selectors for automated tests; preserve desktop-horizontal intent while handling narrower widths gracefully  
**Scale/Scope**: One home page scaffold with side navigation, welcome, quick stats, recent decks, and pinned decks including populated/empty/loading/missing-data visual states

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

The file `.specify/memory/constitution.md` is a placeholder template and does not define enforceable constitutional principles yet. Planning gates are therefore evaluated against repository architecture guidance and feature-level constraints instead.

Pre-Phase-0 gate status:
- PASS: Shared UI host-agnostic rule is preserved by keeping reusable home view UI in `src/Ui.Shared`.
- PASS: Host-specific concerns remain in host projects (`src/Ui.Web`, future `src/Ui.Maui`).
- PASS: Testability requirement is addressed with stable selectors and planned shared/web test coverage.

Post-Phase-1 re-check:
- PASS: Data model and contracts keep workflow/state in shared abstractions.
- PASS: Contract design avoids host-specific APIs in shared UI.
- PASS: Empty/loading/missing data states and automation hooks are explicitly specified.

## Project Structure

### Documentation (this feature)

```text
specs/002-home-view-blueprint-theme/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── home-view-ui-contract.md
└── tasks.md
```

### Source Code (repository root)

```text
src/
├── Ui.Shared/
│   ├── Components/
│   │   └── Home/
│   │       ├── HomeDashboard.razor
│   │       ├── SideNav.razor
│   │       ├── DeckCard.razor
│   │       └── QuickStatTile.razor
│   ├── Models/
│   │   └── Home/
│   │       ├── HomeDashboardViewModel.cs
│   │       ├── StudyDeckSummaryViewModel.cs
│   │       └── QuickStatMetricViewModel.cs
│   └── Styles/
│       └── home-blueprint-theme.css
├── Ui.Web/
│   ├── Pages/
│   │   └── Home.razor
│   └── Program.cs
└── Ui.Maui/
    └── Program.cs

tests/
├── Ui.Shared.Tests/
│   └── Home/
│       ├── HomeDashboardTests.cs
│       └── DeckCardTests.cs
└── Ui.E2E.Playwright/
    └── HomeView.spec.ts
```

**Structure Decision**: Use shared UI components and view models in `src/Ui.Shared` for cross-host reuse, with host routing/wiring in `src/Ui.Web` and future MAUI DI integration in `src/Ui.Maui`.

## Complexity Tracking

No constitutional violations requiring exception tracking were identified.

## Implementation Status (2026-04-27)

Completed through Phase 5 with Phase 6 polish artifacts added.

### Build and Validation Outcomes

- Web build (`src/Ui.Web/Ui.Web.csproj`): PASS
- Shared component tests (`tests/Ui.Shared.Tests/Ui.Shared.Tests.csproj`): PASS
- MAUI Windows build (`src/Ui.Maui/Ui.Maui.csproj -f net8.0-windows10.0.19041.0`): BLOCKED BY ENVIRONMENT
    - `Microsoft.Maui.Sdk` is not resolvable in the current machine environment (`MSB4236`).

### Documentation Outputs

- Developer doc: `docs/dev/ui-home-view-blueprint-architecture.md`
- User doc: `docs/users/home-dashboard-overview.md`
