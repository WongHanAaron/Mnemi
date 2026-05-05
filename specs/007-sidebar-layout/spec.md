# Feature Specification: Sidebar App Layout

**Feature Branch**: `007-sidebar-layout`  
**Created**: 2026-05-04  
**Status**: Draft  
**Input**: User description: "Update the overall app layout to match the layout for the Blazor Blueprint music app example (https://blazorblueprintui.com/blueprints/apps/app-music). There should be no top-bar. The side bar should contain 3 sections: the top which shows the user's icon and the page they are currently on, a collapsible section of 'quick links', and a collapsible section of 'pinned decks'. Implement the side bar as a separate sharable component based on ui-architecture-guidelines.md."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Persistent Sidebar Navigation (Priority: P1)

As a student using the flashcard app, I want a persistent sidebar that shows my identity and current location so I always know where I am in the app and can access key navigation options without losing context.

**Why this priority**: The sidebar is the primary navigation and orientation mechanism for the entire app. Without it, users lack consistent wayfinding and the core layout structure is incomplete. All other sidebar features (quick links, pinned decks) depend on this foundation.

**Independent Test**: Can be fully tested by launching the app and verifying that the sidebar is visible on the left side of the screen, displays the user's icon and the name of the current page, and remains visible as the user navigates between pages. Delivers immediate value as the app's primary navigation shell.

**Acceptance Scenarios**:

1. **Given** a user has loaded the application, **When** the app renders, **Then** a sidebar is displayed on the left side containing the user's avatar/icon and the name of the current page at the top.
2. **Given** the user is on the Home page, **When** they navigate to the Review page, **Then** the sidebar updates the current page label to "Review" while keeping the user icon unchanged.
3. **Given** the user is viewing the app on a desktop-width screen, **When** they resize the window, **Then** the sidebar maintains a consistent width and the main content area fills the remaining space.
4. **Given** the app is rendered, **When** the user looks at the screen, **Then** there is no horizontal top-bar across the top of the app; all primary navigation is in the sidebar.

---

### User Story 2 - Collapsible Quick Links Section (Priority: P2)

As a student, I want a collapsible "Quick Links" section in the sidebar so I can quickly jump to frequently used pages like Home, Review, and Settings, and collapse the section when I need more space for pinned decks.

**Why this priority**: Quick links provide fast navigation to core app destinations, reducing the number of clicks to move between primary views. This is the second most important sidebar feature after the user identity section.

**Independent Test**: Can be fully tested by verifying that the "Quick Links" section appears in the sidebar below the user section, contains links to core pages (Home, Review, etc.), and can be expanded/collapsed by clicking the section header. Delivers navigation efficiency even without pinned decks.

**Acceptance Scenarios**:

1. **Given** the sidebar is displayed, **When** the user views the sidebar, **Then** a "Quick Links" section is visible below the user section, with achevron or toggle indicating it can be collapsed.
2. **Given** the "Quick Links" section is expanded, **When** the user clicks the section header or chevron, **Then** the section collapses, hiding the individual quick link items.
3. **Given** the "Quick Links" section is collapsed, **When** the user clicks the section header or chevron, **Then** the section expands, revealing the individual quick link items.
4. **Given** the "Quick Links" section is expanded, **When** the user clicks a quick link item (e.g., "Home"), **Then** the app navigates to that page and the sidebar reflects the new current page.
5. **Given** the user navigates between pages, **When** they return to a page with quick links, **Then** the collapse/expand state of the Quick Links section is preserved during the session.

---

### User Story 3 - Collapsible Pinned Decks Section (Priority: P2)

As a student managing multiple flashcard decks, I want a "Pinned Decks" section in the sidebar where I can see and quickly access my most important decks, and collapse the section when I don't need it.

**Why this priority**: Pinned decks provide direct access to a user's most-used study material from anywhere in the app, significantly reducing navigation friction. This is equally important as quick links for study efficiency.

**Independent Test**: Can be fully tested by pinning one or more decks and verifying they appear in the "Pinned Decks" section in the sidebar, are clickable to navigate directly to that deck, and the section can be expanded/collapsed independently of the Quick Links section.

**Acceptance Scenarios**:

1. **Given** the user has pinned at least one deck, **When** they view the sidebar, **Then** a "Pinned Decks" section is visible below the "Quick Links" section, listing the pinned deck names.
2. **Given** the "Pinned Decks" section is expanded, **When** the user clicks a pinned deck name, **Then** the app navigates to that deck's study view.
3. **Given** the "Pinned Decks" section is expanded, **When** the user clicks the section header or chevron, **Then** the section collapses, hiding the list of pinned decks.
4. **Given** the "Pinned Decks" section is collapsed, **When** the user clicks the section header or chevron, **Then** the section expands, revealing the pinned deck list.
5. **Given** the user has no pinned decks, **When** they view the sidebar, **Then** the "Pinned Decks" section shows an empty state message (e.g., "No pinned decks") or is hidden entirely.
6. **Given** the "Pinned Decks" section is expanded, **When** the user's pinned decks list changes (deck pinned or unpinned from elsewhere in the app), **Then** the sidebar reflects the updated list.

---

### User Story 4 - Responsive Sidebar Behavior (Priority: P3)

As a student using the app on different devices, I want the sidebar to adapt appropriately to narrow screens so that I can still navigate effectively on phones and tablets.

**Why this priority**: While important for mobile users, the core desktop layout must be established first. This ensures the sidebar design doesn't break on smaller screens and follows the project's responsive layout guidelines.

