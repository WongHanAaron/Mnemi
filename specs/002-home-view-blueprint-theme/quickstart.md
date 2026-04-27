# Quickstart: Home View Blueprint Theme

## Purpose

Validate and iterate on the home dashboard scaffold for the blueprint-inspired desktop layout in a shared-ui-safe way.

## Prerequisites

- .NET 8 SDK installed
- Workspace opened at repository root
- Existing solution restored (`dotnet restore` if needed)

## Run the Web Host

Use the existing task:
- `run-web`

Or run directly:
```powershell
 dotnet run --project src/Ui.Web/Ui.Web.csproj
```

Open the local URL printed by the host and navigate to the home route.

## Manual Validation Checklist

1. Desktop horizontal composition is present:
- Side navigation rail
- Welcome panel
- Quick stats section
- Recent decks section
- Pinned decks section

2. Deck cards follow audio-card-inspired structure:
- Cover/visual area
- Metadata hierarchy (title/subtitle/status)
- Progress/value indicators
- Primary study action control

3. Theme consistency:
- Shared tokenized colors/spacing/typography across sections
- No mixed default and custom style fragments

4. Section state rendering:
- Loading state visible for quick stats/recent/pinned
- Empty state visible for recent/pinned
- Missing-data fallback visible for profile and deck metadata gaps

5. Automation hooks:
- Stable `data-testid` attributes exist for core sections and primary actions

## Build Validation

Run the web build task:
- `build-web`

Optional MAUI compile check (Windows target):
- `build-maui-windows`

## Validation Results (2026-04-27)

- Web build: PASS
	- Command: `build-web`
	- Result: `src/Ui.Web/Ui.Web.csproj` builds successfully.
- MAUI Windows build: BLOCKED BY ENVIRONMENT
	- Command: `build-maui-windows`
	- Result: `Microsoft.Maui.Sdk` not found in this environment (`MSB4236`).
	- Note: This indicates missing MAUI workload/tooling on the machine, not a detected feature-code compile error in this pass.

## Suggested Test Coverage

- `tests/Ui.Shared.Tests`: component rendering tests for populated/empty/loading/missing states.
- `tests/Ui.E2E.Playwright`: selector stability and primary action visibility on desktop viewport.
