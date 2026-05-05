# Implementation Plan: Sidebar App Layout

**Branch**: `007-sidebar-layout` | **Date**: 2026-05-04 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/007-sidebar-layout/spec.md`

## Summary

Replace the current top-bar navigation with a persistent sidebar layout inspired by the Blazor Blueprint music app pattern. The sidebar will contain three sections: a user identity header (avatar + current page), a collapsible Quick Links section, and a collapsible Pinned Decks section. The sidebar will be implemented as a shared Razor component in `Ui.Shared`, following the project's UI architecture guidelines for host-agnostic, responsive components that work in both Blazor WebAssembly and MAUI Blazor Hybrid hosts.

## Technical Context

**Language/Version**: C# (.NET 8.0)  
**Primary Dependencies**: Microsoft.AspNetCore.Components.Web 8.0, Microsoft.AspNetCore.Components.WebAssembly 8.0, Microsoft.Maui.Controls 8.0  
**Storage**: In-memory (session-scoped collapse state), no persistence required  
**Testing**: bUnit for component unit tests, Playwright for web E2E, Appium for MAUI E2E  
**Target Platform**: Blazor WebAssembly (browser) + .NET MAUI Blazor Hybrid (Windows desktop, future iOS/Android)  
**Project Type**: web-app with shared Blazor component library  
**Performance Goals**: 60fps animations, <200ms sidebar section expand/collapse, <100ms initial sidebar render  
**Constraints**: Single shared component codebase (Ui.Shared RCL) for both Web + MAUI hosts, no horizontal top-bar, must follow ui-architecture-guidelines.md (no host-specific code in shared components, no @rendermode in shared components, interface-based service injection for platform differences)  
**Scale/Scope**: 3 sidebar sections, ~5 quick links, up to ~20 pinned decks visible before scrolling

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

The project constitution (`.specify/memory/constitution.md`) is currently a blank template with no defined principles. No constitutional gates apply. However, the following project-level design constraints from `docs/dev/ui-architecture-guidelines.md` serve as de facto governance:

| Gate | Status | Notes |
|------|--------|-------|
| Shared component in Ui.Shared | ✅ PASS | Sidebar will live in `src/Ui.Shared/Components/Layout/` |
| No host-specific code in shared components | ✅ PASS | Platform detection via injected interface, not direct API calls |
| No @rendermode in shared components | ✅ PASS | Render mode chosen by host app |
| Interface-based platform abstraction | ✅ PASS | `IViewStateService` interface defined in Application layer |
| No MAUI/browser APIs in Ui.Shared | ✅ PASS | Viewport detection implemented in host services only |
| Responsive breakpoints follow ViewBreakpoints | ✅ PASS | Phone ≤767, Tablet 768-1023, Desktop ≥1024 |
| Component accepts data via parameters/DI | ✅ PASS | Navigation items, user info, pinned decks via parameters + DI |
| CSS-only responsiveness where possible | ✅ PASS | Content area sizing via CSS Grid, sidebar visibility via media queries |

## Project Structure

### Documentation (this feature)

```text
specs/007-sidebar-layout/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   └── sidebar-contracts.md
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
src/
├── Application/
│   └── Ports/
│       └── IViewStateService.cs          # NEW: ViewState service interface
├── Ui.Shared/
│   ├── Components/
│   │   └── Layout/
│   │       ├── AppSidebar.razor           # NEW: Main shared sidebar component
│   │       ├── AppSidebar.razor.css       # NEW: Sidebar styles (CSS isolation)
│   │       ├── SidebarUserSection.razor   # NEW: User icon + current page section
│   │       ├── SidebarSection.razor       # NEW: Reusable collapsible section
│   │       ├── SidebarSection.razor.css   # NEW: Collapsible section styles
│   │       ├── SidebarQuickLinks.razor    # NEW: Quick links content
│   │       ├── SidebarPinnedDecks.razor   # NEW: Pinned decks content
│   │       └── SidebarNavLink.razor       # NEW: Individual nav link item
│   ├── Models/
│   │   ├── SidebarNavItem.cs              # NEW: Nav item view model
│   │   ├── PinnedDeckItem.cs             # NEW: Pinned deck display model
│   │   └── ViewState.cs                  # MOVED: From docs to code (enum + breakpoints)
│   └── wwwroot/
│       └── Styles/
│           └── sidebar-layout.css         # NEW: Global sidebar layout styles
├── Ui.Web/
│   ├── Components/
│   │   └── Layout/
│   │       ├── MainLayout.razor           # MODIFIED: Replace top-nav with sidebar layout
│   │       └── NavMenu.razor              # REMOVED: Top-bar nav eliminated
│   ├── Services/
│   │   └── WebViewStateService.cs         # NEW: Browser viewport detection via JS interop
│   └── wwwroot/
│       └── css/
│           └── app.css                    # MODIFIED: Remove nav-menu styles, add sidebar shell
└── Ui.Maui/
    └── Services/
        └── MauiViewStateService.cs        # NEW: MAUI window-size detection
```

**Structure Decision**: Single shared component library (`Ui.Shared`) with host-specific service implementations in `Ui.Web.Services` (note: current codebase places services inline in `Ui.Web/` rather than a separate `Ui.Web.Services` project). The sidebar component lives in `src/Ui.Shared/Components/Layout/` following the existing pattern where `HomeSideNav.razor` already exists. The `IViewStateService` interface is defined in `Application/Ports/` to keep shared contracts in the Application layer, consistent with the architecture guideline that shared layers define *what* is needed.

## Complexity Tracking

> No constitutional violations. All design decisions follow existing project patterns and ui-architecture-guidelines.md.