**Independent Test**: Can be fully tested by resizing the browser or emulator to phone and tablet widths and verifying the sidebar either collapses to a hamburger-toggleable overlay or adapts to a bottom navigation pattern, while all sidebar content remains accessible.

**Acceptance Scenarios**:

1. **Given** the user is on a phone-width screen (viewport ≤ 767px), **When** the app renders, **Then** the sidebar is hidden by default and can be revealed via a menu trigger button.
2. **Given** the sidebar is hidden on a narrow screen, **When** the user taps the menu trigger, **Then** the sidebar appears as an overlay or slide-out panel.
3. **Given** the user is on a tablet-width screen (viewport 768px–1023px), **When** the app renders, **Then** the sidebar may display in a compact or collapsed mode, with icons visible and text labels hidden or reduced.
4. **Given** the user is on a desktop-width screen (viewport ≥ 1024px), **When** the app renders, **Then** the sidebar is fully visible with all sections, icons, and labels displayed.

---

### Edge Cases

- What happens when the user has a very long list of pinned decks (e.g., 50+)? The pinned decks list should scroll within its section without pushing other sidebar content off-screen.
- What happens when a pinned deck is deleted from another part of the app while visible in the sidebar? The sidebar should remove it from the pinned list without error.
- What happens when quick links or pinned deck names are very long? Text should truncate with an ellipsis rather than overflow or wrap in a way that breaks the sidebar layout.
- How does the sidebar behave when the user is not authenticated (if authentication is enabled)? The user icon area should show a generic/default avatar and the sidebar sections should still function for navigation.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST display a persistent sidebar on the left side of the application layout on desktop-width screens.
- **FR-002**: System MUST NOT display a horizontal top-bar navigation across the top of the application.
- **FR-003**: The sidebar MUST contain a top section displaying the current user's avatar/icon and the name of the currently active page.
- **FR-004**: The sidebar MUST contain a collapsible "Quick Links" section that provides navigation links to core application pages (at minimum: Home and Review).
- **FR-005**: The "Quick Links" section MUST support expand/collapse toggling that persists state within the user's session.
- **FR-006**: The sidebar MUST contain a collapsible "Pinned Decks" section that lists decks the user has pinned for quick access.
- **FR-007**: The "Pinned Decks" section MUST support expand/collapse toggling independently of the "Quick Links" section.
- **FR-008**: Clicking a pinned deck entry in the sidebar MUST navigate the user to that deck's study view.
- **FR-009**: The "Pinned Decks" section MUST display an appropriate empty state when the user has no pinned decks.
- **FR-010**: The sidebar MUST be implemented as a shared Razor component in the Ui.Components project, following the architecture guidelines in ui-architecture-guidelines.md.
- **FR-011**: The main content area MUST fill the remaining space to the right of the sidebar and scroll independently when content overflows.
- **FR-012**: The sidebar MUST adapt responsively: collapsing to an overlay or toggleable panel on phone-width screens, and optionally displaying in compact mode on tablet-width screens.
- **FR-013**: The current page indicator in the sidebar top section MUST update when the user navigates between pages.
- **FR-014**: Quick link items MUST visually indicate which page is currently active (e.g., highlighted state).

### Key Entities

- **Sidebar**: The primary navigation container displayed on the left side of the app. Contains a user section, quick links section, and pinned decks section. Responsive across viewport sizes.
- **SidebarSection**: A collapsible grouping within the sidebar. Has a header with a label and toggle chevron, and content that can be shown or hidden. Used for both Quick Links and Pinned Decks.
- **QuickLink**: A navigation link to a core application page (e.g., Home, Review). Displayed as a clickable item within the Quick Links section, with an active/inactive visual state.
- **PinnedDeck**: A reference to a user-pinned flashcard deck, displayed as a clickable item within the Pinned Decks section, showing the deck name and navigating to the deck on click.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can identify their current location in the app within 1 second of glancing at the sidebar top section.
- **SC-002**: Users can navigate to any core page (Home, Review) in 2 clicks or fewer from any location in the app using the sidebar.
- **SC-003**: Users can access a pinned deck in 2 clicks or fewer (expand Pinned Decks if collapsed, then click the deck).
- **SC-004**: The sidebar layout renders correctly at desktop (≥1024px), tablet (768px–1023px), and phone (≤767px) viewport widths without horizontal scrolling or content overlap.
- **SC-005**: Collapsing or expanding a sidebar section feels instantaneous (visual feedback within 200ms of click).
- **SC-006**: The sidebar component can be rendered in both the web (Ui.Web) and MAUI (Ui.Maui) host applications without modification to the shared component code.

## Assumptions

- The existing `Ui.Shared` Razor Class Library will host the sidebar component, consistent with the project's architecture guidelines for shared Blazor components.
- User avatar/icon data will be provided by an existing or future authentication service; the sidebar component will accept it as a parameter or injected service rather than sourcing it directly.
- The pinned decks data will be managed by an existing deck management service; the sidebar will consume this data rather than owning it.
- The quick links destinations (Home, Review, etc.) correspond to existing or planned pages in the application.
- The responsive breakpoints (phone ≤ 767px, tablet 768px–1023px, desktop ≥ 1024px) follow the project's existing ViewState/ViewBreakpoints conventions defined in ui-architecture-guidelines.md.
- The sidebar width on desktop will be approximately 240–280px, following common sidebar design patterns, but the exact width will be determined during implementation.
- The sidebar collapse/expand state for each section will be maintained in-memory during the session; persistent state across sessions is out of scope for this feature.
