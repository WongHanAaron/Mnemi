# Tasks: Sidebar App Layout

**Input**: Design documents from `/specs/007-sidebar-layout/`
**Prerequisites**: plan.md (required), spec.md (required)

**Tests**: Not explicitly requested in specification. Test tasks omitted. Component testing can be done manually via `dotnet run` and visual verification.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3, US4)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create the foundational interfaces and enums that all sidebar components depend on

- [x] T001 Create ViewState enum and ViewBreakpoints constants in `src/Ui.Shared/Models/ViewState.cs` (Phone ≤767, Tablet 768-1023, Desktop ≥1024)
- [x] T002 Create IViewStateService interface in `src/Ui.Shared/Ports/IViewStateService.cs` (moved from Application/Ports per architecture — Application can't reference Ui.Shared)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core sidebar shell, layout grid, and host-specific viewport services that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [x] T003 Replace MainLayout.razor shell with CSS Grid layout (sidebar column + content column) in `src/Ui.Web/Components/Layout/MainLayout.razor`
- [x] T004 Remove NavMenu.razor top-bar component — cleared content in `src/Ui.Web/Components/Layout/NavMenu.razor`
- [x] T005 [P] Create AppSidebar.razor shell component (sidebar container with `aside` element) in `src/Ui.Shared/Components/Layout/AppSidebar.razor`
- [x] T006 [P] Create sidebar layout CSS (CSS Grid shell, sidebar width 16rem, content fills remaining space) in `src/Ui.Shared/wwwroot/Styles/sidebar-layout.css`
- [x] T007 [P] Implement WebViewStateService (browser resize → ViewState via JS interop) in `src/Ui.Web/Services/WebViewStateService.cs`
- [x] T008 [P] Implement MauiViewStateService (MAUI window size → ViewState) in `src/Ui.Maui/Services/MauiViewStateService.cs`
- [x] T009 Register IViewStateService in DI containers: added to `Program.cs` in `src/Ui.Web/Program.cs` and `src/Ui.Maui/Program.cs`
- [x] T010 Integrate empty AppSidebar into MainLayout.razor and verify two-column layout renders without top-bar

**Checkpoint**: Foundation ready - sidebar shell appears on the left, main content on the right, no top-bar. User story implementation can now begin.

---

## Phase 3: User Story 1 - Persistent Sidebar Navigation (Priority: P1) 🎯 MVP

**Goal**: Users see a sidebar with their user icon/avatar and the name of the current page, and this updates as they navigate

**Independent Test**: Launch the app, verify sidebar displays user icon and "Home" as current page, navigate to Review, verify sidebar shows "Review"

### Implementation for User Story 1

- [x] T011 [P] [US1] Create SidebarNavItem model (Id, Label, Href, IconKind properties) in `src/Ui.Shared/Models/SidebarNavItem.cs`
- [x] T012 [P] [US1] Create SidebarUserSection.razor component (user avatar placeholder + current page label) in `src/Ui.Shared/Components/Layout/SidebarUserSection.razor`
- [x] T013 [US1] Add CurrentPage parameter to AppSidebar.razor and pass it to SidebarUserSection in `src/Ui.Shared/Components/Layout/AppSidebar.razor`
- [x] T014 [US1] Wire current page detection: derive page name from NavigationManager.Uri in `src/Ui.Web/Components/Layout/MainLayout.razor`
- [x] T015 [US1] Update app.css to remove nav-menu and top-bar related styles, ensure sidebar + content layout fills viewport in `src/Ui.Web/wwwroot/css/app.css`
- [x] T016 [US1] Sidebar user section styles merged into `src/Ui.Shared/wwwroot/Styles/sidebar-layout.css`

**Checkpoint**: Sidebar shows user icon area and current page name. Navigating changes the label. No top-bar present.

---

## Phase 4: User Story 2 - Collapsible Quick Links Section (Priority: P2)

**Goal**: Sidebar contains a collapsible Quick Links section with links to Home, Review, and other core pages that can be expanded/collapsed with a chevron toggle

**Independent Test**: Verify Quick Links section appears below user section, expand/collapse works via header click, clicking a link navigates correctly, active link is highlighted

### Implementation for User Story 2

- [x] T017 [P] [US2] Create SidebarSection.razor reusable collapsible component (header with label + chevron, collapsible content region) in `src/Ui.Shared/Components/Layout/SidebarSection.razor`
- [x] T018 [P] [US2] Collapse animation CSS merged into `src/Ui.Shared/wwwroot/Styles/sidebar-layout.css`
- [x] T019 [P] [US2] Create SidebarNavLink.razor component (icon + label, active highlight, href navigation) in `src/Ui.Shared/Components/Layout/SidebarNavLink.razor`
- [x] T020 [US2] Create SidebarQuickLinks.razor component (wraps SidebarSection, contains SidebarNavLink items for Home, Review, Decks) in `src/Ui.Shared/Components/Layout/SidebarQuickLinks.razor`
- [x] T021 [US2] Integrate SidebarQuickLinks into AppSidebar.razor between user section and pinned decks in `src/Ui.Shared/Components/Layout/AppSidebar.razor`
- [x] T022 [US2] Pass active route to SidebarNavLink to highlight current page with active/inactive visual state in `src/Ui.Shared/Components/Layout/SidebarQuickLinks.razor`

**Checkpoint**: Quick Links section appears, collapses/expands with animation, links navigate and highlight active page

---

## Phase 5: User Story 3 - Collapsible Pinned Decks Section (Priority: P2)

**Goal**: Sidebar shows user's pinned flashcard decks in a collapsible section below Quick Links, with empty state when no decks are pinned

**Independent Test**: Verify Pinned Decks section appears, shows deck names when decks are pinned, clicking a deck navigates to study view, section collapses/expands independently of Quick Links, empty state shows when no decks pinned

### Implementation for User Story 3

- [x] T023 [P] [US3] Create PinnedDeckItem model (Id, Name, Href properties) in `src/Ui.Shared/Models/PinnedDeckItem.cs`
- [x] T024 [US3] Create SidebarPinnedDecks.razor component (wraps SidebarSection, accepts `IReadOnlyList<PinnedDeckItem>` parameter, shows empty state "No pinned decks" when list empty) in `src/Ui.Shared/Components/Layout/SidebarPinnedDecks.razor`
- [x] T025 [US3] Add PinnedDecks parameter and OnDeckSelected callback to AppSidebar.razor in `src/Ui.Shared/Components/Layout/AppSidebar.razor`
- [x] T026 [US3] Wire pinned decks data: pass sample pinned deck list from MainLayout.razor in `src/Ui.Web/Components/Layout/MainLayout.razor`
- [x] T027 [US3] Scroll overflow handling merged into `src/Ui.Shared/wwwroot/Styles/sidebar-layout.css`
- [x] T028 [US3] Text truncation for long deck names merged into `src/Ui.Shared/wwwroot/Styles/sidebar-layout.css`

**Checkpoint**: Pinned Decks section functional - shows pinned decks with navigation, empty state works, scrolls on overflow, text truncates

---

## Phase 6: User Story 4 - Responsive Sidebar Behavior (Priority: P3)

**Goal**: Sidebar adapts to narrow screens: hidden by default on phone with hamburger toggle, compact on tablet, fully visible on desktop

**Independent Test**: Resize browser to phone width, verify sidebar hidden with toggle button visible; tap toggle, verify sidebar appears as overlay; resize to tablet, verify compact mode; resize to desktop, verify full sidebar

### Implementation for User Story 4

- [x] T029 [US4] Add sidebar toggle button (hamburger menu icon) that appears on phone/tablet viewports in `src/Ui.Shared/Components/Layout/AppSidebar.razor`
- [x] T030 [US4] CSS media queries for sidebar responsiveness in `src/Ui.Shared/wwwroot/Styles/sidebar-layout.css`
- [x] T031 [US4] Phone overlay behavior (slide-in, transform) in `src/Ui.Shared/wwwroot/Styles/sidebar-layout.css`
- [x] T032 [US4] Wire IViewStateService into AppSidebar.razor (inject, subscribe to OnViewStateChanged, auto-open/close) in `src/Ui.Shared/Components/Layout/AppSidebar.razor`
- [x] T033 [US4] Add overlay backdrop that closes sidebar when clicked on phone-width screens in `src/Ui.Shared/Components/Layout/AppSidebar.razor`

**Checkpoint**: Sidebar fully responsive across all viewport sizes. Phone shows toggle + overlay, tablet shows compact icons, desktop shows full sidebar.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Final cleanup, consistency, and verification across all user stories

- [x] T034 [P] Consistent sidebar colors using CSS variables from home-blueprint-theme.css in `src/Ui.Shared/wwwroot/Styles/sidebar-layout.css`
- [ ] T035 [P] Verify sidebar renders correctly in MAUI Blazor host by launching `src/Ui.Maui/Ui.Maui.csproj` and checking layout
- [x] T036 Removed remaining references to NavMenu.razor (only MainLayout referenced it, now updated)
- [x] T037 Verified no horizontal top-bar remnants in CSS or Razor files
- [x] T038 Full app build passes: `dotnet build src/Ui.Web/Ui.Web.csproj` compiles with 0 errors
- [x] T039 Aria labels and roles added to sidebar (`aria-label`, `role="navigation"`, `aria-expanded` on sections, `aria-current` on nav links)
- [x] T040 Data-testid attributes added to all sidebar elements (`data-testid="sidebar"`, `data-testid="sidebar-toggle"`, `data-testid="sidebar-section-quick-links"`, etc.)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Story 1 (Phase 3)**: Depends on Foundational phase completion
- **User Story 2 (Phase 4)**: Depends on US1 (SidebarUserSection must exist before adding sections below it)
- **User Story 3 (Phase 5)**: Depends on US2 (SidebarSection collapsible component reused by Pinned Decks)
- **User Story 4 (Phase 6)**: Depends on US1+US2+US3 (responsive behavior wraps the complete sidebar)
- **Polish (Phase 7)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 2 (P2)**: Depends on US1 (sidebar shell and user section must be in place)
- **User Story 3 (P2)**: Depends on US2 (reuses SidebarSection collapsible component)
- **User Story 4 (P3)**: Depends on US1+US2+US3 (responsive behavior applies to complete sidebar)

### Within Each User Story

- Models before components
- Reusable components before composite components
- Component creation before integration into parent
- Core implementation before styling

### Parallel Opportunities

- T001 and T002 can run in parallel (different files, no dependencies)
- T005, T006, T007, T008 can all run in parallel within Foundational phase
- T011 and T012 can run in parallel within US1
- T017, T018, T019 can run in parallel within US2
- T023 can run in parallel with US2 tasks
- All Polish tasks marked [P] can run in parallel

---

## Parallel Example: User Story 1

```bash
# Launch models and component in parallel:
Task: "Create SidebarNavItem model in src/Ui.Shared/Models/SidebarNavItem.cs"
Task: "Create SidebarUserSection.razor component in src/Ui.Shared/Components/Layout/SidebarUserSection.razor"

# Then wire them together:
Task: "Add CurrentPage parameter to AppSidebar.razor"
Task: "Wire current page detection in MainLayout.razor"
```

## Parallel Example: User Story 2

```bash
# Launch all foundation components in parallel:
Task: "Create SidebarSection.razor in src/Ui.Shared/Components/Layout/SidebarSection.razor"
Task: "Create SidebarSection.razor.css in src/Ui.Shared/Components/Layout/SidebarSection.razor.css"
Task: "Create SidebarNavLink.razor in src/Ui.Shared/Components/Layout/SidebarNavLink.razor"

# Then compose:
Task: "Create SidebarQuickLinks.razor using SidebarSection + SidebarNavLink"
Task: "Integrate SidebarQuickLinks into AppSidebar.razor"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001-T002)
2. Complete Phase 2: Foundational (T003-T010)
3. Complete Phase 3: User Story 1 (T011-T016)
4. **STOP and VALIDATE**: Launch app, verify sidebar with user section and current page works, no top-bar
5. Deploy/demo MVP

### Incremental Delivery

1. **MVP** (US1): Sidebar with user avatar + current page label, no top-bar
2. **+US2**: Add collapsible Quick Links for fast navigation
3. **+US3**: Add collapsible Pinned Decks for direct deck access
4. **+US4**: Add responsive behavior for phone/tablet
5. **+Polish**: Accessibility, E2E test selectors, theme consistency

### Suggested MVP Scope

Deliver User Story 1 first. This establishes the sidebar layout shell, removes the top-bar, and provides the user identity + current page orientation. All subsequent features build on this foundation.
