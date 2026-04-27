# Home View Blueprint Architecture

## Overview

This document describes the implemented architecture for the Home View Blueprint Theme feature in the shared UI stack.

Feature artifacts live under:
- specs/002-home-view-blueprint-theme/

## Architecture Goals

- Keep home dashboard UI host-agnostic in shared libraries.
- Keep host-specific service registration in host projects.
- Expose stable UI selectors for automation.
- Support deterministic UI states for loading, empty, and missing-data rendering.

## Implemented Project Boundaries

Shared and host boundaries follow the repository guidance:

- src/Ui.Shared
  - Home components and view models.
  - Theme tokens and classes.
  - No direct MAUI or web-host API usage.
- src/Application
  - Home dashboard service contract and data transfer records.
- src/Ui.Web
  - Dashboard service implementation and route wiring.
- src/Ui.Maui
  - Equivalent service wiring to the shared contract.

## Key Components

Home dashboard components:
- src/Ui.Shared/Components/Home/HomeDashboard.razor
- src/Ui.Shared/Components/Home/HomeSideNav.razor
- src/Ui.Shared/Components/Home/DeckCard.razor
- src/Ui.Shared/Components/Home/QuickStatTile.razor

Home view models and enums:
- src/Ui.Shared/Models/Home/

Application contract:
- src/Application/Home/IHomeDashboardService.cs

Web service implementation:
- src/Ui.Web/Services/HomeDashboardService.cs
- src/Ui.Web/Services/HomeDashboardStubDataProvider.cs

MAUI service implementation:
- src/Ui.Maui/Services/HomeDashboardService.cs
- src/Ui.Maui/Program.cs

## UI States and Fallbacks

The shared dashboard component supports section-level states:
- Loading
- Empty
- MissingData
- Populated

Fallback behavior is explicitly rendered for missing profile and deck metadata to keep the home surface resilient under partial data.

## Selector Contract

Primary selectors implemented:
- data-testid="home-shell"
- data-testid="home-sidenav"
- data-testid="home-welcome"
- data-testid="home-quick-stats"
- data-testid="home-recent-decks"
- data-testid="home-pinned-decks"
- data-testid="home-primary-study-action"
- data-testid="deck-card-{deckId}"
- data-testid="deck-card-action-{deckId}"
- state selectors under data-testid="home-state-*"

## Styling and Theme Exposure

Single-source home theme stylesheet:
- src/Ui.Shared/Styles/home-blueprint-theme.css

Static web asset exposure is wired through:
- src/Ui.Shared/Ui.Shared.csproj

Consumed by web home route:
- src/Ui.Web/Pages/Home.razor

## Validation Snapshot

- Shared component tests pass in tests/Ui.Shared.Tests.
- Web build passes for src/Ui.Web/Ui.Web.csproj.
- MAUI Windows build requires local MAUI workload installation in the environment where the build runs.

## Future Considerations

- Add Playwright project configuration for executing E2E specs under tests/Ui.E2E.Playwright.
- Replace stubbed dashboard data with production application service integration.
- Extend adaptive layout behavior for richer tablet and phone variants while retaining shared component contracts.
